#!/bin/bash
IP=$(curl '192.168.0.103/?emu=fba')
echo "ip is $IP"
/bin/bash /opt/retropie/configs/all/runRom.sh "$IP" &