#https://docs.docker.com/engine/install/debian/#install-using-the-repository

sudo systemctl enable docker.service
sudo systemctl enable containerd.service


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

# docker run -d \
  # --name homeassistant \
  # --privileged \
  # --restart=unless-stopped \
  # -e TZ=America/New_York \
  # -v /home/pi/homeassistant/configuration.yaml \
  # --network=host \
  # ghcr.io/home-assistant/home-assistant:stable
  
docker run -d \
  --name homeassistant \
  --privileged \
  --restart=unless-stopped \
  -e TZ=America/New_York \
  -v /home/pi/homeassistant/config:/config \
  --network=host \
  --device=/PATH_TO_YOUR_USB_STICK
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

sudo docker stop diyHue
sudo docker rm diyHue
docker run -d --name diyHue --restart=always --network=host -e MAC=E4:5F:01:45:9B:C8 -v /mnt/hue-emulator/export:/opt/hue-emulator/export diyhue/core:latest
#sudo docker run -d --name diyhue --restart=always --net=host imightbebob/diyhue


#command docker containers to restart
#sudo docker container ls -a
#sudo docker start $(docker ps -a -q --filter "status=exited")
sudo docker update --restart unless-stopped $(docker ps -q)
sudo docker inspect 7d97


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

#give root access to docker group to share custom folders
#sudo usermod -aG docker root


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
	
#on manager
sudo docker swarm init --advertise-addr 192.168.0.219   #~ this will output the command needed for workers to join
	
#on worker (0.214)
sudo docker swarm join --token SWMTKN-1-0qg11jc0dpjdrf9tv7f504r2jhyet95trcbh1o5yz889fow8k6-exwpzo10leqiyr6k65z0iyelq 192.168.0.219:2377
exit                                                  #~ falls back to `node1`



#ip: 0.136
#rsync -abv /var/lib/docker/containers/7d971c0ce4f5965013e18fe1bb208f5a55997f30dfcbf03dd10a20d89a36c52f/ /home/pi/homeassistant/backups/

sudo -i
mkdir -p /home/pi/homeassistant/backups/
rsync -abv /var/lib/docker/containers/57e62cd764b0dfb1e269b666739c610db9a2055d6747508bdbd6db5ebccf85bd/ /home/pi/homeassistant/backups/

rsync -a --recursive --progress --verbose /var/lib/docker/containers/57e62cd764b0dfb1e269b666739c610db9a2055d6747508bdbd6db5ebccf85bd/ /mnt/fileshare/

# sudo docker container ls -a
/dev/mmcblk0p1: UUID="C839-E506"
/dev/sda4: UUID="CE83-9F6E"

sudo blkid /dev/sda4
UUID="CE83-9F6E" /mnt/usb1 "vfat" defaults,auto,users,rw,nofail,noatime 0 0

#print disk labels
#sudo fdisk -l

sudo mount -t ntfs-3g /dev/sdb2 /media/external


sudo mkdir -p /etc/samba/auto/ && 
sudo touch /etc/samba/smb.conf

sudo echo -e "First Line" | tee ~/etc/samba/smb.conf

sudo echo "First Line" >  ~/etc/samba/smb.conf


sudo echo -e "include = /etc/samba/auto/usb0.conf" | tee /etc/samba/smb.conf

sudo mkdir -p /etc/usbmount/umount.d/50_remove_samba_export
sudo touch /etc/usbmount/umount.d/50_remove_samba_export
sudo chmod +x /etc/usbmount/umount.d/50_remove_samba_export

echo -e "#!/bin/bash" | tee -a /etc/usbmount/umount.d/50_remove_samba_export
echo "SHARENAME=\`basename \$UM_MOUNTPOINT\`" | tee -a /etc/usbmount/umount.d/50_remove_samba_export
echo -e "rm -f /etc/samba/auto/\$SHARENAME.conf" | tee -a /etc/usbmount/umount.d/50_remove_samba_export
echo -e "" | tee -a /etc/usbmount/umount.d/50_remove_samba_export
echo -e "/etc/init.d/samba restart" | tee -a /etc/usbmount/umount.d/50_remove_samba_export

cd /etc/samba/
curl -o smb.conf https://raw.githubusercontent.com/zentyal/samba/master/examples/smb.conf.default


sudo mkdir -p /mnt/fileshare
sudo chown -R pi  /mnt/fileshare
sudo chown -R pi:pi /home/pi/hass-intermediate-sync
sudo chown -R pi /home/pi/hass-intermediate-sync
 
[hass-sync]
comment = Contents are read/write by all.
path = /home/pi/hass-intermediate-sync
read only = no
guest ok = yes
create mask = 0666
force create mode = 0666
directory mask = 0777
force directory mode = 0777

sudo service smbd restart
# mount on windows:  \\raspberrypi\hass-sync\

mount.cifs //raspberrypi/hass-sync /mnt/fileshare/
mount.cifs //192.168.0.220/hass-sync /mnt/fileshare/ -o user=pi

rsync -a --recursive --progress --verbose --delete /var/lib/docker/overlay2/16f9ee054b248c68a0891998c56afceb562929a7b9ca061718f1ded690f6af0f/diff/config/ /mnt/fileshare/

sudo docker stop homeassistant
docker export home-assistant > /tmp/homeassistant.tar
sudo docker start $(docker ps -a -q --filter "status=exited")
sudo docker container ls -a


