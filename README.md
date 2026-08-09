# Play.Trading
Service to process trades and orchestrate saga

## Building app
dotnet build

## Running app
dotnet run

## Running MongoDB with localhost volume
docker run -d --rm --name mongo -p 27017:27017 -v mongodbdata:/data/db mongo

## Add reference to exported Common library
dotnet add package Play.Common

## Build the docker image

```powershell
$env:GH_OWNER="Play-Microservices"
$env:GH_USERNAME="[USERNAME HERE]"
$env:GH_PAT="[PAT HERE]"
$version="1.0.0"
docker build --secret id=GH_USERNAME --secret id=GH_OWNER --secret id=GH_PAT -t play.trading:$version .
```

```bash
export GH_OWNER="Play-Microservices"
export GH_USERNAME="[USERNAME HERE]"
export GH_PAT="[PAT HERE]"
version="1.0.0"

docker build \
  --secret id=GH_OWNER \
  --secret id=GH_USERNAME \
  --secret id=GH_PAT \
  -t play.trading:$version .
```

## Run the docker image

```powershell
version="1.0.2"
docker run -it -=rm -p 5000:8080 --name catalog -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq -e --network playinfrastructure_default play.trading:$version
```

```bash
version="1.0.2"
docker run -it --rm \
  -p 5006:8080 \
  --name catalog \
  -e MongoDbSettings__Host=mongo \
  -e RabbitMQSettings__Host=rabbitmq \
  --network playinfrastructure_default \
  "play.trading:$version"
```