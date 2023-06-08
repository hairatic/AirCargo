using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AirCargo.CLI;
using AirCargo.Infrastructure;

namespace AirCargo.Services
{
    /// <summary>
    /// Cross-platform class for inter-process access to the file repo
    /// I guess I got carried away with this one a little...
    /// </summary>
    public class FileAccessService
    {
        private const string AppGuid = "900ee030-be92-4676-9cf8-7d155d99df45";
        private const int UiTimeoutMs = 200;

        private const string SemaphoreId = $"Global\\{{{AppGuid}}}";
        private static readonly Semaphore Semaphore = new(1, 1, SemaphoreId);

        public async Task<Result<string>> ReadFileAsync(string repoName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return await ReadFileLockAsync(repoName);
            else
                return await ReadFileSemaphoreAsync(repoName);
        }

        public async Task<Result<bool>> WriteFileAsync(string repoName, string content)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return await WriteFileLockAsync(repoName, content);
            else
                return await WriteFileSemaphoreAsync(repoName, content);
        }

         async Task<Result<string>> ReadFileSemaphoreAsync(string repoName)
         { 
             var result = new Result<string>();

            var resourceLocked = false;
            try
            {
                resourceLocked = Semaphore.WaitOne(UiTimeoutMs, false);
                if (resourceLocked)
                {
                    string directory = InitWorkingDirectory();
                    string filePath = Path.Combine(directory, repoName);

                    if (!File.Exists(filePath))
                    {
                        result.IsSuccessful = true;
                        result.Value = string.Empty;
                        return result;
                    }
                    else
                    {
                        string text = await File.ReadAllTextAsync(filePath);
                        result.IsSuccessful = true;
                        result.Value = text;
                        return result;
                    }
                }
                else
                {
                    result.IsSuccessful = false;
                    result.Errors.Add(UserMessages.RepositoryIsBusy);
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.Errors.Add(UserMessages.RepositoryError);
                result.Errors.Add(ex.Message);
                return result;
            }
            finally
            {
                if (resourceLocked)
                    Semaphore.Release();
            }
        }

         async Task<Result<bool>> WriteFileSemaphoreAsync(string repoName, string fileContents)
         {
             var result = new Result<bool>();
             var resourceLocked = false;
             try
             {
                resourceLocked = Semaphore.WaitOne(UiTimeoutMs, false);
                if (resourceLocked)
                {
                    string directory = InitWorkingDirectory();
                    string filePath = Path.Combine(directory, repoName);

                    await File.WriteAllTextAsync(filePath, fileContents);

                    result.IsSuccessful = true;
                    result.Value = true;
                    return result;
                }
                else
                {
                    result.IsSuccessful = false;
                    result.Errors.Add(UserMessages.RepositoryIsBusy);
                    return result;
                }
             }
             catch (Exception ex)
             {
                 result.IsSuccessful = false;
                 result.Errors.Add(UserMessages.RepositoryError);
                 result.Errors.Add(ex.Message);
                 return result;
             }
             finally
             {
                if (resourceLocked)
                    Semaphore.Release();
             }
        }

        // no semaphore for Linux
        // only shared lock under the sakura
        // samurai sobs

        async Task<Result<string>> ReadFileLockAsync(string repoName)
        {
            var result = new Result<string>();

            string directory = InitWorkingDirectory();
            string filePath = Path.Combine(directory, repoName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.OpenOrCreate,
                                                             FileAccess.ReadWrite,
                                                             FileShare.None))
                {
                    using var sr = new StreamReader(stream, Encoding.UTF8);
                    string content = await sr.ReadToEndAsync();

                    result.IsSuccessful = true;
                    result.Value = content;
                    return result;
                }
            }
            catch (IOException ex)
            {
                result.IsSuccessful = false;
                result.Errors.Add(UserMessages.RepositoryIsBusy);
                result.Errors.Add(ex.Message);
                return result;
            }
        }

        async Task<Result<bool>> WriteFileLockAsync(string repoName, string content)
        {
            var result = new Result<bool>();

            string directory = InitWorkingDirectory();
            string filePath = Path.Combine(directory, repoName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.OpenOrCreate,
                                                             FileAccess.ReadWrite,
                                                             FileShare.None))
                {
                    using var sw = new StreamWriter(stream);
                    stream.SetLength(0);
                    await sw.WriteAsync(content);
                }

                result.IsSuccessful = true;
                result.Value = true;
                return result;
            }
            catch (IOException e)
            {
                result.IsSuccessful = false;
                result.Errors.Add(UserMessages.RepositoryIsBusy);
                return result;
            }
        }

        string InitWorkingDirectory()
        {
            string path = Directory.GetCurrentDirectory();
            string repoDirectoryPath = Path.Combine(path, "repo");
            if (!Directory.Exists(repoDirectoryPath))
                Directory.CreateDirectory(repoDirectoryPath);

            return repoDirectoryPath;
        }
    }
}
