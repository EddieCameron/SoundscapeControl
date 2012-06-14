SoundscapeControl.cs
Copyright Eddie Cameron 2012
-------
SoundscapeControl.cs is a simple script to help manage fadeins/outs of audio in Unity3D.
To use
- put the script on a gameobject
- add whatever audiogroups you like, they just need a name and one or more audiosources
- set the audiosources to whatever volume they should be at max
- at runtime, call FadeInGroup/FadeOutGroup (static) and provide the name of the group

A simple script, but I've wasted a hell of a lot of time sorting out audio fade logic. Call fadein while something is fading out,
call fadeout ten times in a row, NO LONGER WILL IT HURT
-------
Source is under GPL, do what you will with it.