#!/bin/bash

git pull;
killall -9 dotnet;
dotnet build;
dotnet run --project TimeMeasurement_Backend
