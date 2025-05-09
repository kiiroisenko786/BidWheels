
## Known issues
Depending on the system, some services can load faster than others, resulting in errors as the services try to connect to rabbitmq or one of the dbs.

Despite the dependencies in the docker-compose.yml, this issue persists, the only found solution is to manually restart each service after the dbs have started and initialized to ensure they properly connect.
