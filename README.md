# AI Limit - Difficulty Scaling

Difficulty scaling mod for AI Limit. Pick a preset, drop it in, play.

A MelonLoader mod for **AI Limit** that lets you customize game difficulty through presets. Choose from 5 difficulty levels plus a custom preset.

## Requirements

- [MelonLoader](https://melon-loader.com/) 0.7.3+ installed for AI Limit

## Installation

1. Download and install MelonLoader for AI Limit if you haven't already
2. Copy `DifficultyScaling.dll` and the `DifficultyScaling_cfg/` folder into your game's `Mods/` directory:
   ```
   <game>/Mods/
       DifficultyScaling.dll
       DifficultyScaling_cfg/
           difficulty.03_normal
           Difficulty/
               00_custom/scaling.toml
               01_story/scaling.toml
               02_easy/scaling.toml
               03_normal/scaling.toml
               04_adept/scaling.toml
               05_hard/scaling.toml
   ```
3. **Select your difficulty** by renaming the flag file. For example, to play on Easy, rename:
   - `difficulty.03_normal` -> `difficulty.02_easy`

## How to Change Difficulty

The mod reads the empty flag file named `difficulty.<preset>` to determine which preset to load. You can use the **number** or **name** (case-insensitive):

| Flag File Name | Preset |
|---|---|
| `difficulty.00` or `difficulty.custom` | Custom (edit the TOML yourself) |
| `difficulty.01` or `difficulty.story` | Story |
| `difficulty.02` or `difficulty.easy` | Easy |
| `difficulty.03` or `difficulty.normal` | Normal |
| `difficulty.04` or `difficulty.adept` | Adept (base game values) |
| `difficulty.05` or `difficulty.hard` | Hard |

Only one `difficulty.*` file should exist at a time.

## Preset Values

| Preset | Monster Atk | Monster Def | Player Atk | Player Def | Player HP |
|---|---|---|---|---|---|
| Story | x0.4 | x0.4 | x1.5 | x1.2 | x1.5 |
| Easy | x0.6 | x0.6 | x1.2 | x1.2 | x1.35 |
| Normal | x0.8 | x0.85 | x1.1 | x1.1 | x1.2 |
| Adept | x1.0 | x1.0 | x1.0 | x1.0 | x1.0 |
| Hard | x1.2 | x1.25 | x0.9 | x0.9 | x0.9 |

All multipliers are relative to the base game. **Adept** is identical to vanilla.

## Custom Preset

Edit `DifficultyScaling_cfg/Difficulty/00_custom/scaling.toml` with your own values, then set the flag to `difficulty.00` or `difficulty.custom`:

```toml
monster_attack_multiplier = 0.80
monster_defense_multiplier = 0.85
player_attack_multiplier = 1.05
player_defense_multiplier = 1.05
player_hp_multiplier = 1.2
```

## Debug Logging

Add `debug = true` to any preset's `scaling.toml` to enable verbose logging in the MelonLoader console.
