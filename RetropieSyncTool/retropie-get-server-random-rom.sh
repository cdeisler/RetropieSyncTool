#!/bin/bash
SERVER_HOST="192.168.0.103"
RETROPIE_VER="0"
PIMODEL=$(cat /proc/cpuinfo | grep 'Model')
echo "PIMODEL $PIMODEL"
  case $PIMODEL in
    *"Pi 4 Model B"*)
        RETROPIE_VER="4.4";;
    *"Pi 3 Model"*)
        RETROPIE_VER="2.3";;
    *"Pi Zero 2"*)
        RETROPIE_VER="2.3";;
    *"Pi Zero 1"*)
        RETROPIE_VER="0.1";;
    *)
        echo "INVALID PIMODEL";;
  esac

idx=$(shuf -i 1-4 -n 1)

echo "idx $idx"

EMU=$1

echo "${SERVER_HOST}/?emu=${EMU}&model=$PIMODEL"
ROM=$(curl "${SERVER_HOST}/?emu=${EMU}&model=$RETROPIE_VER")
echo "ROM is $ROM pi_model: $PIMODEL retropie_ver: $RETROPIE_VER"
/bin/bash /opt/retropie/configs/all/runRom.sh "$ROM" "$PIMODEL" &