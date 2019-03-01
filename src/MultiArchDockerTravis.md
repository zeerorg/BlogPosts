# Build Multi-Arch docker images on Travis

A lot of people are shipping applications as docker images so that they can run in a container. Docker has first class support for multiple architectures and if you are trying to add support for different platforms to your app, you might have come across the problem of automating this process. The pain point is the fact that most hosted CI services like travis and circle only provide amd64 environment.

But some projects do ship out constantly updated docker images for different architectures and they don't do so manually. Let's take a look at the official docker images for golang ([repo](https://github.com/docker-library/golang)), in the README you'll see that they use Travis, Appveyor and Jenkins. Jenkins is used to build arm images, but it is a self hosted service and hence not our goal. Another approach is using [drone cloud](https://cloud.drone.io/) as a ci service, since they have arm servers which can be used to build docker images. But if you don't want to change the current CI service you __were__ out of luck.

## You don't need physical ARM servers

You can run a program compiled for ARM on amd64 linux machine if it has [`binfmt_misc`](https://www.kernel.org/doc/html/v4.18/admin-guide/binfmt-misc.html) support. It essentially allows you to run a program without worrying about which architecture it was built for. `binfmt_misc` doesn't come by default in ubuntu, but adding it is really easy using docker.

1. Set `dist: xenial` to `.travis.yml` since I've found that binfmt_misc mount, errors out on ubuntu precise which is the default travis distro.
2. Add the following block to travis config:

   ```yaml
   before_install:
     - sudo docker run --privileged linuxkit/binfmt:v0.6
   ```

   This single command mounts binfmt_misc to the os `/proc/sys/fs` directory. Now our build server has superpowers.

## Using Buildkit for building docker images

[Buildkit](https://github.com/moby/buildkit) is a toolkit for building container images, it is used in docker and many other places and is advertised to be quite fast. It has a client-server architecture and you'll need to start the server and then use the client to interact with it. There is a fantastic [demo](https://asciinema.org/a/GYOx4B88r272HWrLTyFwo156s) on building multi-arch docker images by [Tõnis Tiigi](https://github.com/tonistiigi).

Our process will goes like this, first we'll start the buildkit server as a container process, then we'll copy the `buildctl` binary which is the command line frontend for buildkit to our `/usr/bin` directory and then we'll set `BUILDKIT_HOST` environment variable. Finally, before_install block will look like this:

```yaml
before_install:
  - sudo docker run --privileged linuxkit/binfmt:v0.6
  - sudo docker run -d --privileged -p 1234:1234 --name buildkit moby/buildkit:latest --addr tcp://0.0.0.0:1234 --oci-worker-platform linux/amd64 --oci-worker-platform linux/armhf
  - sudo docker cp buildkit:/usr/bin/buildctl /usr/bin/
  - export BUILDKIT_HOST=tcp://0.0.0.0:1234
```

We specify `--addr` to buildkit server to bind it to the specified port and address, we also give `--oci-worker-platform` argument to tell the buildkit server that our machine can be used to create images for these platforms.

## Building Images

We can now finally write the script to build images.

```bash
PLATFORM=arm # equivalent to armhf
DOCKERFILE_LOCATION="./Dockerfile.armhf"
DOCKER_USER="someone"
DOCKER_IMAGE="some_server"
DOCKER_TAG="latest"

buildctl build --frontend dockerfile.v0 \
        --frontend-opt platform=linux/${PLATFORM} \
        --frontend-opt filename=./${DOCKERFILE_LOCATION} \
        --exporter image \
        --exporter-opt name=docker.io/${DOCKER_USER}/${IMAGE}:${TAG}-${PLATFORM} \
        --exporter-opt push=true \
        --local dockerfile=. \
        --local context=.
```

This command has a few options, we'll go over them one by one

1. `--frontend dockerfile.v0` specifies which frontend to use with buildkit, there is also a `gateway.v0` frontend, but we'll stick with dockerfile.
2. `--frontend-opt`, this is used to pass parameters to the fronend used.
   * `platform` specifies the platform we want to build the image for, these are listed [here](https://github.com/containerd/containerd/blob/a69a0b0192f647aff8730e493f2da622eb0fd13d/platforms/platforms.go#L88).
   * `filename` specifies the filename of Dockerfile to be used equivalent to docker build's `-f` argument.
3. `--exporter image` specifies the export plugin to be used, there are also `oci` and `local`.
   * `name` is the url of the registry to push docker images to.
   * `push` can be set to `true` to push the built docker images.

## Pushing manifest - Final Step to Multi-Arch

A container manifest is a file that contains data about a container image. We can create a manifest which points to images for different architectures so that when using the image on a particular architecture the docker automatically pulls the desired image.

<img src="https://zeerorgprocessedblog.blob.core.windows.net/photos/container-manifest.png" alt= "Kubernetes Setting" style="width:50%; margin-left: auto; margin-right: auto; display: block;"/>

Creating manifest is an experimental Docker-cli feature and you should update docker for good measure. Add following lines to .travis.yml

```yaml
addons:
  apt:
    packages:
      - docker-ce
```

First we create the manifest, then we annotate the manifest and finally do a push. For this example let us take the user to be "someone" and docker image to be "my-image" with the tag "latest" i.e the image is "someone/my-image:latest", PLATFORM_1 and PLATFORM_2 are two different platforms for which we need to create the multi arch image. The commands look like this:

```bash
export DOCKER_CLI_EXPERIMENTAL=enabled

docker manifest create someone/my-image:latest \
            someone/my-image:latest-${PLATFORM_1} \
            someone/my-image:latest-${PLATFORM_2}

docker manifest annotate someone/my-image:latest someone/my-image:latest-${PLATFORM_1} --arch ${PLATFORM_1}
docker manifest annotate someone/my-image:latest someone/my-image:latest-${PLATFORM_2} --arch ${PLATFORM_2}

docker manifest push someone/my-image:latest
```

Now you have successfully automated the creation and push of multiarch docker images.

## Concluding points

I added multi-arch support to [cron-connector](https://github.com/zeerorg/cron-connector) and was able to remove armhf specific config files as well as automate the pushing of armhf images. You can find my commit [here](https://github.com/zeerorg/cron-connector/commit/9c219366fd1898c9a7749fda55b0d36cf6a67e15). Also, mad props to [Johannes Würbach](https://github.com/johanneswuerbach) who's Grafana PR [#14617](https://github.com/grafana/grafana/pull/14617) was where I picked up the `binfmt_misc` part and to buildkit team for creating a really nice piece of software.