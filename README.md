# ModEnsemble

ModEnsemble is an alternative template/json mod loader, that avoids common
problems with built-in mod loader. It also is intended as a general modding
purpose library of various modding utility functions. Requires Unity Mod
Manager to function.

## IMPORTART

The content of `Files/JsonFix` are minor derivatives of the original in game
files. The author of the mode does NOT claim any authorship of the files
content. All rights for the content of the files belong to Pavonics Interactive.

## IMPORTANT: Installation notes

* Uncheck "Use Mods" checkbox from the mod menu of the game.
* Build the project or Download as an archive.
* Copy `$(project_location)/Result/ModEnsemble` or archive content into
  `$(game_directory)/Mods/Enabled`

## Features

* No mangling with on-disk files - all json-mod merges happen in memory
* Pick-up of new json-mods without game restart (see some limitations below)
* Allows loading non-vanilla template types without crashing
* Some yet undocumented features for extending leaf templates

## Limitations / Known Issues

* New json-mods are picked up when one of the following happens:
* * Initial loading of the game
* * New Game is started
* * Save is Loaded
* * Transitioning to main menu from game/skirmish
* The above means, that newly downloaded mods will not be active for "skirmish"
  until one of the above happens.

## Planned features
* Soft-reload of the templates is already supported, but not tested or exposed
  to be triggered. UI to trigger soft-reload will be available in 0.0.3
* Better integration of dynamic mod load with skirmish mode.

## Feature details

## AlternateTemplateLoadManager

ModEnsemble completely intercepts the template loading process of the game. It
works in the following way.

* The AlternateTemplateLoadManager then intercepts TemplateManager.Initialize
  call. In there it scans in-game template directory and mod directories, and
  builds up an internal registry of the template files.
* The above "scanning" phase has a hook `ModHook_ManualTemplateRegistration`,
  which mods can intercept to manually register templates that require custom
  handling (see below).
* Next, the AlternateTemplateLoadManager gets over its internal record, does
  in-memory merge of vanilla and modded-jsons, deserializes the result and
  pushes the objects into original TemplateManager.
* The loading process also has a hook for mods to perform post-load patching of
  templates - `ModHook_InCodeTemplateOverride`
* To accomodate the above hooks, AlternateTemplateLoadManager also patches
  certain methods in Unity Mod Manager itself to accomodate full reload of all
  templates once all mods are loaded (as well as on mod toggle).

When intercepting `ModHook_ManualTemplateRegistration` mods can register
additional entries to be loaded. This might be useful if one wants to split
their json on multiple files for ease of managing (the built-in merger behaviour
only acts on matching file names and assumes file name specifies the tempalte
type, and the alternate loader inherits that behaviour for compatibility). It
may also be necessary for `leaf templates extensions` (documentation on that
TBD). To avoid auto-load of the files you need a custom handling of, suiffix
their extension with "_".

```C#
namespace SomeMod {

    [HarmonyPatch(typeof(AlternateTemplateLoadManager), "ModHook_ManualTemplateRegistration")]
    class Loader {
        public static void Prefix() {
            ModTemplate template = new ModTemplate();
            template.typeName = "TIMissileTemplate";
            template.modName = "SomeMod";
            template.jsonFiles = new string[] {
                Main.mod.Path + "Templates/MissilesBatch1.json_",
                Main.mod.Path + "Templates/MissilesBatch2.json_" };
            AlternateTemplateLoadManager.Register(template);
        }
    }
}
```
