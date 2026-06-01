using System;
using System.IO;
using System.Reflection;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;

[assembly: MelonInfo(typeof(DifficultyScaling.ModMain), "AI Limit Difficulty Scaling", "1.0.0", "CyrixJD115")]
[assembly: MelonGame("SenseGames", "AILIMIT")]

namespace DifficultyScaling;

public class ModMain : MelonMod
{
    private static float _playerAttackMult = 1.5f;
    private static float _monsterAttackMult = 0.4f;
    private static float _playerHpMult = 2.0f;
    private static float _playerDefMult = 2.0f;
    private static float _monsterDefMult = 0.4f;
    private static bool _debug = false;

    public override void OnInitializeMelon()
    {
        var cfgDir = Path.Combine(MelonLoader.Utils.MelonEnvironment.ModsDirectory, "DifficultyScaling_cfg");

        var flagFiles = Directory.GetFiles(cfgDir, "difficulty.*");
        if (flagFiles.Length == 0)
        {
            MelonLogger.Error("No difficulty.* flag file found in DifficultyScaling_cfg/. Mod will use defaults.");
            return;
        }

        var flagValue = Path.GetFileName(flagFiles[0]).Substring("difficulty.".Length).ToLowerInvariant();

        var diffDirs = Directory.GetDirectories(Path.Combine(cfgDir, "Difficulty"));
        string matchedDir = null;
        foreach (var d in diffDirs)
        {
            var folder = Path.GetFileName(d);
            var parts = folder.Split('_', 2);
            var num = parts[0];
            var name = parts.Length > 1 ? parts[1] : "";
            if (num == flagValue || name.ToLowerInvariant() == flagValue)
            {
                matchedDir = d;
                break;
            }
        }

        if (matchedDir == null)
        {
            MelonLogger.Error($"No preset matching '{flagValue}' found. Mod will use defaults.");
            return;
        }

        var configPath = Path.Combine(matchedDir, "scaling.toml");
        var cfg = TomlConfig.Parse(configPath);

        _playerAttackMult = TomlConfig.GetFloat(cfg, "player_attack_multiplier", 1.5f);
        _monsterAttackMult = TomlConfig.GetFloat(cfg, "monster_attack_multiplier", 0.4f);
        _playerHpMult = TomlConfig.GetFloat(cfg, "player_hp_multiplier", 2.0f);
        _playerDefMult = TomlConfig.GetFloat(cfg, "player_defense_multiplier", 2.0f);
        _monsterDefMult = TomlConfig.GetFloat(cfg, "monster_defense_multiplier", 0.4f);
        _debug = TomlConfig.GetBool(cfg, "debug", false);

        MelonLogger.Msg($"Difficulty Scaling loaded (preset: {Path.GetFileName(matchedDir)})");
        MelonLogger.Msg($"PlayerAtk x{_playerAttackMult}, MonsterAtk x{_monsterAttackMult}, PlayerHP x{_playerHpMult}, PlayerDef x{_playerDefMult}, MonsterDef x{_monsterDefMult}, Debug={_debug}");
    }

