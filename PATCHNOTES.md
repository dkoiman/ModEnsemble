# Patchnotes

## 0.0.3

* ModEnsemble doesn't override broken vanilla jsons anymore. It just redirects
  to the alternative data source when those are processed.
* Added on-demand soft (patch) and hard (re-create) reloading of the templates,
  accessible via mod menu (Ctrl + F10).
* Fixed Skirmish not immediately picking up new templates.
* Extended documentation on how to use ModEnsemble from other mods.
* Documented "Leaf template extension" feature.

## 0.0.2

* Initial release
* Alternate Template Load Manager introduced
* * No mangling with on-disk files - all json-mod merges happen in memory
* * Pick-up of new json-mods without game restart (with some limitations)
* * Allows loading non-vanilla template types without crashing
* * Some yet undocumented features for extending leaf templates

## 0.0.1
* Pre-release build
