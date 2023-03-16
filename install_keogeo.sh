#!/bin/bash

# Function to check if a package is installed and up-to-date
check_package() {
    package_name=$1
    dpkg -l "$package_name" &> /dev/null
    if [ $? -ne 0 ]; then
        echo "$package_name is not installed"
        return 1
    fi

    installed_version=$(dpkg-query -W -f='${Version}' "$package_name")
    available_version=$(apt-cache policy "$package_name" | grep Candidate | awk '{print $2}')

    if [ "$installed_version" != "$available_version" ]; then
        echo "$package_name is outdated"
        return 1
    fi

    echo "$package_name is up-to-date"
    return 0
}


sudo apt-get update
sudo apt-get -y upgrade


username="kiosk"

# Add user to sudoers
echo "$username ALL=(ALL) NOPASSWD:ALL" > /tmp/extra_sudoers

if visudo -cf /tmp/extra_sudoers; then
    cat /tmp/extra_sudoers >> /etc/sudoers
    echo "Added $username to sudoers"
else
    echo "Error: Invalid syntax"
fi

rm /tmp/extra_sudoers

# Create drop-in configuration for getty@tty1.service
getty_override_dir="/etc/systemd/system/getty@tty1.service.d"
getty_override_file="${getty_override_dir}/override.conf"

mkdir -p "$getty_override_dir"

cat > "$getty_override_file" << EOF
[Service]
ExecStart=
ExecStart=-/sbin/agetty --noissue --autologin kiosk %I $TERM
Type=idle
EOF

echo "Created drop-in configuration for getty@tty1.service"

# Reload systemd configuration
systemctl daemon-reload
echo "Reloaded systemd configuration"


# Disable cloud-init
sudo touch /etc/cloud/cloud-init.disabled
echo "Disabled cloud-init"

# Reconfigure cloud-init
#DEBIAN_FRONTEND=noninteractive sudo dpkg-reconfigure cloud-init
#echo "Reconfigured cloud-init"

#Uninstall the package and delete the folders
sudo apt-get -y purge cloud-init
sudo rm -rf /etc/cloud/ && sudo rm -rf /var/lib/cloud/


packages=("net-tools" "wpasupplicant" "unclutter" "git" "build-essential", "python3-pyqt5")

# Iterate over packages and install or update if necessary
for package in "${packages[@]}"; do
    if ! check_package "$package"; then
        echo "Installing or updating $package..."
        sudo apt-get -y install "$package"
    else
        echo "$package is already up-to-date"
    fi
done

#install additional requirements
#sudo apt-get -y install net-tools
#sudo apt-get -y install wpasupplicant
#sudo apt-get -y install unclutter


#start libretro install
sudo add-apt-repository -y ppa:libretro/stable
sudo apt-get update

#sudo apt-get -y install git build-essential

echo "clone libretro/retroarch"
git clone https://github.com/libretro/RetroArch.git retroarch

sudo cp /etc/apt/sources.list /etc/apt/sources.list.bak && sudo sed -i 's/^# deb-src/deb-src/' /etc/apt/sources.list
sudo apt-get update

sudo apt-get -y build-dep retroarch
cd retroarch
git pull

./configure
make clean
make -j4


cd ..

#Fetch cores
git clone https://github.com/libretro/libretro-super.git
cd libretro-super
./libretro-fetch.sh fbneo
./libretro-build.sh fbneo

#sudo ./retroarch -L /home/kiosk/libretro-super/dist/unix/fbneo_libretro.so /home/kiosk/roms/arcade/kof96.zip


#sudo apt-get install -y python3-pyqt5




