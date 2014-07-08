Minecraft Windows Server Wrapper
=========
Server wrapper for controlling the minecraft server, and exposing some API calls to the website.


This will build straight out, just need to open and compile. However to run it as a non-windows service, goto the project properties and add a -debug for the command line. This will run it in console mode.


There are a few custom commands implemented thus far that you can use from service control (sc.exe).

* sc command minecraftserver 200 - restarts the server internally
* sc command minecraftserver 201 - disables auto-save
* sc command minecraftserver 202 - enables auto-save
* sc command minecraftserver 203 - forces a save

You can also add or override custom command by adding settings to appSettings.

* &lt;add key="Command204" value="me is laggy and needs a restart;save-all;stop;" /&gt;

That can then be fired with:

* sc command minecraftserver 204 - runs custom command

The value is a semi-colon separated list of commands to execute, you can run as many as you like.

