#!/bin/bash
RETROPIE_VER="0"
PIMODEL=$(cat /proc/cpuinfo | grep 'Model')
  case $PIMODEL in
    *"Pi 4 Model B"*)
        RETROPIE_VER="4.40";;
    *"Pi 3 Model"*)
        RETROPIE_VER="2.3";;
    *"Pi Zero 2"*)
        RETROPIE_VER="2.3";;
    *"Pi Zero 1"*)
        RETROPIE_VER="0.1";;
    *)
        echo "INVALID PIMODEL";;
  esac
echo "192.168.0.103/?emu=fba&model=$PIMODEL"
ROM=$(curl "192.168.0.103/?emu=fba&model=$RETROPIE_VER")
echo "ROM is $ROM pi_model: $PIMODEL retropie_ver: $RETROPIE_VER"
/bin/bash /opt/retropie/configs/all/runRom.sh "$ROM" "$PIMODEL" &