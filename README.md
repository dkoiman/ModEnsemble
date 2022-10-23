# ModEnsemble

ModEnsemble is an alternative template/json mod loader, that avoids common
problems with built-in mod loader. It also is intended as a general modding
purpose library of various modding utility functions. Requires Unity Mod
Manager to function.

## IMPORTART

The content of `Files/JsonFixes` are minor derivatives of the original in game
files. The author of the mode does NOT claim any authorship of the files
content. All rights for the content of the files belong to Pavonics Interactive.

## IMPORTANT: Installation notes

* Uncheck "Use Mods" checkbox from the mod menu of the game.
* Build the project or Download as an archive.
* Copy `$(project_location)/Result/ModEnsemble` or archive content into
  `$(game_directory)/Mods/Enabled`

## Features

* No mangling with on-disk files - all json-mod merges happen in memory
* Pick-up of new json-mods without game restart
* * Automatically when loading save / starting new game / starting skirmish
* * Manually at any time via mod menu (ctrl + F10)
* * * Soft-reload attempts to patch in existing template objects.
* * * Hard-reload recreates all the objects (and may cause crashes or
      in-game inconsistencies. Don't use it unless you know what you are doing.
* Allows loading non-vanilla template types without crashing
* Allows leaf template extensions

## Limitations / Known Issues

* stationModelResource assets are cached only once when new game, load save or
  skirmish start are done. If subsequent template reloads introduce new
  stationModelResource, the game won't properly pick those up.
* As with vanilla, the game will crash if the same stationModelResource is
  used in multiple templates.

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

    // Registering arbitrary named json files.
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

    // In-code template override. New templates can also be added here to
    // TemplateManager safely at this point.
    [HarmonyPatch(typeof(AlternateTemplateLoadManager), "ModHook_InCodeTemplateOverride")]
    class Loader {
        public static void Prefix() {
            TILaserWeaponTemplate template =
                TemplateManager.Find<TILaserWeaponTemplate>("PointDefenseLaserTurret");
            if (template == null) {
                return;
            }
            template.targetingRange_km = 1337;
        }
    }
}
```

## Leaf template extension

AlternateTemplateLoadManager provides a way to extend the leaf templates. It
particularly may be useful to define extra parameters attached to a specific
entity type (say, PD evasion chance for a missile). To do so, one needs to
intercept `ModHook_ManualTemplateRegistration` and register an extending
template type. The extending template type must inherit the base template type
and reside in global namespace.

NOTE: The merger will match tempaltes based on the original file name for
`*.json` files or based on the supplied type during manual template. The base
type should be used in those cases, not extending one.

NOTE: You can only extend leaf templates in such a way. If the base template
has child template types, they won't inherit the extended fields.

``` C#

class TIAdvMissileTemplate : TIMissileTemplate {
	public float base_evasion_chance = 0.8f;
}

namespace YourModNamespace {
    [HarmonyPatch(typeof(AlternateTemplateLoadManager), "ModHook_ManualTemplateRegistration")]
    class Loader {
        public static void Prefix() {
            ModTemplate template = new ModTemplate();
            template.typeName = "TIMissileTemplate";
            template.modName = "AdvancedMissiles";
            template.jsonFiles = new string[] { Main.mod.Path + "TIAdvMissileTemplate.json_" };
            AlternateTemplateLoadManager.Register(template);
		AlternateTemplateLoadManager.RegisterTemplateExtension(
			"TIMissileTemplate", typeof(TIAdvMissileTemplate));
        }
    }
}
```
