$Id: BUILD,v 1.11 2005/10/23 01:35:59 titer Exp $

BUILD file for HandBrake <http://handbrake.fr/>

Building HandBrake with make
=============================

Step 1: get needed tools
==============

+ gcc and g++
    There are usually included in your OS' dev tools. On BeOS/Zeta, the
    default gcc isn't enough, gcc 2.95.3 is required.

+ nasm (Only for x86. On Mac OS X Intel, Xcode 2.4.1 include it)

Cygwin:
	See Trac > Windows Compile Guide

Step 2: configuration
==============

The HB_BUILD and HB_VERSION are defined in a single file, the Makefile correct the values in Xcode too.

The file where the informations are stored is Makefile.config

Step 3: build
==============

Run `make'. This will build libhb, Handbrake and HandBrakeCLI as Universal Binary.
This build method use precompiled contrib libraries. Script to build those binaries are provided too.
All the build is handled by Xcode 2.4.1, should work on powerPC and Intel Macs.

# To build under Cygwin simply use the command:
make HandbrakeCLI

This will download pre-compiled libaries and build the handbrake source.

-----------------------------------------------------------------------------------------------------

Building HandBrake with jam
=============================

You can build HandBrake on BeOS, MacOS X, Linux, and on Windows using Cygwin.
If you'd like to port it to another OS, email me (titer@m0k.org).

Step 1: get needed tools
==============

+ gcc and g++
    There are usually included in your OS' dev tools. On BeOS/Zeta, the
    default gcc isn't enough, gcc 2.95.3 is required.

+ jam
    I use 2.5rc3, earlier versions might cause issues.
    On BeOS, you can download it at <http://www.haiku-os.org/develop.php>.
    On OS X, you cannot use the modified jam shipped with the developer
    tools, use the one included in the Handbrake svn checkout instead.
    On Cygwin, get the jam source from
    http://public.perforce.com/public/jam/index.html,
    compile it with gcc in Cygwin, and put the jam executable somewhere on
    your path.

+ nasm (Only for x86. On Mac OS X Intel, Xcode 2.4.1 include it)

+ libtool, autoconf, automake
    To build libdca (the DTS audio extraction library) on Mac OS X via jam, you'll
    need to update the default Mac OS X versions of libtool, autoconf and automake.
    Compilation has been seen to work with libtool and libtool-shlibs v1.5.22-1000,
    autoconf v2.60-4, and automake v1.9.6-3.  You can update these tools using Fink.
    Download the Fink 0.8.1 Binary Installer for your platform (PowerPC or Intel)
    from http://www.finkproject.org/download/index.php?phpLang=en and install Fink
    using the installer.  If you want to use a GUI to run Fink, you can install
    FinkCommander.  Download the FinkCommander 0.5.4 installer from
    http://finkcommander.sourceforge.net/ and install from the disk image.  You can
    install libtool, libtool-shlibs, autoconf and automake using FinkCommander.

Cygwin:
	See Trac > Windows Compile Guide

Step 2: build
==============

Run `./configure && jam'. This will build every library HandBrake
requires, then HandBrake itself.
