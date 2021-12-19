#sudo apt-get update -y
#sudo apt-get install git make -y
#sudo apt-get install apache2 -y
cd /home/pi/
sudo chown -R pi /tmp/install-apache.sh
sudo chown -R pi /tmp/dhcpcd.conf
echo  "evaluating webCoRE install "
if [ -d \"/home/pi/webCoRE\" ]
then
echo  "webCoRE exists, removing existing install first"
sudo rm -f -r /home/pi/webCoRE
else
echo  "webCoRE folder missing"
fi
if [ -d \"/home/pi/webCoRE\" ]
then
echo "webCoRE exists, skipping install" 
else
echo git clone webcore
#git clone https://github.com/ajayjohn/webCoRE
git clone https://github.com/jp0550/webCoRE
fi
cd webCoRE
echo checkout patches
git checkout hubitat-patches
cd dashboard
sudo rm /var/www/webcore
sudo ln -s `pwd` /var/www/webcore
cd ~/
cd /tmp
mv -f 000-default.conf /etc/apache2/sites-available/
mv -f apache2.conf /etc/apache2/
cd /etc/apache2/
sed -i '1s/^\xEF\xBB\xBF//' apache2.conf
sudo chown -R www-data /etc/apache2/sites-available/
sudo chmod 775 '/etc/apache2/sites-available/'
sudo chown www-data /etc/apache2/apache2.conf
sudo find /var/www -type d -exec chmod 2750 {} \+
sudo find /var/www -type f -exec chmod 640 {} \+
sudo chown -R www-data /var/www
sudo chgrp -R www-data /var/www

echo modify default.conf
cd /etc/apache2/sites-available/
sudo chown -R www-data:www-data 000-default.conf
echo a2enmod
sudo a2enmod rewrite
sudo service apache2 restart
sudo chmod -R /var/www/webcore 400
find /var/www/webcore -type d -exec chmod -R u+x {} \;
sudo chown www-data:www-data /etc/apache2/apache2.conf
sudo chown -R www-data:www-data /etc/apache2/sites-available/
sudo chown www-data:www-data /etc/apache2/apache2.conf
cd /etc/apache2/
sudo service apache2 reload

echo exit
