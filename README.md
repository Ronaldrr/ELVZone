# ELVZone

Revit 2022 add-in for placing 2D camera view zones on a plan.

## Commands

- `ELVZone.Commands.PlaceViewZonesCommand` places four fan-shaped view zones for the selected camera element.
- `ELVZone.Commands.OpenViewZoneSettingsCommand` opens the WPF settings window.

## Settings

Settings are stored in:

```text
%APPDATA%/ViewZonePlugin/settings.json
```

The settings window supports:

- selecting element parameters for horizontal angle, vertical angle, mounting height, zone lengths 1-4, and total length;
- enabling or disabling every parameter mapping;
- selecting `FilledRegionType` and line style for zones 1-4;
- enabling or disabling fills and outline lines per zone;
- saving, importing, exporting, and resetting JSON settings.

## Geometry

Zones are created on the active `ViewPlan` as 2D fan sectors from the camera insertion point and facing direction.
The horizontal angle defines the fan opening. Zone lengths 1-4 are treated as consecutive segment lengths and are clamped by total length.
Arcs are approximated by line segments.

Vertical angle and mounting height are read and stored in command data, but no 3D view pyramid is created yet.

## Build

The project targets Revit 2022 and .NET Framework 4.8. Revit API references are provided by the local `EvaRevitPlugin_2022` NuGet package, the same way as in `ELVSchemes`.

```cmd
dotnet build ELVZone.csproj
```

Place `ELVZone.dll` and `ELVZone.addin` in a Revit add-ins folder. Update the `<Assembly>` path in `ELVZone.addin` if the DLL is not located next to the manifest.
