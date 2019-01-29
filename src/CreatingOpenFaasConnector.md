# Creating your own OpenFaas Connector

[OpenFaas](https://www.openfaas.com/) is a serverless platform which can be deployed on Docker Swarm or a kubernetes cluster. A nice introduction to OpenFaas can be found in this [post](https://blog.alexellis.io/quickstart-openfaas-cli/) by [Alex](https://twitter.com/alexellisuk), founder of OpenFaas.

This post is going to be about how to create your own OpenFaas connector to add new functionality to your deployment. There are some connectors already being worked on which you can find in the [docs](https://docs.openfaas.com/reference/triggers/).

## Introduction

Serverless is a restrictive model, you can only work within the framework provided and extending functionality is almost impossible, in most cases. OpenFaas differs in this respect as from an architecture standpoint, it is just a collection of containers talking with each other through REST. A connector can be viewed as a container running in the cluster alongside other OpenFaas components, and interacting with them.

Any long running task that waits for an external event can be abstracted away as a connector. This includes, but is not limited to, listening to messages in a queueing service, listening to events in real time database, a cron service and maybe even a websocket connection.

## Function Annotations and OpenFaas Gateway

Functions deployed on OpenFaas can have annotations, which are a list of key-value pairs to assign extra data to functions. You can set function annotations by specifying them in the yaml file or giving an `--annotations` flag to faas-cli. The functions can be queried through OpenFaas gateway's REST api: `GET /system/functions`. It returns an output something like this:

```javascript
[
  {
    "name": "fn1",
    "image": "functions/nodeinfo:latest-armhf",
    "invocationCount": 56,
    "replicas": 1,
    "envProcess": "",
    "availableReplicas": 0,
    "labels": {
      "com.openfaas.function": "fn1",
      "function": "true"
    },
    "annotations": {
      "schedule": "* * * * *",
      "topic": "cron-function"
    }
  },
  {
    "name": "password-gen",
    "image": "192.168.1.9:5000/passwd-gen:latest",
    "invocationCount": 24,
    "replicas": 1,
    "envProcess": "python index.py",
    "availableReplicas": 0,
    "labels": {
      "com.openfaas.function": "password-gen",
      "com.openfaas.uid": "146101163",
      "function": "true"
    },
    "annotations": null
  },
  {
    "name": "fn2",
    "image": "functions/nodeinfo:latest-armhf",
    "invocationCount": 55,
    "replicas": 1,
    "envProcess": "",
    "availableReplicas": 0,
    "labels": {
      "com.openfaas.function": "fn2",
      "function": "true"
    },
    "annotations": {
      "schedule": "* * * * *",
      "topic": "cron-function"
    }
  }
]
```

Some function annotations have a "topic" key, which is the preferred way of telling a connector "you'll need this function", the connector can then invoke that function providing it the required data.

## Connector SDK

OpenFaas has a connector sdk for fetching a list of functions and invoking functions with a set topic. A dead simple connector that invokes functions every 2 seconds:

```go
package main

import (
  "errors"
  "os"
  "time"

  "github.com/openfaas-incubator/connector-sdk/types"
)

func main() {
  creds := types.GetCredentials()
  gURL, _ := os.LookupEnv("gateway_url")
  config := &types.ControllerConfig{
    RebuildInterval: time.Millisecond * 1000,
    GatewayURL:      gURL,
    PrintResponse:   true,
  }

  controller := types.NewController(creds, config)
  controller.BeginMapBuilder()

  topic := "faas-request"
  invokeTime := time.Second * 2

  for {
    time.Sleep(invokeTime)
    data := []byte("test " + time.Now().String())
    controller.Invoke(topic, &data)
  }
}
```

## Package and deploy

Now, lets get to packaging the connector in a container. The steps involved are:

- Create a vendor directory

This is a fairly easy step, you'll need to install [dep](https://golang.github.io/dep/) and then run `dep init && dep ensure` to create a vendor folder.

- Create a Dockerfile

This will be a multi stage build Dockerfile. We'll first compile the program at build stage and then copy the executable to alpine. Sample dockerfile looks like the following.

```bash
FROM golang:1.10.4 as builder
RUN mkdir -p /go/src/github.com/github_username/connector
WORKDIR /go/src/github.com/gihub_username/connector

COPY vendor     vendor
COPY main.go    .

# Run a gofmt and exclude all vendored code.
RUN test -z "$(gofmt -l $(find . -type f -name '*.go' -not -path "./vendor/*"))"

RUN go test -v ./...

# Stripping via -ldflags "-s -w" 
RUN CGO_ENABLED=0 GOOS=linux go build -a -ldflags "-s -w" -installsuffix cgo -o ./connector

FROM alpine:3.8

RUN addgroup -S app \
    && adduser -S -g app app

WORKDIR /home/app

COPY --from=builder /go/src/github.com/github_username/connector/    .

RUN chown -R app:app ./

USER app

CMD ["./connector"]
```

There isn't much you would have to change to adapt it for your connector. Now, build it and push it to your registry: `docker build -t docker_hub_id/connector . && docker push docker_hub_id/connector`

- Create docker-compose.yml and kubernetes deployment files

The last part involves creating yaml files for Docker Swarm and/or Kubernetes. The bare essentials that need to be provided to container are gateway url and login credentials. Thankfully, since we are deploying to the same stack in swarm or to the same namespace in kubernetes, accessing these values is easy.

docker-compose.yml:

```yaml
version: '3.2'
services:
  connector:
    image: docker_hub_id/connector
    hostname: sample-connector
    environment:
      gateway_url: http://gateway:8080
      basic_auth: "true"
      secret_mount_path: "/run/secrets/"
    secrets:
      - basic-auth-password
      - basic-auth-user
    networks:
      - func_functions

networks:
  func_functions:
    external: true

secrets:
    basic-auth-user:
        external: true
    basic-auth-password:
        external: true
```

kubernetes/connector-dep.yml:

```yaml
apiVersion: extensions/v1beta1
kind: Deployment
metadata:
  labels:
    app: connector
    component: sample-connector
  name: sample-connector
  namespace: openfaas
spec:
  replicas: 1
  template:
    metadata:
      labels:
        app: connector
        component: sample-connector
    spec:
      containers:
      - name: sample-connector
        image: docker_hub_id/connector
        env:
          - name: gateway_url
            value: "http://gateway.openfaas:8080"
          - name: basic_auth
            value: "true"
          - name: secret_mount_path
            value: "/var/secrets/"
        volumeMounts:
            - name: auth
              readOnly: true
              mountPath: "/var/secrets/"
      volumes:
      - name: auth
        secret:
          secretName: basic-auth
```

- Test these out

If you've pushed the image to docker hub you only need to give a command to swarm or kubernetes to deploy your container.

```bash
# Docker swarm
docker stack deploy func -c ./docker-compose.yml

# kubernetes
kubectl create -f ./kubernetes --namespace openfaas
```

Deploy a test function to openfaas using the command:

```bash
faas-cli deploy --image=functions/nodeinfo --name=trigger-func --annotation topic=faas-request
```

In the OpenFaas portal you'll see the function named `trigger-func` and its invocation count incrementing every 2 seconds.

## More examples and conclusion

OpenFaas connector pattern spawned from [kafka-connector](https://github.com/openfaas-incubator/kafka-connector) to add new functionality to existing OpenFaas deployment without changing OpenFaas source code. I dwelled into connectors so that I can build a timer trigger for OpenFaas since this functionality is not currently present. You can checkout cron connector [repo](https://github.com/zeerorg/cron-connector) and even build your own awesome connector.
