# BookStore APP

Application configuration is provided via environment variables:
- `MONGO_HOST`: MongoDB url
- `MONGO_PORT`: MongoDB port
- `MONGO_USER`: MongoDB user
- `MONGO_PASS`: MongoDB password
- `MONGO_DB_NAME`: MongoDB database name
- `MONGO_DB_COLLECTION`: Mongo collection for Books



## Run locally

```sh
# Start mongoDB
podman run -d \
  -p 27017:27017 \
  -e MONGO_INITDB_ROOT_USERNAME=admin \
  -e MONGO_INITDB_ROOT_PASSWORD=pass \
  --name mongo-bookstore \
  mongo

# Create database
podman cp ./mongo-data.js mongo-bookstore:/
podman exec mongo-bookstore sh -c "mongosh -u admin -p pass < /mongo-data.js"

# Setup configuration
export MONGO_HOST=localhost
export MONGO_PORT=27017
export MONGO_USER=admin
export MONGO_PASS=pass
export MONGO_DB_NAME=BookStore
export MONGO_DB_COLLECTION=Books

# Runn application
dotnet restore
dotnet run
```