using UnityEditor;
using UnityEngine;

namespace MaskEffect
{
    public static class CreateExpansionAssets
    {
        [MenuItem("MaskEffect/Fix Chassis Scales")]
        public static void FixChassisScales()
        {
            // Sniper: 35.3 units tall mesh → target ~0.3 in-game (like Scout)
            var sniper = AssetDatabase.LoadAssetAtPath<ChassisData>("Assets/Resources/Data/Chassis/Sniper.asset");
            if (sniper != null)
            {
                sniper.chassisScale = new Vector3(0.009f, 0.009f, 0.009f);
                sniper.indicatorHeight = 1.5f;
                sniper.indicatorRadius = 0.3f;
                EditorUtility.SetDirty(sniper);
                Debug.Log($"Sniper scale fixed to {sniper.chassisScale}");
            }

            // Colossus: 77.2 units tall mesh → target ~1.0 in-game (bigger than Tank at 0.69)
            var colossus = AssetDatabase.LoadAssetAtPath<ChassisData>("Assets/Resources/Data/Chassis/Colossus.asset");
            if (colossus != null)
            {
                colossus.chassisScale = new Vector3(0.013f, 0.013f, 0.013f);
                colossus.moveSpeed = 0.5f;
                colossus.indicatorHeight = 2.0f;
                colossus.indicatorRadius = 0.5f;
                EditorUtility.SetDirty(colossus);
                Debug.Log($"Colossus scale fixed to {colossus.chassisScale}, speed={colossus.moveSpeed}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Chassis scales fixed!");
        }

        [MenuItem("MaskEffect/Create Expansion Assets")]
        public static void CreateAll()
        {
            CreateChassisAssets();
            CreateAbilityAssets();
            CreateMaskAssets();
            UpdateExistingMasks();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Expansion assets created successfully!");
        }

        private static void CreateChassisAssets()
        {
            // Sniper
            var sniper = ScriptableObject.CreateInstance<ChassisData>();
            sniper.chassisName = "Sniper";
            sniper.chassisType = ChassisType.Sniper;
            sniper.maxHP = 60;
            sniper.armor = 5;
            sniper.attackDamage = 45;
            sniper.attackInterval = 3.0f;
            sniper.range = 5f;
            sniper.moveSpeed = 0.8f;
            sniper.evasion = 0f;
            sniper.isRanged = true;
            sniper.baseDamageType = DamageType.Physical;
            sniper.resistanceType = ResistanceType.Physical;
            sniper.canFly = false;
            sniper.hoverHeight = 0f;
            sniper.spawnPreference = SpawnPreference.Backline;
            sniper.projectileData = AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Resources/Data/Projectiles/SniperProjectileData.asset");
            sniper.chassisScale = new Vector3(0.009f, 0.009f, 0.009f);
            sniper.indicatorHeight = 1.5f;
            sniper.indicatorRadius = 0.3f;
            EnsureFolder("Assets/Resources/Data/Chassis");
            AssetDatabase.CreateAsset(sniper, "Assets/Resources/Data/Chassis/Sniper.asset");

            // Colossus
            var colossus = ScriptableObject.CreateInstance<ChassisData>();
            colossus.chassisName = "Colossus";
            colossus.chassisType = ChassisType.Colossus;
            colossus.maxHP = 250;
            colossus.armor = 30;
            colossus.attackDamage = 25;
            colossus.attackInterval = 2.5f;
            colossus.range = 1f;
            colossus.moveSpeed = 0.5f;
            colossus.evasion = 0f;
            colossus.isRanged = false;
            colossus.baseDamageType = DamageType.Physical;
            colossus.resistanceType = ResistanceType.Physical;
            colossus.canFly = false;
            colossus.hoverHeight = 0f;
            colossus.spawnPreference = SpawnPreference.FrontlineCenter;
            colossus.projectileData = AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Resources/Data/Projectiles/ColossusProjectileData.asset");
            colossus.chassisScale = new Vector3(0.013f, 0.013f, 0.013f);
            colossus.indicatorHeight = 2.0f;
            colossus.indicatorRadius = 0.5f;
            AssetDatabase.CreateAsset(colossus, "Assets/Resources/Data/Chassis/Colossus.asset");

            Debug.Log("Created Sniper and Colossus chassis assets.");
        }

        private static void CreateAbilityAssets()
        {
            EnsureFolder("Assets/Resources/Data/Abilities");

            // === Phantom mask abilities ===
            CreateAbility("ScoutPhantom_PhaseDash", "Phase Dash",
                "40% chance to become untargetable for 1s after each attack.",
                ChassisType.Scout, MaskType.Phantom, "PhaseDash",
                cooldown: 0f, duration: 1f, value1: 0.4f, value2: 0f);

            CreateAbility("JetPhantom_Cloak", "Cloak",
                "Every 8s: become invisible for 2s, next attack deals +30% damage.",
                ChassisType.Jet, MaskType.Phantom, "Cloak",
                cooldown: 8f, duration: 2f, value1: 0.3f, value2: 0f);

            CreateAbility("TankPhantom_Mirage", "Mirage",
                "Every 10s: taunt 2 nearest enemies for 3s.",
                ChassisType.Tank, MaskType.Phantom, "Mirage",
                cooldown: 10f, duration: 3f, value1: 2f, value2: 0f);

            CreateAbility("SniperPhantom_RevengeBlink", "Revenge Blink",
                "When hit: teleport to safest tile and next shot deals +50% damage. 10s cooldown.",
                ChassisType.Sniper, MaskType.Phantom, "RevengeBlink",
                cooldown: 10f, duration: 0f, value1: 0.5f, value2: 0f);

            CreateAbility("ColossusPhantom_DisplacementField", "Displacement Field",
                "Passive: 25% miss chance on all attacks against this mech.",
                ChassisType.Colossus, MaskType.Phantom, "DisplacementField",
                cooldown: 0f, duration: 0f, value1: 0.25f, value2: 0f);

            // === Commander mask abilities ===
            CreateAbility("ScoutCommander_RallyCry", "Rally Cry",
                "Every 6s: allies within 2 tiles get +15% speed for 3s.",
                ChassisType.Scout, MaskType.Commander, "RallyCry",
                cooldown: 6f, duration: 3f, value1: 0.15f, value2: 2f);

            CreateAbility("JetCommander_AirSuperiority", "Air Superiority",
                "Passive: allies in the same column receive periodic shield (10% damage reduction).",
                ChassisType.Jet, MaskType.Commander, "AirSuperiority",
                cooldown: 0f, duration: 0f, value1: 0.1f, value2: 0f);

            CreateAbility("TankCommander_IronWill", "Iron Will",
                "Passive: adjacent allies are immune to Slow and Root effects.",
                ChassisType.Tank, MaskType.Commander, "IronWill",
                cooldown: 0f, duration: 0f, value1: 0f, value2: 0f);

            CreateAbility("SniperCommander_Spotter", "Spotter",
                "Every 5s: mark current target for 4s, all allies deal +20% damage to marked target.",
                ChassisType.Sniper, MaskType.Commander, "Spotter",
                cooldown: 5f, duration: 4f, value1: 0.2f, value2: 0f);

            CreateAbility("ColossusCommander_TitanPresence", "Titan Presence",
                "Passive: allies within 3 tiles regenerate 2% max HP per second.",
                ChassisType.Colossus, MaskType.Commander, "TitanPresence",
                cooldown: 0f, duration: 0f, value1: 0.02f, value2: 3f);

            // === Existing masks x new chassis ===
            CreateAbility("SniperWarrior_ArmorPiercingRound", "Armor Piercing Round",
                "Every 3rd shot ignores 50% armor + distance bonus damage.",
                ChassisType.Sniper, MaskType.Warrior, "ArmorPiercingRound",
                cooldown: 0f, duration: 0f, value1: 0.5f, value2: 3f);

            CreateAbility("ColossusWarrior_OverwhelmingForce", "Overwhelming Force",
                "Every 4th attack stuns target for 1s. +15% bonus damage to stunned targets.",
                ChassisType.Colossus, MaskType.Warrior, "OverwhelmingForce",
                cooldown: 0f, duration: 1f, value1: 4f, value2: 0.15f);

            CreateAbility("SniperRogue_KillShot", "Kill Shot",
                "Targets below 30% HP take +75% damage. On kill: skip charge-up for next shot.",
                ChassisType.Sniper, MaskType.Rogue, "KillShot",
                cooldown: 0f, duration: 0f, value1: 0.75f, value2: 0.3f);

            CreateAbility("ColossusRogue_CrushingGrip", "Crushing Grip",
                "Every 6s: root nearest enemy for 2s and mark them for +50% damage from Colossus.",
                ChassisType.Colossus, MaskType.Rogue, "CrushingGrip",
                cooldown: 6f, duration: 2f, value1: 0.5f, value2: 0f);

            CreateAbility("SniperAngel_OverwatchProtocol", "Overwatch Protocol",
                "Every 5s: shield lowest HP ally for 15% of Sniper's attack damage.",
                ChassisType.Sniper, MaskType.Angel, "OverwatchProtocol",
                cooldown: 5f, duration: 4f, value1: 0.15f, value2: 0f);

            CreateAbility("ColossusAngel_LivingFortress", "Living Fortress",
                "Allies behind Colossus get periodic shield. Every 10s: AoE shield to allies in 3 tiles.",
                ChassisType.Colossus, MaskType.Angel, "LivingFortress",
                cooldown: 10f, duration: 5f, value1: 0.15f, value2: 3f);

            Debug.Log("Created 16 ability assets.");
        }

        private static void CreateAbility(string fileName, string abilityName, string description,
            ChassisType chassis, MaskType mask, string classId,
            float cooldown, float duration, float value1, float value2)
        {
            var ability = ScriptableObject.CreateInstance<MaskAbilityData>();
            ability.abilityName = abilityName;
            ability.description = description;
            ability.requiredChassis = chassis;
            ability.requiredMask = mask;
            ability.abilityClassId = classId;
            ability.cooldown = cooldown;
            ability.duration = duration;
            ability.value1 = value1;
            ability.value2 = value2;
            AssetDatabase.CreateAsset(ability, $"Assets/Resources/Data/Abilities/{fileName}.asset");
        }

        private static void CreateMaskAssets()
        {
            EnsureFolder("Assets/Resources/Data/Masks");

            // Phantom mask
            var phantom = ScriptableObject.CreateInstance<MaskData>();
            phantom.maskName = "Phantom";
            phantom.maskType = MaskType.Phantom;
            phantom.defaultTargetingMode = TargetingMode.LowestHP;
            phantom.bonusEvasion = 0.1f;
            phantom.damageMultiplier = 1.1f;
            phantom.maskTint = new Color(0.608f, 0.365f, 0.898f); // Violet #9B5DE5
            phantom.scoutAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/ScoutPhantom_PhaseDash.asset");
            phantom.jetAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/JetPhantom_Cloak.asset");
            phantom.tankAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/TankPhantom_Mirage.asset");
            phantom.sniperAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/SniperPhantom_RevengeBlink.asset");
            phantom.colossusAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/ColossusPhantom_DisplacementField.asset");
            AssetDatabase.CreateAsset(phantom, "Assets/Resources/Data/Masks/Phantom.asset");

            // Commander mask
            var commander = ScriptableObject.CreateInstance<MaskData>();
            commander.maskName = "Commander";
            commander.maskType = MaskType.Commander;
            commander.defaultTargetingMode = TargetingMode.Nearest;
            commander.bonusHP = 15;
            commander.bonusArmor = 5;
            commander.damageMultiplier = 1.0f;
            commander.maskTint = new Color(0f, 0.706f, 0.847f); // Light Blue #00B4D8
            commander.scoutAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/ScoutCommander_RallyCry.asset");
            commander.jetAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/JetCommander_AirSuperiority.asset");
            commander.tankAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/TankCommander_IronWill.asset");
            commander.sniperAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/SniperCommander_Spotter.asset");
            commander.colossusAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/ColossusCommander_TitanPresence.asset");
            AssetDatabase.CreateAsset(commander, "Assets/Resources/Data/Masks/Commander.asset");

            Debug.Log("Created Phantom and Commander mask assets.");
        }

        private static void UpdateExistingMasks()
        {
            // Warrior: + Sniper (ArmorPiercingRound) + Colossus (OverwhelmingForce)
            var warrior = AssetDatabase.LoadAssetAtPath<MaskData>("Assets/Resources/Data/Masks/Warrior.asset");
            if (warrior != null)
            {
                warrior.sniperAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/SniperWarrior_ArmorPiercingRound.asset");
                warrior.colossusAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/ColossusWarrior_OverwhelmingForce.asset");
                EditorUtility.SetDirty(warrior);
            }

            // Rogue: + Sniper (KillShot) + Colossus (CrushingGrip)
            var rogue = AssetDatabase.LoadAssetAtPath<MaskData>("Assets/Resources/Data/Masks/Rogue.asset");
            if (rogue != null)
            {
                rogue.sniperAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/SniperRogue_KillShot.asset");
                rogue.colossusAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/ColossusRogue_CrushingGrip.asset");
                EditorUtility.SetDirty(rogue);
            }

            // Angel: + Sniper (OverwatchProtocol) + Colossus (LivingFortress)
            var angel = AssetDatabase.LoadAssetAtPath<MaskData>("Assets/Resources/Data/Masks/Angel.asset");
            if (angel != null)
            {
                angel.sniperAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/SniperAngel_OverwatchProtocol.asset");
                angel.colossusAbility = AssetDatabase.LoadAssetAtPath<MaskAbilityData>("Assets/Resources/Data/Abilities/ColossusAngel_LivingFortress.asset");
                EditorUtility.SetDirty(angel);
            }

            Debug.Log("Updated Warrior, Rogue, Angel masks with Sniper/Colossus abilities.");
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = System.IO.Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent))
                    EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
