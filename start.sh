#!/bin/bash
echo "ENV VARIABLES"
echo "=============\n"
echo -n "Redis__MessageTimeout = "
echo $Redis__MessageTimeout
echo -n "Redis__MessageTimeout = "
echo $Redis__Host
echo -n "Redis__Host = "
echo $Redis__Port
echo -n "Redis__Password = "
echo $Redis__Password
dotnet ChatAPI.dll