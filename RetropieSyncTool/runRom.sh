#!/bin/bash 

echo "runRom starting..."
echo "runRom: $1"

: ${EMULATOR_NAME:="default"}

#BASE_DIRECTORY="$(dirname -- "$(realpath -- $1)")"
BASE_DIRECTORY="$(basename "$(dirname "$1")")"

#BASE_DIRECTORY=$(echo "$1" | cut -d "/" -f2)
echo "parent directory: $BASE_DIRECTORY";

case $BASE_DIRECTORY in
    *"fba"*)
        EMULATOR_NAME="fbneo";;
    *"mame2003"*)
        EMULATOR_NAME="mame-libretro";;
    *)
        echo "INVALID FILE";;
esac

# if [ BASE_DIRECTORY == fba ]
# then
# EMULATOR_NAME = fbneo
# fi

# if [ BASE_DIRECTORY == mame2003 ]
# then
# EMULATOR_NAME = mame-libretro
# fi

echo "EMULATOR_NAME: $EMULATOR_NAME";

sudo ps -ef | awk '/emulation/ {print $2}' | xargs sudo kill -9 2092
sudo ps -ef | awk '/mame/ {print $2}' | xargs sudo kill -9 2092
sudo ps -ef | awk '/lr-fbneo/ {print $2}' | xargs sudo kill -9 2092

echo "path: /opt/retropie/libretrocores/lr-${EMULATOR_NAME}/${EMULATOR_NAME}_libretro.so"
#eval "/opt/retropie/admin/joy2key/joy2key.py && /opt/retropie/emulators/retroarch/bin/retroarch -L /opt/retropie/libretrocores/lr-$EMULATOR_NAME/${EMULATOR_NAME}_libretro.so --config /opt/retropie/configs/${EMULATOR_NAME}/retroarch.cfg $1 --appendconfig /opt/retropie/configs/all/retroarch.cfg && emulationstation"


#sudo /opt/retropie/admin/joy2key/joy2key.py && /opt/retropie/emulators/retroarch/bin/retroarch -L /opt/retropie/libretrocores/lr-$EMULATOR_NAME/${EMULATOR_NAME}_libretro.so --config /opt/retropie/configs/${EMULATOR_NAME}/retroarch.cfg $1 --appendconfig /opt/retropie/configs/all/retroarch.cfg && emulationstation

/opt/retropie/emulators/retroarch/bin/retroarch -L /opt/retropie/libretrocores/lr-${EMULATOR_NAME}/${EMULATOR_NAME}_libretro.so --config /opt/retropie/configs/${EMULATOR_NAME}/retroarch.cfg $1 --appendconfig /opt/retropie/configs/all/retroarch.cfg && emulationstation


#sudo /opt/retropie/supplementary/runcommand/runcommand.sh 0 _SYS_ fba /home/pi/RetroPie/roms/fba/doubledr.zip
#ssh pi@192.168.0.136 'bash -s' < /opt/retropie/supplementary/runcommand/runcommand.sh 0 _SYS_ fba /home/pi/RetroPie/roms/fba/doubledr.zip

#echo "root directory: $(dirname -- "$file")"

#IFS='/' array=($fullpath)
#echo "directory: /${array[1]}"

#/opt/retropie/emulators/retroarch/bin/retroarch -L /opt/retropie/libretrocores/lr-mame2000/mame2000_libretro.so --config /opt/retropie/configs/mame-libretro/retroarch.cfg $randomRom --appendconfig /opt/retropie/configs/all/retroarch.cfg && emulationstation