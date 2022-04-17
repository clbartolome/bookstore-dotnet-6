# BookStore APP

## Run locally

```sh
# Create a Network for mongo and mongo express
podman network create mongo-express-network

# Start mongoDB
podman run -d \
  -p 27017:27017 \
  -e MONGO_INITDB_ROOT_USERNAME=user \
  -e MONGO_INITDB_ROOT_PASSWORD=pass \
  --name mongo-payments-server \
  --net mongo-express-network \
  mongo
```