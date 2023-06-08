# AirCargo

**Usage:**

show-flights                         // shows flight schedule

add-flights --filename flights.json  // add flights to schedule

add-orders --filename orders.json    // dispatch orders to existing flights


Implemented functional requirements:

- Every order is dispatched from Montreal (YUL) airport by default
- You can't schedule flights over fleet capacity
- You can't schedule flights violating unique flight number constraint
- You can't scedule flight with same destination and arrival
- Orders are dispatched by priority. Higher the priority - sooner the flight
- You can't violate unique order id constraint
- File-based multiplatform repository with concurrency control

