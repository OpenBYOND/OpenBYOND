[![Build Status](http://ci.nexisonline.net/buildStatus/icon?job=OpenBYOND)](http://ci.nexisonline.net/job/OpenBYOND/)

OpenBYOND
=========

OpenBYOND for C#

Compiling
=========

Windows
-------

 1. Install .NET 4.0 if needed.
 2. Install Visual C# Express (Full studio will also work)
 3. Run Prebuild.bat.
 4. Open OpenBYOND.sln.
 5. Set OpenBYONDClient as the startup project.
 6. Run with F5.

Linux/Mac OSX
-------------

 1. Install Mono (must support .NET 4.0)
 2. Install nant
 3. Run ```mono bin/Prebuild.exe /target:nant /file:Prebuild.xml``` to generate nant project.
 4. Run ```nant``` to compile.
