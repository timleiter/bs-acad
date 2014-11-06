bs-acad
=======

Hack-a-thon project for integrating AutoCAD with bitspring

Building and Running
====================

Install the ObjectARX and .Net autodesk frameworks for Autocad.


Open the solution in Visual studio 2013

Build and run the solution from Visual Studio

Open a dwg file

In the command line bar at the bottom of the autocad window enter the command NETLOAD, navigate to the debug (or release if you built that) folder for the plugin and select the BitSpring For AutoCad.dll file.

Select load in the warning dialog that appears.

To upload via BitSpring in the command line bar enter the command UPLOADTOBITSPRING
