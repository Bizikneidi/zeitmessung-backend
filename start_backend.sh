#!/bin/bash

sudo git pull;
sudo killall -9 dotnet;
sudo dotnet build;
sudo dotnet run --project TimeMeasurement_Backend
