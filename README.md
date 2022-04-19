# bookstore-dotnet-6
Bookstore project using .Net Core or 6.

## GitOps

### Configure CICD environment

- Create CICD resources namespace: `oc new-project cicd-resources`

- Install ArgoCD operator:

  Follow [this instructions](https://docs.openshift.com/container-platform/4.8/cicd/gitops/installing-openshift-gitops.html).
  Retrieve the user 'admin' password:
  ```sh
  oc get secret openshift-gitops-cluster -n openshift-gitops -ojsonpath='{.data.admin\.password}' | base64 -d
  ```
  Retrieve ArgoCD URL:
  ```sh
  oc get route openshift-gitops-server -ojsonpath='{.spec.host}' -n openshift-gitops
  ```

- Install nexus: 
  ```sh
  # Apply resources
  oc apply -f cicd-resources/nexus.yaml -n cicd-resources
  # Get route (admin/admin123)
  oc get route nexus -o jsonpath='{.spec.host}' -n cicd-resources
  ```





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
  -e MONGO_DB_COLLECTION=Books \
  -n bookstore

# Labels
oc label deploy book-store \
  app.kubernetes.io/part-of=book-store \
  app.openshift.io/runtime=dotnet \
  -n bookstore

# Annotations
oc annotate deploy book-store app.openshift.io/connects-to='[{"apiVersion":"apps/v1","kind":"Deployment","name":"book-store-mongodb"}]' -n bookstore


# Expose service
oc expose svc book-store -n bookstore
```

- Test App:
```sh
# Get URL
export APP_URL=$(oc get route book-store -o jsonpath='{.spec.host}' -n bookstore)

# Create two Books
curl http://$APP_URL/api/books \
  -X POST \
  -d '{"bookName": "The Lord of the Rings. The Fellowship of the Ring","price": 43.15,"category": "Fantasy","author": "J. R. R. Tolkien"}' \
  -H "Content-Type: application/json"
curl http://$APP_URL/api/books \
  -X POST \
  -d '{"bookName": "The Accursed Legions","price": 25.30,"category": "Historical","author": "Santiago Posteguillo"}' \
  -H "Content-Type: application/json"

# Retrieve Books
curl http://$APP_URL/api/books
```


