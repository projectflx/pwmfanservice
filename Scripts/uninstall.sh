#!/bin/bash

#Reminder to remove device tree overlay for pwm from config.txt
echo -e "\033[0;33mPlease remove the device tree overlay from the config.txt manually if it isn't needed any more!.\033[0m"

#Stopping service
echo "Disableing, stopping and removing the service..."
systemctl disable pwmfan.service
systemctl stop pwmfan.service
rm /etc/systemd/system/pwmfan.service
echo "done."

#Copy files to application folder
echo "Removing the application from /opt/"
rm -r /opt/pwmfanservice
echo "done."