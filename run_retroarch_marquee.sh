#!/bin/bash
#get script path

scriptfile=$(readlink -f $0)
installpath=`dirname $scriptfile`

echo "installpath $installpath"
-

python3 /home/kiosk/PieMarquee2/PieMarquee2/PieMarquee2.py > /dev/null 2>&1 && sudo ./retroarch/retroarch -L /home/kiosk/libretro-super/dist/unix/fbneo_libretro.so /home/kiosk/libretro-super/libretro-fbneo/roms/aof.zip

echo
echo "Setup Completed. "