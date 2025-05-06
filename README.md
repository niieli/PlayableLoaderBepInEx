# Original Playable Loader

[UMM Mod by m_cube](https://www.nexusmods.com/herecomesniko/mods/2?tab=description)

## Why another version?

I ported the mod to BepInEx so it can be used with the [Here Comes Niko! Randomizer](https://www.nexusmods.com/herecomesniko/mods/2?tab=description).

## How to use??
When loaded in, open the menu and in the bottom left there should be a new icon. 

![image](https://github.com/user-attachments/assets/f690cea7-c77a-4e2f-8a84-de71793076e0)


Pressing it will open up the Playable Loader Menu, where you can enable/disable the mod and change your playable
![image](https://github.com/user-attachments/assets/a1116b6a-c0b3-4ffe-a446-bb11fa5e1f14)


# Can I make my own playables?
Yes, you can use the HereComesHueh folder (`playables/HereComesHueh`) as a reference on how to structure your own folder.

The mod will create a new folder (`originalFiles`) where the normal Niko assets are stored, so you can use them to create your own.

Your final folder should look like this:
```swift
BepInEx/
├── plugins/
│   └── PlayableLoaderBepInEx.dll
│       ├── playables/
│       │   └── YourCustomPlayable
│       │       ├── assets/
│       │       └── icon.png
│       │       └── info.ini
```
- assets folder conatins all the pngs.

- `icon.png` is the icon of your playable, which will be used in the menu.

- `info.ini` conatins Name, Author & icon
   - Name=Your Playable Name
   - Author=YourName
   - icon=icon.png
