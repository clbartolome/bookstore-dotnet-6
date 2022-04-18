# bookstore-dotnet-6
Bookstore project using .Net Core or 6.

## OpenShift simple deployment

- Create a namespace: `oc new-project bookstore`
- Deploy a MongoDB database. Can use this helm chart:
```sh
# Open new terminal and clone this repository
git clone git@github.com:clbartolome/helm-charts.git
cd helm-charts/mongodb

helm install book-store . --set-string credentials.username=user,credentials.userpassword=pass,openshiftApplicationName=book-store
```
- Deploy application
```sh
# Review dotnet IS and update it in case there is no 6.0 version -> oc create -f https://raw.githubusercontent.com/redhat-developer/s2i-dotnetcore/master/dotnet_imagestreams.json
oc new-app \
  --name book-store \
  dotnet:6.0-ubi8~https://github.com/clbartolome/bookstore-dotnet-6#master \
  --context-dir app \
  -e MONGO_HOST=book-store-mongodb \
  -e MONGO_PORT=27017 \
  -e MONGO_USER=user \
  -e MONGO_PASS=pass \
  -e MONGO_DB_NAME=BookStore \
  -e MONGO_DB_COLLECTION=Books

```


