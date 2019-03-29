# SteelSeriesUnity
Custom made SteelSeries Game Sense API for Unity.

Handler configuration is complex, in this implementation handlers are configured through multiple `TextAsset` (JSON file).

# Why?
- faster, less allocations, extensible
- does not use FullSerializer
- uses reusable HttpClient
- events testable in Editor without entering PlayMode
- uses less assets for configuration
  - one global config
  - one text asset (JSON) per handler

# Scope
- same scope as original Unity implementation
- does not include support for screen handlers

# License
MIT
