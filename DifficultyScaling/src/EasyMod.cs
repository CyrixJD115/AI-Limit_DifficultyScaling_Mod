using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;

[assembly: MelonInfo(typeof(DifficultyScaling.ModMain), "AI Limit Difficulty Scaling", "1.2.6", "CyrixJD115")]
[assembly: MelonGame("SenseGames", "AILIMIT")]

namespace DifficultyScaling;

public class ModMain : MelonMod
{
    private static float _playerAttackMult = 1.5f;
    private static float _monsterAttackMult = 0.4f;
    private static float _playerHpMult = 2.0f;
    private static float _playerDefMult = 2.0f;
    private static float _monsterDefMult = 0.4f;
    private static float _crystalMult = 1.0f;
    private static float _fallDeathHeightMult = 1.0f;
    private static bool _debug = false;
    private static bool _crystalScaled = false;

    private const string LOG = "[AI Limit: Difficulty Scaling]";
    private const string DBG = "[AIL:DS]";

    public override void OnInitializeMelon()
    {
        var cfgDir = Path.Combine(MelonLoader.Utils.MelonEnvironment.ModsDirectory, "DifficultyScaling_cfg");
        var configPath = Path.Combine(cfgDir, "config.toml");

        if (!File.Exists(configPath))
        {
            MelonLogger.Error($"{LOG} config.toml not found in DifficultyScaling_cfg/. Mod will use defaults.");
            return;
        }

        var config = TomlConfig.Parse(configPath);
        var presetKey = TomlConfig.GetString(config, "difficulty", "03_normal");

        if (OperatingSystem.IsWindows())
        {
            var users = TomlConfig.Parse(configPath, "users");
            if (users.Count > 0)
            {
                var userProfiles = new List<string>();
                var profilesDir = @"C:\Users";
                if (Directory.Exists(profilesDir))
                {
                    foreach (var dir in Directory.GetDirectories(profilesDir))
                    {
                        var name = Path.GetFileName(dir);
                        if (name.StartsWith("Public") || name.StartsWith("Default") || name.StartsWith("All Users"))
                            continue;
                        userProfiles.Add(name);
                    }
                }

                string currentUser = Environment.UserName;
                if (userProfiles.Count > 0)
                {
                    var match = userProfiles.Find(u => u.Equals(currentUser, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                        currentUser = match;
                }

                var userPreset = TomlConfig.GetString(users, currentUser, null);
                if (userPreset != null)
                {
                    presetKey = userPreset;
                    MelonLogger.Msg($"{LOG} Windows user '{currentUser}' -> preset {presetKey}");
                }
            }
        }

        var diffDirs = Directory.GetDirectories(Path.Combine(cfgDir, "Difficulty"));
        string matchedDir = null;
        foreach (var d in diffDirs)
        {
            var folder = Path.GetFileName(d);
            var parts = folder.Split('_', 2);
            var num = parts[0];
            var name = parts.Length > 1 ? parts[1] : "";
            if (folder == presetKey || num == presetKey || name.ToLowerInvariant() == presetKey)
            {
                matchedDir = d;
                break;
            }
        }

        if (matchedDir == null)
        {
            MelonLogger.Error($"{LOG} No preset matching '{presetKey}' found. Mod will use defaults.");
            return;
        }

        var presetPath = Path.Combine(matchedDir, "scaling.toml");
        var cfg = TomlConfig.Parse(presetPath);

        _playerAttackMult = TomlConfig.GetFloat(cfg, "player_attack_multiplier", 1.5f);
        _monsterAttackMult = TomlConfig.GetFloat(cfg, "monster_attack_multiplier", 0.4f);
        _playerHpMult = TomlConfig.GetFloat(cfg, "player_hp_multiplier", 2.0f);
        _playerDefMult = TomlConfig.GetFloat(cfg, "player_defense_multiplier", 2.0f);
        _monsterDefMult = TomlConfig.GetFloat(cfg, "monster_defense_multiplier", 0.4f);
        _crystalMult = TomlConfig.GetFloat(cfg, "crystal_multiplier", 1.0f);
        _fallDeathHeightMult = TomlConfig.GetFloat(cfg, "fall_death_height_multiplier", _playerHpMult);
        _debug = TomlConfig.GetBool(cfg, "debug", false);

        MelonLogger.Msg($"{LOG} Loaded preset: {Path.GetFileName(matchedDir)}");
        MelonLogger.Msg($"{LOG} PlayerAtk x{_playerAttackMult} | MonsterAtk x{_monsterAttackMult} | PlayerDef x{_playerDefMult} | MonsterDef x{_monsterDefMult} | PlayerHP x{_playerHpMult} | Crystal x{_crystalMult} | FallHeight x{_fallDeathHeightMult}");
    }

    public override void OnLateInitializeMelon()
    {
        var harmony = HarmonyInstance;

        var playerType = typeof(Player);
        var monsterType = typeof(Monster);

        var patches = new (string Name, MethodInfo Method, MethodInfo Prefix, MethodInfo Postfix)[]
        {
            ("Player.DecreaseHp",       playerType.GetMethod("DecreaseHp", new Type[] { typeof(float), typeof(ActorDefine.DeadType), typeof(bool), typeof(bool), typeof(bool) }),
                typeof(ModMain).GetMethod(nameof(PlayerDecreaseHpPrefix), BindingFlags.Static | BindingFlags.NonPublic), null),
            ("Monster.DecreaseHp",      monsterType.GetMethod("DecreaseHp", new Type[] { typeof(float), typeof(ActorDefine.DeadType), typeof(bool), typeof(bool), typeof(bool) }),
                typeof(ModMain).GetMethod(nameof(MonsterDecreaseHpPrefix), BindingFlags.Static | BindingFlags.NonPublic), null),
            ("Player.GetHpMaxAttribute", playerType.GetMethod("GetHpMaxAttribute", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null),
                null, typeof(ModMain).GetMethod(nameof(GetHpMaxPostfix), BindingFlags.Static | BindingFlags.NonPublic)),
            ("Player.GetHPMax", playerType.GetMethod("GetHPMax", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null),
                null, typeof(ModMain).GetMethod(nameof(GetHpMaxPostfix), BindingFlags.Static | BindingFlags.NonPublic)),
            ("Player.GetDefenseInfo",   playerType.GetMethod("GetDefenseInfo", new Type[] { typeof(bool) }),
                null, typeof(ModMain).GetMethod(nameof(PlayerGetDefenseInfoPostfix), BindingFlags.Static | BindingFlags.NonPublic)),
            ("Monster.GetDefenseInfo",  monsterType.GetMethod("GetDefenseInfo", new Type[] { typeof(bool) }),
                null, typeof(ModMain).GetMethod(nameof(MonsterGetDefenseInfoPostfix), BindingFlags.Static | BindingFlags.NonPublic)),
        };

        int patched = 0;
        foreach (var (name, method, prefix, postfix) in patches)
        {
            if (method != null)
            {
                harmony.Patch(method,
                    prefix: prefix != null ? new HarmonyMethod(prefix) : null,
                    postfix: postfix != null ? new HarmonyMethod(postfix) : null);
                MelonLogger.Msg($"{LOG}   \u2713 {name}");
                patched++;
            }
            else
            {
                MelonLogger.Msg($"{LOG}   \u2717 {name} \u2014 NOT FOUND");
            }
        }

        MelonLogger.Msg($"{LOG} Patched {patched}/{patches.Length} methods");
    }

    public override void OnUpdate()
    {
        if (_crystalScaled || _crystalMult == 1.0f) return;

        try
        {
            var configData = GlobalConfig.ConfigData;
            if (configData == null) return;

            var monsterDrops = configData.MonsterDrop;
            if (monsterDrops == null || monsterDrops.Count == 0) return;

            int count = monsterDrops.Count;
            for (int i = 0; i < count; i++)
            {
                var drop = monsterDrops[i];
                if (drop == null) continue;
                drop.Money = (int)Math.Round(drop.Money * _crystalMult);
            }

            _crystalScaled = true;
            MelonLogger.Msg($"{LOG} Crystal x{_crystalMult} applied to {count} MonsterDrop entries");
        }
        catch (Exception ex)
        {
            if (_debug) MelonLogger.Msg($"{DBG} Crystal scaling waiting for data... ({ex.Message})");
        }
    }

    private static void PlayerDecreaseHpPrefix(ref float value, ActorDefine.DeadType deadType)
    {
        if (deadType == ActorDefine.DeadType.FallDead)
        {
            if (_debug) MelonLogger.Msg($"{DBG} Player DMG   {value} SKIP (void fall timer not scaled)");
            return;
        }
        if (deadType == ActorDefine.DeadType.TouchGroundDead)
        {
            value /= _fallDeathHeightMult;
            if (_debug) MelonLogger.Msg($"{DBG} Player DMG   {value} (fall height x{_fallDeathHeightMult})");
            return;
        }
        if (_debug) MelonLogger.Msg($"{DBG} Player DMG   {value} \u2192 {value * _monsterAttackMult} (x{_monsterAttackMult})");
        value *= _monsterAttackMult;
    }

    private static void MonsterDecreaseHpPrefix(ref float value)
    {
        if (_debug) MelonLogger.Msg($"{DBG} Monster HP   {value} \u2192 {value * _playerAttackMult} (x{_playerAttackMult})");
        value *= _playerAttackMult;
    }

    private static void GetHpMaxPostfix(ref float __result)
    {
        if (_debug) MelonLogger.Msg($"{DBG} Player HP    {__result} \u2192 {__result * _playerHpMult} (x{_playerHpMult})");
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
        if (_debug) MelonLogger.Msg($"{DBG} Player Def   scaled x{_playerDefMult}");
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
        if (_debug) MelonLogger.Msg($"{DBG} Monster Def  scaled x{_monsterDefMult}");
    }
}
