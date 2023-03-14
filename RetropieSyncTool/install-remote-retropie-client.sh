#!/bin/bash

cd /opt/retropie/configs/all/
chmod +x retropie-get-server-random-rom.sh
nohup /bin/bash /opt/retropie/configs/all/retropie-get-server-random-rom.sh &
chmod +x /opt/retropie/configs/all/runRom.sh

if mount | grep -q /192.168.0.220/share
FILE=/192.168.0.220/share/retropie/4.4/roms/arcade/nitedrvr.zip
[ -f $FILE ] && echo "$FILE exist." || echo "$FILE does not exist."


if mount | grep -q /192.168.0.220/share
then
    if [[ ! -f /192.168.0.220/share/retropie/4.4/roms/arcade/nitedrvr.zip ]]
    then
        echo "mounted"
    fi
else
    echo "not mounted"
fi


smbclient //raspberrypi/share/retropie/4.4/roms/arcade/nitedrvr.zip


smbclient -L "192.168.0.220" | grep -i "share"
smbclient -L SERVERNAME | grep -i "YOURSHARENAME"
[ -e "/192.168.0.220/share/retropie/4.4/roms/arcade/nitedrvr.zip" ] && echo 1 || echo 0

cd /home/pi/RetroPie/roms/arcade
smbget -R smb://raspberrypi/share/retropie/4.4/roms/arcade --update --nonprompt
smbget -R smb://raspberrypi/share/retropie/4.4/roms/fba --update --nonprompt

sudo smbget -O -U pi%raspberry smb://raspberrypi/share/retropie-get-server-random-rom.sh
sudo smbget -O -U pi%raspberry smb://raspberrypi/share/runRom.sh


wget -q http://www.whatever.com/filename.txt -O /path/filename.txt

smbget -U pi%raspberry smb://raspberrypi/share/retropie/4.4/roms/arcade/nitedrvr.zip
idx=$(shuf -i 1-4 -n 1)

case "$idx" in

1)  echo "Sending SIGHUP signal"
    
    ;;
2)  echo  "Sending SIGINT signal"
   
    ;;
3)  echo  "Sending SIGQUIT signal"
   
    ;;
4) echo  "Sending SIGKILL signal"
   
   ;;
*) echo "Signal number $idx is not processed"
   ;;
esac

