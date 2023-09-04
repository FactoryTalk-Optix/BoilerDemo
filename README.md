# Boiler Demo

FactoryTalk Optix `BoilerDemo` application. This demo was developed to show some features of FactoryTalk Optix.  

The BoilerDemo application communicates to a GuardLogix 5580 controller and OPC UA with the UA ANSI C Server.

This application will connect via OPC-UA to the demo server provided by the UA Foundation [link](https://www.unified-automation.com/downloads/opc-ua-servers.html) and a Rockwell controller, such as FactoryTalk Echo, running the Boiler Demo code that can be found in `ProjectFiles\LogixCode` folder,

Note that this application is designed to work with the `BoilerDemo_Client` application in order to demonstrate OPC UA connectivity.  

Both applications must be loaded and running in separate instances of FactoryTalk Optix Studio in order to demonstrate OPC UA connectivity:

1. `BoilerDemo` 
2. `BoilerDemo_Client` 

## Clone repository

1. Click on the green `CODE` button in the top right corner
2. Select `HTTPS` and copy the provided URL
3. Open FT Optix IDE
4. Click on `Open` and select the `Remote` tab
5. Paste the URL from step 2
6. Click `Open` button in bottom right corner to start cloning process

## Preparing the demo

1. Download the OPC-UA Server from the link above
2. Download the PLC Code from `ProjectFiles\LogixCode` to a PLC (FT Echo is fine)
3. Execute the demo

## Explore the project:

1. Once the application is loaded, start the Emulator by clicking on the `Play` button
2. Once the application is running in the emulator, click on the following buttons and valves located on the main boiler screen:
               1. Click on the `wave` icon under the boiler to Fill the boiler
               2. Click on the `heat` icon under the boiler to heat the boiler
               3. Click on the `snowflake` icon under the boiler to cool the boiler
               4. Open valve `EV101`
               5. Open valve `EV102`
               6. Open valve `EV201`
               7. Open valve `EV202`
3. When a tank is empty, an alarm is generated.


## Description

This demo was made to demonstrate interoperability between different communication drivers, three aliases in the MainPage are used to interface those drivers, when clicking a button, the command is also sent to the other driver to trigger the water flow.

Widgets has been prepared using images from the TemplateLibrary

## Script for presentations

When exposing this demo, the typical procedure is described in the [dedicated NetLogic](./ProjectFiles/NetSolution/DemonstrationScript.cs)
