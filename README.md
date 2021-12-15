![Marshal Icon](https://github.com/sim2kid/DesktopAquarium/blob/main/Assets/Materials/Textures/icon.PNG?raw=true)
# Desktop Aquarium
This project allows you to have a bunch of pet fish that sit in the background of your desktop as an animated backdrop.

You can choose which monitor to set as the background.

### Recent Goals
- [x] Refactor to have the desktop code be independent of Unity.
- [x] Allow for the desktop code to align to an assigned desktop. (Multi-Monitor Support)
- [x] Add a tray Icon that can grab the window from the background

### Here are the two C# files of note
[The wallpaper control code (System specific. In this case, Windows)](https://github.com/sim2kid/DesktopAquarium/blob/main/Assets/Scripts/Background/Windows/Wallpaper.cs)<br>
[The unity controller that is a monobehavior](https://github.com/sim2kid/DesktopAquarium/blob/main/Assets/Scripts/WindowController.cs)

# Build Info
This project was built in Unity 2020.3. Any of those versions should load perfectly fine.


# Sources
* Putting the window behind the desktop icons has been enabled by this code project. The approch works for Windows 8, 10, and 11
  * [Draw Behind Desktop Icons in Windows 8+](https://www.codeproject.com/Articles/856020/Draw-Behind-Desktop-Icons-in-Windows-plus)
