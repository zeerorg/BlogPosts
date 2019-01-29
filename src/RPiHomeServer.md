# Sharing your apps with the world for 5$

Raspberry Pi is a bit underpowered to compete with a full server, no doubt about it, but a lot of our hobby apps which don't ever see the light of the internet, or our test projects which are not ready for the cloud world are the best match with this tiny marvel. You learn a lot about how servers work and get a project hosted on internet, for the cheapest price possible.

## What I run on my Raspberry Pi

I currently have an [OpenFaas](https://docs.openfaas.com/) instance running on my Raspberry Pi serving two applications, first a Github Webhook endpoint and second a Password generator based on [EFF list](https://www.eff.org/dice).  OpenFaas runs over Docker swarm which I manage using a [Portainer](https://www.portainer.io/) ui also running as Docker container. I monitor resource usage with [Glances](https://nicolargo.github.io/glances/) server. I plan on adding a [Jupyter Hub](https://jupyterhub.readthedocs.io/en/stable/) instance and putting all of this behind an NGINX proxy server possibly with auth.

Yes, my setup is a bit too overkill for a small machine like raspberry pi, but it has taught me a lot of things about a lot of different software and possibilities of this tiny monster.

You can access the password generator on this [link](https://home.zeerorg.site:3333/).

## Prerequisites

1. 5$ Raspberry Pi
2. Domain Name (Optional but highly recommended for https)

## Diagram Time

![Home server setup](https://zeerorgprocessedblog.blob.core.windows.net/photos/rpiserver.jpg)

## Reverse proxy - because, Security

Setting up a reverse proxy server might seem like a bit of a pain but it provides a secure gateway as exposing your home network and your applications on the internet is always a risk. You can also use it to setup SSL if you have a domain. I prefer Nginx simply because it has a lot of guides and is simple to setup (Apache's config scares me). Simple HTTP reverse proxy:

```nginx
server {
  listen 443;
  location / {
    proxy_pass http://locahost:[:PORT]/;
  }
}
```

Deploying:

```bash
sudo nano /etc/nginx/sites-enabled/default
# Copy the config then Ctrl+x to save
sudo nginx -s reload
```

Also see: [SSL with nginx](https://www.digitalocean.com/community/tutorials/how-to-create-an-ssl-certificate-on-nginx-for-ubuntu-14-04)

## Correctly starting your applications

The second biggest mistake you can make after ignoring security, is assuming that your raspberry pi will be running most of the time, and if it reboots, you'll just start your services again. Don't make this mistake, setting up a self starting service is trivial and its the best that you can do for your peace of mind. Refer [this official Raspbian guide](https://www.raspberrypi.org/documentation/linux/usage/systemd.md) on setting up a self starting service.

app.service

```bash
[Unit]
Description=My Applicaiton
After=network.target

[Service]
ExecStart=/usr/bin/python3 -u main.py
WorkingDirectory=/home/pi/applicaiton
Restart=always
User=pi

[Install]
WantedBy=multi-user.target
```

Copy and enable service

```bash
sudo cp app.service /etc/systemd/system/app.service
sudo systemctl start app.service
sudo systemctl enable app.service
```

## ARM has come a long way

Another concern which people have before going and hosting their service is that it'll be a pain to build for a different architecture, or they'll have to submit to satan to get their application running. It was the case few years back, but not anymore! Building your application for different architecture is as easy as providing a build time flag for compiled languages, or installing the interpreted languages runtime. Interpreted languages like python and javascript can be easily installed using existing package manager.

Statically compiled languages can be cross compiled for ARM on your current machine. For example:

- Golang: `env GOOS=linux GOARCH=arm GOARM=5 go build`
- .NET Core: `dotnet publish -r linux-arm`
