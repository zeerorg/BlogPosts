# Develop with kubernetes on Docker Desktop with WSL

Docker Desktop for Windows comes shipped with kubernetes out of the box. It preconfigures a lot of stuff on your Windows machine, like downloads kubectl and puts everything in PATH variable, so that it can be accessed from powershell. But a lot of developers have took a liking to WSL which provides a more linux native environment for development. This post will go through steps to set up kubernetes for wsl.

You should have docker for desktop installed and WSL enabled to follow along.

Steps:

1. **Enable kubernetes** if you haven't done so already. Go to docker desktop settings -> Kubernetes tab -> check "Enable Kubernetes" setting. After status says "kubernetes is running" run the following command in powershell.

   ```bash
   kubectl get nodes
   ```

   If it shows `docker-for-desktop` then it means kubernetes is running correctly.

   <img src="https://zeerorgprocessedblog.blob.core.windows.net/photos/enable-k8s-docker-desktop.png" alt= "Kubernetes Setting" style="width:50%"/>

2. Open WSL console and **install kubectl** using the following commands

   ```bash
   sudo apt-get update && sudo apt-get install -y apt-transport-https
   curl -s https://packages.cloud.google.com/apt/doc/apt-key.gpg | sudo apt-key add -
   echo "deb https://apt.kubernetes.io/ kubernetes-xenial main" | sudo tee -a /etc/apt/sources.list.d/kubernetes.list
   sudo apt-get update
   sudo apt-get install -y kubectl
   ```

3. **Setting up kubeconfig file**: This is the magic step, kubernetes runs on port 6443 on your host computer and to access it you'll need a kubeconfig file. This is already done by docker for the windows environment but for WSL you'll need to grab the config. Turns out it's pretty easy. Just run the following command in bash and replace {username} with your windows username (not the WSL user name)

   ```bash
   cp /mnt/c/Users/{username}/.kube/config ~/.kube/config
   ```

   This command copies your kubeconfig file from windows to wsl.

4. **All done**: Just run

   ```bash
   kubectl get nodes
   ```

   to see a `docker-for-desktop` node.