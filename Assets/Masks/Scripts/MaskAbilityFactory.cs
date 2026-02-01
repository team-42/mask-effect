using System;
using System.Collections.Generic;

namespace MaskEffect
{
    public static class MaskAbilityFactory
    {
        private static readonly Dictionary<string, Func<IMaskAbility>> registry = new Dictionary<string, Func<IMaskAbility>>
        {
            { "HitAndRun",      () => new HitAndRunAbility() },
            { "DiveSlash",      () => new DiveSlashAbility() },
            { "Challenge",      () => new ChallengeAbility() },
            { "ExecuteChain",   () => new ExecuteChainAbility() },
            { "Mark",           () => new MarkAbility() },
            { "Grapple",       () => new GrappleAbility() },
            { "GuardianLeap",   () => new GuardianLeapAbility() },
            { "SkyBarrier",     () => new SkyBarrierAbility() },
            { "Sanctuary",      () => new SanctuaryAbility() },
            // Phantom mask abilities
            { "PhaseDash",          () => new PhaseDashAbility() },
            { "Cloak",              () => new CloakAbility() },
            { "Mirage",             () => new MirageAbility() },
            { "RevengeBlink",       () => new RevengeBlinkAbility() },
            { "DisplacementField",  () => new DisplacementFieldAbility() },
            // Commander mask abilities
            { "RallyCry",           () => new RallyCryAbility() },
            { "AirSuperiority",     () => new AirSuperiorityAbility() },
            { "IronWill",           () => new IronWillAbility() },
            { "Spotter",            () => new SpotterAbility() },
            { "TitanPresence",      () => new TitanPresenceAbility() },
            // Existing masks x new chassis
            { "ArmorPiercingRound", () => new ArmorPiercingRoundAbility() },
            { "OverwhelmingForce",  () => new OverwhelmingForceAbility() },
            { "KillShot",           () => new KillShotAbility() },
            { "CrushingGrip",       () => new CrushingGripAbility() },
            { "OverwatchProtocol",  () => new OverwatchProtocolAbility() },
            { "LivingFortress",     () => new LivingFortressAbility() },
        };

        public static IMaskAbility Create(string abilityClassId)
        {
            if (string.IsNullOrEmpty(abilityClassId)) return null;

            if (registry.TryGetValue(abilityClassId, out var factory))
                return factory();

            UnityEngine.Debug.LogWarning($"MaskAbilityFactory: Unknown ability ID '{abilityClassId}'");
            return null;
        }
    }
}