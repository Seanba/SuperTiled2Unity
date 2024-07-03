# Changelog

## [2.2.1] - 2024-07-02
- Bug fix: Resolved issue with the way asset paths were used within ST2U. That had broken importing in the MacOS and Linux versions of the Unity Editor
- Bug fix: "Layer/Object Sorting" option has been added back to the Tiled Map Importer Settings

## [2.2.0] - 2024-06-01
- No longer creating and managing embedded sprite atlases. This allows uses to put tile sheets into sprite altases of their choosing. ST2U now uses the original textures to make tiles.
- Improved error and warning reporting
- ST2U now uses "error tiles" when something goes wrong with importing textures or tilesets.
- Bug fix: Packaged zip is now compatible with Linux

## [2.1.1] - 2023-09-14
- Bug fix: Compile errors in Collider2DExtensions.cs fixed for Unity 2022.3.9

## [2.1.0] - 2023-09-13
- Feature: Tileset Image Collection Subrects are now supported
- Bug fix: Compiler warnings for Unity Version 2022.3 and newer fixed

## [2.0.1] - 2023-08-21
- Bug fix: Fixed compile errors with Unity version 2022.1 and earlier

## [2.0.0] - 2023-08-19
- Initial submission
- Super Tiled2Unity jumps to version 2.0.0 as a package managed by the UPM

