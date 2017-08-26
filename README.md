# hkxPoser
This is a simple editor of animation.hkx. I made it for the purpose of fine adjustment of existing pose.

## Prerequisite
- .NET Framework 4.5.2
- [hkdump](https://github.com/opparco/hkdump)

## Usage
Start hkxPoser.exe.

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

## License

MIT License
