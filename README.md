# AI Limit - Difficulty Scaling

Difficulty scaling mod for AI Limit. Pick a preset, drop it in, play.

A MelonLoader mod for **AI Limit** that lets you customize game difficulty through presets. Choose from 5 difficulty levels plus a custom preset. Scales monster stats, player stats, crystal drops, and fall death height.

## Requirements

- [MelonLoader](https://melon-loader.com/) 0.7.3+ installed for AI Limit

### Steam Deck / Linux (Proton)

If running on Steam Deck or Linux via Proton, set this launch option in Steam:

```
WINEDLLOVERRIDES="version=n,b" %command%
```

Right-click AI Limit in Steam → Properties → General → Launch Options, paste the command above.

## Installation

1. Download and install MelonLoader for AI Limit if you haven't already
2. Copy `DifficultyScaling.dll` and the `DifficultyScaling_cfg/` folder into your game's `Mods/` directory:
   ```
   <game>/Mods/
       DifficultyScaling.dll
       DifficultyScaling_cfg/
           config.toml
           Difficulty/
               00_custom/scaling.toml
               01_story/scaling.toml
               02_easy/scaling.toml
               03_normal/scaling.toml
               04_adept/scaling.toml
               05_hard/scaling.toml
   ```

## How to Change Difficulty

Edit `DifficultyScaling_cfg/config.toml` and set the `difficulty` value:

```toml
difficulty = "03_normal"
```

Supports preset name or number (case-insensitive):

| Value | Preset |
|---|---|
| `00` or `custom` | Custom (edit the TOML yourself) |
| `01` or `story` | Story |
| `02` or `easy` | Easy |
| `03` or `normal` | Normal |
| `04` or `adept` | Adept (base game values) |
| `05` or `hard` | Hard |

### Multi-User (Windows Only)

If multiple people share the same PC, you can set per-user overrides in `config.toml`:

```toml
difficulty = "03_normal"

[users]
Alice = "01_story"
Bob = "05_hard"
```

The mod detects the Windows username and loads that user's preset. Users not listed fall back to the default.

## Preset Values

| Preset | Monster Atk | Monster Def | Player Atk | Player Def | Player HP | Crystal | Fall Height |
|---|---|---|---|---|---|---|---|
| Story | x0.4 | x0.4 | x1.5 | x1.2 | x1.5 | x1.5 | x1.5 |
| Easy | x0.6 | x0.6 | x1.2 | x1.2 | x1.35 | x1.3 | x1.35 |
| Normal | x0.8 | x0.85 | x1.1 | x1.1 | x1.2 | x1.15 | x1.2 |
| Adept | x1.0 | x1.0 | x1.0 | x1.0 | x1.0 | x1.0 | x1.0 |
| Hard | x1.2 | x1.25 | x0.9 | x0.9 | x0.9 | x0.95 | x1.0 |

All multipliers are relative to the base game. **Adept** is identical to vanilla.

### What Each Setting Does

- **Monster Atk** — multiplies damage enemies deal to you
- **Monster Def** — multiplies enemy defense stats (damage reduction, resistances)
- **Player Atk** — multiplies damage you deal to enemies
- **Player Def** — multiplies your defense stats (damage reduction, resistances)
- **Player HP** — multiplies your max HP
- **Crystal** — multiplies crystal drops from enemies
- **Fall Height** — multiplies the height threshold before fall death triggers (does not affect the 15-second void timer)

## Custom Preset

Edit `DifficultyScaling_cfg/Difficulty/00_custom/scaling.toml` with your own values, then set `config.toml` to `difficulty = "00"` or `difficulty = "custom"`:

```toml
monster_attack_multiplier = 0.80
monster_defense_multiplier = 0.85
player_attack_multiplier = 1.05
player_defense_multiplier = 1.05
player_hp_multiplier = 1.2
crystal_multiplier = 1.0
fall_death_height_multiplier = 1.0
```

## Changelog

**v1.2.6** — Fixed quoted TOML keys breaking Windows per-user difficulty overrides.

**v1.2.5** — Fixed fall death height scaling. Removed GetDeadHigh patch.

**v1.2.4** — Migrated to config.toml with Windows multi-user support.

## Debug Logging

Add `debug = true` to any preset's `scaling.toml` to enable verbose logging in the MelonLoader console.
