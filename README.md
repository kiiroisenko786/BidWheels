## Running project locally
To run this project, first clone the repo, then, in the project directory, run (in windows terminal/powershell)

This will install any prerequisite packages and initialize the microservices docker containers 


`npm install`, followed by `docker compose up -d`

Once the docker container services have started running and been initialized, a restart may be required in order for the microservices to connect properly (see known issues)

In a terminal, navigate to BidWheels/frontend/web-app by using `cd frontend/web-app`

From there, run `npm run` to start the client app, which will be hosted on `localhost:3000` from which it can be accessed.

The database will already come seeded with some example auctions to serve as demos.

## Known issues
Depending on the system, some services can load faster than others, resulting in errors as the services try to connect to rabbitmq or one of the dbs.

Despite the dependencies in the docker-compose.yml, this issue persists, the only found solution is to manually restart each service after the dbs have started and initialized to ensure they properly connect.
