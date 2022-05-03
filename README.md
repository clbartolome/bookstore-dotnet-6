# bookstore-dotnet-6
Bookstore project using .Net Core or 6.

## GitOps

### Configure CICD environment

- Create namespace for CICD resources: 
  ```sh
  # CICD Resources
  oc new-project cicd-resources
  ```

- Install Nexus and deploy mongo helm chart: 
  ```sh
  # Apply resources
  oc apply -f cicd-resources/nexus.yaml -n cicd-resources
  # Get route (pass in /nexus-data/admin.password)
  export NEXUS_URL=$(oc get route nexus -o jsonpath='{.spec.host}' -n cicd-resources)
  # Wait until pod is running 
  oc get pods -w -n cicd-resources

  # Get admin Password
  export NEXUS_PASS=$(oc exec deploy/nexus -- cat /nexus-data/admin.password)

  # Create nexus repository for helm charts if needed
  curl -v http://$NEXUS_URL/service/rest/v1/repositories/helm/hosted \
  -u admin:$NEXUS_PASS \
  -X POST \
  -d '{"name":"helm","online":true,"storage":{"blobStoreName":"default","strictContentTypeValidation":true,"writePolicy":"allow_once"},"cleanup":{"policyNames":["string"]},"component":{"proprietaryComponents":true}}  ' \
  -H "Content-Type: application/json"

  # Package heml chart
  helm package mongodb/ --destination mongodb/charts
  # Uplopad helm chart
  curl -v -u admin:$NEXUS_PASS http://$NEXUS_URL/repository/helm/ --upload-file mongodb/charts/mongodb-1.0.0.tgz
  ```

- Install ArgoCD:

  Follow [this instructions](https://docs.openshift.com/container-platform/4.8/cicd/gitops/installing-openshift-gitops.html).

  Get URL and Password:
  ```sh
  # Get Password
  oc get secret openshift-gitops-cluster -n openshift-gitops -ojsonpath='{.data.admin\.password}' | base64 -d
  # Get URL
  oc get route openshift-gitops-server -ojsonpath='{.spec.host}' -n openshift-gitops

  # Apply argo CD cluster configuration (including application namespaces)
  oc apply -f cicd-resources/argo/init.yaml

  # Add dotnet 6 is for S2I process
  oc apply -f https://raw.githubusercontent.com/redhat-developer/s2i-dotnetcore/master/dotnet_imagestreams.json -n openshift

  # Create Argo CD APP
  # !!!! Before that, update nexus URL in charts (and commit changes): gitops/bookstore-mongo/**/Chart.yaml
  oc apply -f cicd-resources/argo/bookstore.yaml

  # Allow acces to dev image and Start build manually
  oc policy add-role-to-user system:image-puller system:serviceaccount:book-store-prod:default -n book-store-dev  
  oc start-build book-store-dev -n book-store-dev  
  ```

- Configure Pipelines:
  ```sh
  # Configure policies
  oc policy add-role-to-user edit system:serviceaccount:$cicd-resources:pipeline -n book-store-dev
  oc policy add-role-to-user edit system:serviceaccount:cicd-resources:pipeline -n book-store-prod
  oc policy add-role-to-user system:image-puller system:serviceaccount:book-store-dev:default -n cicd-resources
  oc policy add-role-to-user system:image-puller system:serviceaccount:book-store-prod:default -n cicd-resources

  ```

## OpenShift simple deployment

- Create a namespace: `oc new-project bookstore`
- Deploy a MongoDB database:
```sh
# Open new terminal and clone this repository
cd mongodb

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


