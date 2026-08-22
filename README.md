# Play.Trading
Service to process trades and orchestrate saga

## Building app

```bash
dotnet build
```

## Running app

```bash
dotnet run
```

## Running MongoDB with localhost volume

```bash
docker run -d --rm --name mongo -p 27017:27017 -v mongodbdata:/data/db mongo
```


## Add reference to exported Common library

```bash
dotnet add package Play.Common
```

## Build the docker image

```bash
export GH_OWNER="Play-Microservices"
export GH_USERNAME="[USERNAME HERE]"
export GH_PAT="[PAT HERE]"
version="1.0.3"
appname="playmicroservices"
docker build \
  --secret id=GH_OWNER \
  --secret id=GH_USERNAME \
  --secret id=GH_PAT \
  -t "$appname.azurecr.io/play.trading:$version" .
```

## Run the docker image

```bash
version="1.0.3"
appname="playmicroservices"
cosmosDbConnString="[CONN STRING HERE]"
serviceBusConnString="[CONN STRING HERE]"
docker run -it --rm \
  -p 5006:8080 \
  --name trading \
  -e MongoDbSettings__ConnectionString=$cosmosDbConnString \
  -e ServiceBusSettings__ConnectionString=$serviceBusConnString \
  -e ServiceSettings__MessageBroker="SERVICEBUS" \
  "$appname.azurecr.io/play.trading:$version"
```

### Publish the Docker image

Login to Azure Container Registry:

```bash
az acr login --name "$appname"
```

Push the Docker image:

```bash
docker push "$appname.azurecr.io/play.trading:$version"
```

### Create Kubernetes namespace with secrets

```bash
namespace=trading
```

Create the namespace:

```bash
kubectl create namespace "$namespace"
```

### Create Kubernetes pod

Deploy the Identity service:

```bash
kubectl apply -f ./kubernetes/trading.yaml -n "$namespace"
```

Check the pods:

```bash
kubectl get pods -n "$namespace"
```

Set the pod name:

```bash
tradingPod="[TRADING POD NAME]"
```

Check the pod logs:

```bash
kubectl logs "$tradingPod" -n "$namespace"
```

Describe the pod:

```bash
kubectl describe pod "$tradingPod" -n "$namespace"
```

Check the services:

```bash
kubectl get services -n "$namespace"
```

### Creating the Azure Managed Identity and granting it access to Key Vault secrets

Create identity:

```bash
az identity create \
  --resource-group $appname \
  --name $namespace
```

Get identity client ID:

```bash
TRADING_CLIENT_ID=$(az identity show \
  -g "$appname" \
  -n "$namespace" \
  --query clientId \
  -o tsv)
```

Set GET/LIST policy for new identity to Key Vault:

```bash
az keyvault set-policy \
  -n $appname \
  --secret-permissions get list \
  --spn $TRADING_CLIENT_ID
```

For RBAC Key Vault use this instead:

```bash
KEYVAULT_ID=$(az keyvault show \
  -n "$appname" \
  -g "$appname" \
  --query id \
  -o tsv)

az role assignment create \
  --assignee "$TRADING_CLIENT_ID" \
  --role "Key Vault Secrets User" \
  --scope "$KEYVAULT_ID"
```

### Establish the federated identity credential

```bash
AKS_OIDC_ISSUER="$(az aks show \
  --name "$appname" \
  --resource-group "$appname" \
  --query "oidcIssuerProfile.issuerUrl" \
  --output tsv)"
  
az identity federated-credential create \
    --name $namespace \
    --identity-name "$namespace" \
    --resource-group "$appname" \
    --issuer "$AKS_OIDC_ISSUER" \
    --subject system:serviceaccount:"$namespace":"$namespace-serviceaccount" \
    --audience api://AzureADTokenExchange
```