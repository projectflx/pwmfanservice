# PWMFanService
## Description
PWMFanService is a systemd-service which aims for minimal dependencies and CPU-usage by utilizing the pwm hardware of a Raspberry Pi to control a fan based on the temperature of the CPU. It is written in C# and has a minimal footprint.

## Used nuget packages
The following list contains the used nuget packages for this service:
- [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting)
- [Microsoft.Extensions.Hosting.Systemd](https://www.nuget.org/packages/Microsoft.Extensions.Hosting.Systemd)
- [Iot.Device.Bindings](https://www.nuget.org/packages/Iot.Device.Bindings)
- [System.Device.Gpio](https://www.nuget.org/packages/System.Device.Gpio)

## Installation
### Hardware
I assume you already connected your fans power and ground cable to the Raspberry Pi.

Connect your fans pwm connector to **pin 18**.
### Software

Create a new directory for the files and change into it
```bash
mkdir pwmfanservice
cd pwmfanservice
```

Get the latest release be downloading it.
```bash
wget https://github.com/projectflx/pwmfanservice/releases/download/v1.0.0/PWMFanService.tar.gz
```
Extract the files
```bash
tar xvf PWMFanService.tar.gz
```

Change directory into the scripts folder, add execution permission and run the script with elevated rights.
```bash
cd ./Scripts
chmod +x install.sh
sudo ./install.sh
```
The install-script does automatically activate the device tree overlay for pwm on pin 18. You are free to reconfigure it to another pin, but keep in mind that the only pins supporting hardware pwm are pin 12, 13, 18 and 19.

Check the console output for additional information.

### Building from source
#### Requirements
For building this project from the source code the only thing you need, is the .NET 6.0 SDK. Additional dependencies will be downloaded via nuget when restoring the project.
#### Build process
After downloading and unzipping the files into a directory of your choice, the only command you need to run within that directory is the following.
```bash
dotnet publish
```
