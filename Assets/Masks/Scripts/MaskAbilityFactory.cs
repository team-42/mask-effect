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