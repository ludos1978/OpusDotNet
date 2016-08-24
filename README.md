# Opus Codec in Unity 3d
Working version of Opus Codec with limited options for Unity3d.

# state
it does a local recording of the microphone, sending via unet, distribution on the server and playback on the clients.

# usage
add the script along with a network identity to a gameobject with 2 child gameobjects, each child needs an audiosource, add one as src the other as player to the script. add this gameobject as player to the network manager.
or just use the provided example...

# thanks to
uses the c# wrapper from opusdotnetinvoke:
https://opusdotnetinvoke.codeplex.com/

compiled the libraries on windows and osx from:
https://www.opus-codec.org/downloads/

# license
lgpl except for the code that is differently licensed.

hopefully someone can use this and make his improvements available to the public as well!

# tested on:
windows 10, 64bit
osx 10.11.5
