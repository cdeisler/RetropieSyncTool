#randomRom=$(find /home/pi/RetroPie/roms/arcade/ -name "*.zip" | shuf -n 1)
randomRom=$(shuf -n 1 /home/pi/autorunrom.txt)
echo "randomRom is $randomRom"
/opt/retropie/emulators/retroarch/bin/retroarch -L /opt/retropie/libretrocores/lr-mame2003/mame2003_libretro.so --config /opt/retropie/configs/mame-libretro/retroarch.cfg $randomRom --appendconfig /opt/retropie/configs/all/retroarch.cfg && emulationstation