    public override void OnLateInitializeMelon()
    {
        var harmony = HarmonyInstance;

        var playerType = typeof(Player);
        var monsterType = typeof(Monster);

        var playerDecreaseHp = playerType.GetMethod("DecreaseHp", new Type[] { typeof(float), typeof(ActorDefine.DeadType), typeof(bool), typeof(bool), typeof(bool) });
        var monsterDecreaseHp = monsterType.GetMethod("DecreaseHp", new Type[] { typeof(float), typeof(ActorDefine.DeadType), typeof(bool), typeof(bool), typeof(bool) });
        var playerGetHpMax = playerType.GetMethod("GetHpMaxAttribute", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);

        if (_debug)
        {
            MelonLogger.Msg($"Player.DecreaseHp: {(playerDecreaseHp != null ? "FOUND" : "NOT FOUND")}");
            MelonLogger.Msg($"Monster.DecreaseHp: {(monsterDecreaseHp != null ? "FOUND" : "NOT FOUND")}");
            MelonLogger.Msg($"Player.GetHpMaxAttribute: {(playerGetHpMax != null ? "FOUND" : "NOT FOUND")}");
        }

        var prefixDecreaseHpOnPlayer = new HarmonyMethod(typeof(ModMain).GetMethod(nameof(PlayerDecreaseHpPrefix), BindingFlags.Static | BindingFlags.NonPublic));
        var prefixDecreaseHpOnMonster = new HarmonyMethod(typeof(ModMain).GetMethod(nameof(MonsterDecreaseHpPrefix), BindingFlags.Static | BindingFlags.NonPublic));
        var postfixGetHpMax = new HarmonyMethod(typeof(ModMain).GetMethod(nameof(GetHpMaxPostfix), BindingFlags.Static | BindingFlags.NonPublic));

        if (playerDecreaseHp != null)
        {
            harmony.Patch(playerDecreaseHp, prefix: prefixDecreaseHpOnPlayer);
            if (_debug) MelonLogger.Msg("Patched Player.DecreaseHp");
        }

        if (monsterDecreaseHp != null)
        {
            harmony.Patch(monsterDecreaseHp, prefix: prefixDecreaseHpOnMonster);
            if (_debug) MelonLogger.Msg("Patched Monster.DecreaseHp");
        }

        if (playerGetHpMax != null)
        {
            harmony.Patch(playerGetHpMax, postfix: postfixGetHpMax);
            if (_debug) MelonLogger.Msg("Patched Player.GetHpMaxAttribute");
        }

        var playerGetDefense = playerType.GetMethod("GetDefenseInfo", new Type[] { typeof(bool) });
        var monsterGetDefense = monsterType.GetMethod("GetDefenseInfo", new Type[] { typeof(bool) });

        if (_debug)
        {
            MelonLogger.Msg($"Player.GetDefenseInfo: {(playerGetDefense != null ? "FOUND" : "NOT FOUND")}");
            MelonLogger.Msg($"Monster.GetDefenseInfo: {(monsterGetDefense != null ? "FOUND" : "NOT FOUND")}");
        }

        var postfixPlayerDefense = new HarmonyMethod(typeof(ModMain).GetMethod(nameof(PlayerGetDefenseInfoPostfix), BindingFlags.Static | BindingFlags.NonPublic));
        var postfixMonsterDefense = new HarmonyMethod(typeof(ModMain).GetMethod(nameof(MonsterGetDefenseInfoPostfix), BindingFlags.Static | BindingFlags.NonPublic));

        if (playerGetDefense != null)
        {
            harmony.Patch(playerGetDefense, postfix: postfixPlayerDefense);
            if (_debug) MelonLogger.Msg("Patched Player.GetDefenseInfo");
        }

        if (monsterGetDefense != null)
        {
            harmony.Patch(monsterGetDefense, postfix: postfixMonsterDefense);
            if (_debug) MelonLogger.Msg("Patched Monster.GetDefenseInfo");
        }

        MelonLogger.Msg("Harmony patching complete.");
    }

    private static void PlayerDecreaseHpPrefix(ref float value)
    {
        if (_debug) MelonLogger.Msg($"[Player.DecreaseHp] value={value} -> {value * _monsterAttackMult}");
        value *= _monsterAttackMult;
    }

    private static void MonsterDecreaseHpPrefix(ref float value)
    {
        if (_debug) MelonLogger.Msg($"[Monster.DecreaseHp] value={value} -> {value * _playerAttackMult}");
        value *= _playerAttackMult;
    }

    private static void GetHpMaxPostfix(ref float __result)
    {
        if (_debug) MelonLogger.Msg($"[Player.GetHpMaxAttribute] {__result} -> {__result * _playerHpMult}");
        __result *= _playerHpMult;
    }

    private static void PlayerGetDefenseInfoPostfix(ref DefenseInfo __result)
    {
        __result.nPhysicsDamageReductionRate *= _playerDefMult;
        __result.nElectricDamageReductionRate *= _playerDefMult;
        __result.nPsychoDamageReductionRate *= _playerDefMult;
        __result.nFireDamageReductionRate *= _playerDefMult;
        __result.nPoisonsDamageReductionRate *= _playerDefMult;
        __result.nPunctureDamageReductionRate *= _playerDefMult;
        __result.nInfectDamageReductionRate *= _playerDefMult;
        __result.nGetHitConfidenceReductionRate *= _playerDefMult;
        __result.MaxHp *= _playerDefMult;
        if (_debug) MelonLogger.Msg($"[Player.GetDefenseInfo] defense scaled x{_playerDefMult}");
    }

    private static void MonsterGetDefenseInfoPostfix(ref DefenseInfo __result)
    {
        __result.nPhysicsDamageReductionRate *= _monsterDefMult;
        __result.nElectricDamageReductionRate *= _monsterDefMult;
        __result.nPsychoDamageReductionRate *= _monsterDefMult;
        __result.nFireDamageReductionRate *= _monsterDefMult;
        __result.nPoisonsDamageReductionRate *= _monsterDefMult;
        __result.nPunctureDamageReductionRate *= _monsterDefMult;
        __result.nInfectDamageReductionRate *= _monsterDefMult;
        __result.nGetHitConfidenceReductionRate *= _monsterDefMult;
        __result.MaxHp *= _monsterDefMult;
        if (_debug) MelonLogger.Msg($"[Monster.GetDefenseInfo] defense scaled x{_monsterDefMult}");
    }
}
