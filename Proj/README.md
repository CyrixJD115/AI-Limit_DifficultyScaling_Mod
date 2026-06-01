# AI Limit - Difficulty Scaling

Difficulty scaling mod for AI Limit. Pick a preset, drop it in, play.

A MelonLoader mod for **AI Limit** that lets you customize game difficulty through presets. Choose from 11 difficulty levels ranging from God Mode to Torment.

## Requirements

- [MelonLoader](https://melon-loader.com/) 0.7.3+ installed for AI Limit

## Installation

1. Download and install MelonLoader for AI Limit if you haven't already
2. Copy `DifficultyScaling.dll` and the `DifficultyScaling_cfg/` folder into your game's `Mods/` directory:
   ```
   <game>/Mods/
       DifficultyScaling.dll
       DifficultyScaling_cfg/
           difficulty.05_normal
           Difficulty/
               00_custom/scaling.toml
               01_god_mode/scaling.toml
               02_story/scaling.toml
               03_relaxed/scaling.toml
               04_easy/scaling.toml
               05_normal/scaling.toml
               06_adept/scaling.toml
               07_hard/scaling.toml
               08_master/scaling.toml
               09_nightmare/scaling.toml
               10_torment/scaling.toml
   ```
3. **Select your difficulty** by renaming the flag file. For example, to play on Hard, rename:
   - `difficulty.05_normal` -> `difficulty.07_hard`

## How to Change Difficulty

The mod reads the empty flag file named `difficulty.<preset>` to determine which preset to load. You can use the **number** or **name** (case-insensitive):

| Flag File Name | Preset |
|---|---|
| `difficulty.00` or `difficulty.custom` | Custom (edit the TOML yourself) |
| `difficulty.01` or `difficulty.god_mode` | God Mode |
| `difficulty.02` or `difficulty.story` | Story |
| `difficulty.03` or `difficulty.relaxed` | Relaxed |
| `difficulty.04` or `difficulty.easy` | Easy |
| `difficulty.05` or `difficulty.normal` | Normal |
| `difficulty.06` or `difficulty.adept` | Adept (base game values) |
| `difficulty.07` or `difficulty.hard` | Hard |
| `difficulty.08` or `difficulty.master` | Master |
| `difficulty.09` or `difficulty.nightmare` | Nightmare |
| `difficulty.10` or `difficulty.torment` | Torment |

Only one `difficulty.*` file should exist at a time.

## Preset Values

| Preset | Monster Atk | Monster Def | Player Atk | Player Def | Player HP |
|---|---|---|---|---|---|
| God Mode | x0.3 | x0.3 | x1.5 | x1.5 | x2.0 |
| Story | x0.4 | x0.45 | x1.2 | x1.2 | x1.5 |
| Relaxed | x0.6 | x0.65 | x1.2 | x1.2 | x1.35 |
| Easy | x0.7 | x0.75 | x1.2 | x1.2 | x1.2 |
| Normal | x0.8 | x0.85 | x1.05 | x1.05 | x1.2 |
| Adept | x1.0 | x1.0 | x1.0 | x1.0 | x1.0 |
| Hard | x1.1 | x1.15 | x1.0 | x1.0 | x0.95 |
| Master | x1.2 | x1.25 | x0.9 | x0.9 | x0.9 |
| Nightmare | x1.35 | x1.4 | x0.85 | x0.85 | x0.9 |
| Torment | x1.5 | x1.55 | x0.8 | x0.8 | x0.85 |

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
