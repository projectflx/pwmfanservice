#!/bin/bash

#Add pwm channel to config.txt
#[all]
#dtoverlay=pwm,pin=18,func=2
if ! grep -q "dtoverlay=pwm" "/boot/config.txt"
then
    echo "dtoverlay=pwm,pin=18,func=2" >> "/boot/config.txt"
    echo "Device tree overlay was added to /boot/config.txt."
    echo -e "\033[0;33m[WARNING] You need to reboot your Raspberry Pi for the changes to take effect.\033[0m"
fi

#Copy files to application folder
echo "Copying files to application folder at /opt/pwmfanservice/"
mkdir /opt/pwmfanservice
cp ../PWMFanService ../libSystem.IO.Ports.Native.so ../appsettings.json /opt/pwmfanservice/
chmod +x /opt/pwmfanservice/PWMFanService
echo "done."

#Copy service file to systemd-directories
echo "Copying service file to /etc/systemd/system/"
cp ../Service/pwmfan.service /etc/systemd/system/
echo "done."