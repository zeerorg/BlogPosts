# k3d - A fast kubernetes dev environment

There are a number of kubernetes dev environments that help develop and test applications created for kubernetes, but each one of them suffers from some issue. Let's take a look at what we want from a kubernetes dev environment.

1. Fast to start
2. Lightweight on resources
3. Retains state on restarts
4. Easy to reset
5. Cross Platform

## Problems with current solutions

Current solutions don't check all of these requirements, we'll see some of them starting with Minikube - the most popular and the officially suggested way of running kubernetes locally. Minikube creates a linux virtual machine which acts as a kubernetes node the client can connect to. Since it relies on a vm it is slow to start and eats up a lot of resources. If you are on a non-linux host and also running Docker (which is a pretty common scenario when working with k8s) then you'll be running 2 virtual machines on your desktop for working with kubernetes.

Microk8s is another popular dev environment, it uses containerd instead of docker as a container host and runs natively on the host machine, this means it is fast to start and lightweight compared to minikube. But it is not cross-platform which is a major bummer.

KinD (Kubernetes in Docker) is a relatively newer project in this space, which, as its name suggests allows you to run kubernetes in a docker container. It works by starting systemd as a process coordinator in a docker container and then starting relevant services like docker and other kubernetes components. But KinD resets its state on every restart and takes a few minutes to start.

Another dev environment is Docker for Mac / Windows which ships with kubernetes. I've used Docker for Windows and I constantly ran into issues with kubernetes where kubectl randomly froze when I tried to deploy more than 10 containers.

## Introducing k3d (k3s with docker)

This is a tool I developed to address the previous points. It is based on a [k3s](https://k3s.io/) which is a lightweight kubernetes distribution. It is fast to start and has a minimal resource footprint. k3d wraps it all in a nice CLI with sane defaults. It is cross-platform and runs on Windows, Mac OS, and Linux and supports creating multiple clusters.

Let's look at a normal workflow:

1. Creating a kubernetes cluster: `k3d create`
   
   ![kubernetes create cluster](https://zeerorgprocessedblog.blob.core.windows.net/photos/k3d-create.png)

2. Set KUBECONFIG and get running pods: `export KUBECONFIG="$(k3d get-kubeconfig)" && kubectl get pods --all-namespaces`
   
   ![kubectl get pods](https://zeerorgprocessedblog.blob.core.windows.net/photos/k3d-run.png)

3. You can now restart your computer or docker and the cluster would still be present. You can delete the cluster by: `k3d delete`

   ![kubernetes delete cluster](https://zeerorgprocessedblog.blob.core.windows.net/photos/k3d-delete.png)

## Installing k3d

k3d is written in rust, the github repository is [here](https://github.com/zeerorg/k3s-in-docker). You can install the tool by:

1. Run a single install script: `curl -sSL https://raw.githubusercontent.com/zeerorg/k3s-in-docker/master/install-script.sh | sudo sh -`
2. Download latest release from github [releases](https://github.com/zeerorg/k3s-in-docker/releases).
3. If you have rust toolchain then you can install it through cargo with: `cargo install k3d`

## Future Updates

One feature that I would love to have is a local registry for pushing and pulling images. Currently, microk8s supports this and I am hopeful it would be possible in some weeks as the PR [#248](https://github.com/rancher/k3s/pull/248) gets merged in k3s. This would thoroughly improve the developer experience.

Also, since k3s is cross-architecture it should be theoretically possible to run kubernetes with different target architecture (armv7 or arm64) on an x86 host machine which has binfmt_misc support enabled in the kernel, helping in developing multi-architecture development.

## Concluding points

I'm really thankful to [rancher](https://rancher.com/) and Darren Shepherd ([@ibuildthecloud](https://twitter.com/ibuildthecloud)) for developing k3s, which is a really useful piece of software.
