# hkxPoser
This is a simple editor for animation.hkx.
I made it for the purpose of fine adjustment of existing pose.

## Prerequisite
- .NET Framework 4.5.2
- [hkdump](https://github.com/opparco/hkdump)

## Usage
1. Prepare nif models.

Copy nif models (.nif) into data\meshes folder.
Copy diffuse textures (.dds) into data\textures folder. subfolders are not needed.

### Example
```
in data\meshes:
  femalebody_1.nif
  femalefeet_1.nif
  femalehands_1.nif
  femalehead.nif

in data\textures:
  femalebody_1.dds
  femalehands_1.dds
  femalehead.dds
```
2. Launch hkxPoser.exe

3. Drop any animation file (.hkx) onto the screen.

Click on a round marker to select a bone.
The selected bone will turn red.

### Transform window
You can operate the selected bone.
- Loc: Location
- Rot: Rotation

Hold down any of the XYZ buttons and move the mouse.
Then the selected bone moves and rotates.

### Menu
- File
  - Open: Ctrl+O Open the existing .hkx file.
  - Save: Ctrl+S Save the created pose as an .hkx file.
- Edit
  - Undo: Ctrl+Z
  - Redo: Ctrl+Y

## Build

### Prerequisite
- Visual Studio 2017
- SharpDX 4.0.1

Run MSBuild

## Acknowledgments
hkxPoser uses Havok(R). (C) Copyright 1999-2008 Havok.com Inc. (and its Licensors). All Rights Reserved. See www.havok.com for details.

- [NifTools Project](http://www.niftools.org/)
- [SharpDX](http://sharpdx.org/)

## License
MIT License

## Author
opparco
