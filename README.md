# Thermominer

The purpose of Thermominer is to act as a thermostat for mining rigs, 
allowing users to use mining rigs for heating their home instead of
electric radiators.

Thermominer will have the mining rigs start mining when the temperature is
below a certain threshold, and stop mining once the temperature gets 
above a certain threshold.

It also features a webinterface where users can view current temperature 
and hasrate with graphs.

# Overview
![overview](https://github.com/studiefredfredrik/thermominer/blob/master/overview.PNG?raw=true "overview")

The repo consists of 3 projects:

## ArduinoTemperatureSensor
A project for reading temperature data from a TMP006 board

## NetCoreWebApi
A .net core webApi project, that also hosts the web interface

## WindowsGui
Interfaces with Arduino temperature sensor and parses ethermine output
for hashrate info. Posts data to the WebApi





