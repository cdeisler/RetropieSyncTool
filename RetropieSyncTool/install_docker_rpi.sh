https://docs.docker.com/engine/install/debian/#install-using-the-repository


sudo apt-get update
sudo apt-get install ca-certificates curl gnupg lsb-release
curl -fsSL https://download.docker.com/linux/debian/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update
sudo apt-get install docker-ce docker-ce-cli containerd.io


 
# USB Path: /dev/ttyUSB0
# Network Key: 0xA5, 0xD9, 0x39, 0xCE, 0xD9, 0x44, 0x95, 0x4C, 0xC7, 0x81, 0xF1, 0x7D, 0x6D, 0xE4, 0xBF, 0x79\

#USB Path: /dev/ttyUSB0
#Network Key: 0x4E, 0xA4, 0xBC, 0xB4, 0x05, 0x4D, 0x51, 0x7C, 0xAD, 0x07, 0xCD, 0xDB, 0x14, 0x65, 0xAA, 0x1F

#/home/pi/homeassistant/configuration.yaml
http://192.168.0.219/

docker stop zwave-js
docker rm zwave-js
#install z-wave js server

sudo docker stop home-assistant
sudo docker rm home-assistant

docker run -d \
  --name homeassistant \
  --privileged \
  --restart=unless-stopped \
  -e TZ=America/New_York \
  -v /home/pi/homeassistant/configuration.yaml \
  --network=host \
  ghcr.io/home-assistant/home-assistant:stable
  
echo "dtoverlay=disable-bt" | sudo tee -a /boot/config.txt
echo "enable_uart=1" | sudo tee -a /boot/config.txt

sudo mkdir -p /home/docker/zwave-js/

sudo docker stop zwave-js
sudo docker rm zwave-js
sudo docker run  -it -p 8091:8091 -p 3000:3000 -e "TZ=America/New_York" --name "zwave-js" --device=/dev/ttyUSB0 --restart unless-stopped --mount source=zwavejs2mqtt,target=/usr/src/app/store zwavejs/zwavejs2mqtt:latest
# Using volumes as persistence --rm # removes existing



sudo docker stop hue2mqtt
sudo docker rm hue2mqtt
sudo docker run --name hue2mqtt \
-v hue2mqtt-data:/root/.hue2mqtt \
-e TZ=America/New_York \
-e BRIDGE_IP=192.168.0.219 \
-e MQTT_PREFIX=hue \
-e MQTT_URL=mqtt://localhost:1883 \
-e POLLING_INTERVAL=1 \
klutchell/hue2mqtt:armhf-latest

sudo docker stop diyhue
sudo docker rm diyhue
sudo docker run -d --name diyhue --restart=always --net=host imightbebob/diyhue


#command docker containers to restart
#sudo docker container ls -a
#sudo docker start $(docker ps -a -q --filter "status=exited")
sudo docker update --restart unless-stopped $(docker ps -q)


#install pihole on docker
cd ~/
cd /opt
sudo -u root mkdir piholeroot

#sudo chown -R pi /tmp/dhcpcd.conf

PIHOLE_BASE=/opt/pihole-storage
TZ=America/New_York
WEBPASSWORD=cjcjcjcj
FTLCONF_REPLY_ADDR4=192.168.0.219

# reset password doesnt' work below
#sudo docker exec pihole sudo pihole -a -p


sudo docker stop pihole
sudo docker rm pihole
PIHOLE_BASE=/opt/piholeroot
WEBPASSWORD=cj
sudo docker run -d \
    --name pihole --restart unless-stopped \
    -p 53:53/tcp -p 53:53/udp \
    -p 80:80 \
    -e TZ="America/New_York" \
    --dns=192.168.0.1 --dns=1.1.1.1 \
	# -v "${PIHOLE_BASE}/etc-pihole:/etc/pihole" \
    # -v "${PIHOLE_BASE}/etc-dnsmasq.d:/etc/dnsmasq.d" \
    --restart=unless-stopped \
    --hostname pi.hole \
    -e VIRTUAL_HOST="pi.hole" \
    -e PROXY_LOCATION="pi.hole" \
    -e ServerIP="192.168.0.219" -e WEBPASSWORD="${WEB_PASSWORD}" \
    pihole/pihole:latest
