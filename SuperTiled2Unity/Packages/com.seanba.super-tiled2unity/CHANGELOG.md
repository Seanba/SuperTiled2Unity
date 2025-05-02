# Changelog

## [2.3.1] - 2025-05-02
- Bug fix: Can now import Aseprite tilesets that do not export an animation clip.
- Bug fix: Can now import Aseprite tilesets that only have one frame (and do not animate).

## [2.3.0] - 2025-04-21
- New Feature: Now supports Aseprite files for animated tiles.
- New Feature: New Tileset Atlas asset type added. This allows us to easily add all sprites in a TSX tileset to a Unity Sprite Atlas, eliminating any visual seams between tiles.
- Bug fix: Super Tiled2Unity no longer tries to automatically chop up source textures into sprites. This was leading to too many issues.

## [2.2.5] - 2025-02-17
- Bug fix: Now support textures that the user wants imported as "Single Mode" sprites.

## [2.2.4] - 2024-12-08
- Bug fix: Infinite maps no longer break inheritance of Unity layer assignment
- Feature: Better reporting when Prefab Replacements invokes user code that throws exceptions
- Feature: Custom properties on Prefab Replacements supports enum values that are saved as integers in Tiled files

## [2.2.3] - 2024-12-06
- Bug fix: Fixed bug where TMX files with multiple internal tilesets could not be imported
- Improvement: Zipped package renamed to com.seanba.super-tiled2unity.zip to match Unity 6 expected embedded package name.

## [2.2.2] - 2024-12-01
- Bug fix: Automapping tiles no longer break the tileset importer
- Bug fix: Textures that were imported at a smaller than expected size were triggering the wrong error. The user is now presented with the right error information.

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

