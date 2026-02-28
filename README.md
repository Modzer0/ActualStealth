# Nuclear Option RCS Mod

A [BepInEx](https://github.com/BepInEx/BepInEx) mod for [Nuclear Option](https://store.steampowered.com/app/1826330/Nuclear_Option/) that modifies radar cross-section (RCS) values for select aircraft and enhances the Medusa's radome radar.

## Features

- Configurable RCS reduction for the Vortex (SmallFighter1), Ifrit (Multirole1), and Darkreach
- Configurable signal strength multiplier for the Medusa (EW1) radome
- Custom loading screen hints
- All values tunable via BepInEx config file

## Installation

1. Install [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases) for Unity (Mono) into your Nuclear Option game directory
2. Copy `NuclearOptionRCSMod.dll` into `BepInEx/plugins/`
3. Launch the game — a config file will be generated at `BepInEx/config/com.nuclearoption.rcsmod.cfg`

## Configuration

Edit `BepInEx/config/com.nuclearoption.rcsmod.cfg` after first launch:

```ini
[RCS Divisors]
# RCS divisor for the FS-20B Vortex (SmallFighter1)
Vortex = 100

# RCS divisor for the KR-67A Ifrit (Multirole1)
Ifrit = 100

# RCS divisor for the SFB-81 Darkreach
Darkreach = 100

[Radar]
# Multiplier for the Medusa (EW1) radome signal strength (applied to maxRange)
MedusaRadarMultiplier = 2
```

The RCS divisor divides the aircraft's base RCS value. A divisor of 100 means the aircraft's RCS becomes 1/100th of its original value.

The radar multiplier is applied to the radome's `maxRange` parameter, which directly scales signal strength in the detection formula.

## How It Works

RCS modifications apply globally to all instances of the affected aircraft (player, friendly AI, and enemy AI). The radar multiplier only affects the Medusa's Radome1 equipment, not its built-in nose radar.

### Detection Formula

The game calculates radar signal strength as:

```
signal = maxRange / distance × RCS^0.25
```

So dividing RCS by 100 reduces detection range to ~31.6% of normal (100^0.25 = √10 ≈ 3.16x reduction). Doubling the radome's maxRange doubles its signal strength at any given distance.

## Building

Requires references to the game's managed assemblies (`Assembly-CSharp.dll`, `UnityEngine.dll`, etc.) and BepInEx core libraries.

```
dotnet build -c Release
```

## License

This project is licensed under the GNU General Public License v3.0. See [LICENSE](LICENSE) for details.
