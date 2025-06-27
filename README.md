## Functionality and Operation
Due to the architecture of this project, there is unfortunately no appropriate hosting platform with a free tier to host this project and its microservices either dockerized or as a kubernetes cluster.

In order to still provide a demonstration of the operation of this project, gifs highlighting the key functionality of the web application will be uploaded onto this README.

## Known issues
Depending on the system, some services can load faster than others, resulting in errors as the services try to connect to rabbitmq or one of the dbs.

Despite the dependencies in the docker-compose.yml, this issue persists, the only found solution is to manually restart each service after the dbs have started and initialized to ensure they properly connect.
