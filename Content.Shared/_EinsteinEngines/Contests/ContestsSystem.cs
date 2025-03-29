using Content.Shared.CCVar;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Physics.Components;

namespace Content.Shared._EinsteinEngines.Contests;

public sealed partial class ContestsSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

    /// <summary>
    ///     The presumed average mass of a player entity
    ///     Defaulted to the average mass of an adult human
    /// </summary>
    private const float AverageMass = 71f;


    #region Mass Contests
    /// <summary>
    ///     Outputs the ratio of mass between a performer and the average human mass
    /// </summary>
    /// <remarks>
    ///     bypassClamp is a deprecated input intended for supporting legacy Nyanotrasen systems. Do not use it if you don't know what you're doing.
    /// </remarks>
    public float MassContest(EntityUid performerUid, bool bypassClamp = false, float rangeFactor = 1f, float otherMass = AverageMass)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem)
            || !_cfg.GetCVar(CCVars.DoMassContests)
            || !TryComp<PhysicsComponent>(performerUid, out var performerPhysics)
            || performerPhysics.Mass == 0)
            return 1f;

        return ContestClamp(ContestClampOverride(bypassClamp)
            ? performerPhysics.Mass / otherMass
            : Math.Clamp(performerPhysics.Mass / otherMass,
                1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor));
    }

    /// <inheritdoc cref="MassContest(EntityUid, bool, float, float)"/>
    /// <remarks>
    ///     MaybeMassContest, in case your entity doesn't exist
    /// </remarks>
    public float MassContest(EntityUid? performerUid, bool bypassClamp = false, float rangeFactor = 1f, float otherMass = AverageMass)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem)
            || !_cfg.GetCVar(CCVars.DoMassContests)
            || performerUid is null)
            return 1f;

        return MassContest(performerUid.Value, bypassClamp, rangeFactor, otherMass);
    }

    /// <summary>
    ///     Outputs the ratio of mass between a performer and the average human mass
    ///     If a function already has the performer's physics component, this is faster
    /// </summary>
    /// <remarks>
    ///     bypassClamp is a deprecated input intended for supporting legacy Nyanotrasen systems. Do not use it if you don't know what you're doing.
    /// </remarks>
    public float MassContest(PhysicsComponent performerPhysics, bool bypassClamp = false, float rangeFactor = 1f, float otherMass = AverageMass)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem)
            || !_cfg.GetCVar(CCVars.DoMassContests)
            || performerPhysics.Mass == 0)
            return 1f;

        return ContestClamp(ContestClampOverride(bypassClamp)
            ? performerPhysics.Mass / otherMass
            : Math.Clamp(performerPhysics.Mass / otherMass,
                1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor));
    }

    /// <summary>
    ///     Outputs the ratio of mass between a performer and a target, accepts either EntityUids or PhysicsComponents in any combination
    ///     If you have physics components already in your function, use <see cref="MassContest(PhysicsComponent, float)" /> instead
    /// </summary>
    /// <remarks>
    ///     bypassClamp is a deprecated input intended for supporting legacy Nyanotrasen systems. Do not use it if you don't know what you're doing.
    /// </remarks>
    public float MassContest(EntityUid performerUid, EntityUid targetUid, bool bypassClamp = false, float rangeFactor = 1f)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem)
            || !_cfg.GetCVar(CCVars.DoMassContests)
            || !TryComp<PhysicsComponent>(performerUid, out var performerPhysics)
            || !TryComp<PhysicsComponent>(targetUid, out var targetPhysics)
            || performerPhysics.Mass == 0
            || targetPhysics.InvMass == 0)
            return 1f;

        return ContestClamp(ContestClampOverride(bypassClamp)
            ? performerPhysics.Mass * targetPhysics.InvMass
            : Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass,
                1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor));
    }

    /// <inheritdoc cref="MassContest(EntityUid, EntityUid, bool, float)"/>
    public float MassContest(EntityUid performerUid, PhysicsComponent targetPhysics, bool bypassClamp = false, float rangeFactor = 1f)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem)
            || !_cfg.GetCVar(CCVars.DoMassContests)
            || !TryComp<PhysicsComponent>(performerUid, out var performerPhysics)
            || performerPhysics.Mass == 0
            || targetPhysics.InvMass == 0)
            return 1f;

        return ContestClamp(ContestClampOverride(bypassClamp)
            ? performerPhysics.Mass * targetPhysics.InvMass
            : Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass,
                1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor));
    }

    /// <inheritdoc cref="MassContest(EntityUid, EntityUid, bool, float)"/>
    public float MassContest(PhysicsComponent performerPhysics, EntityUid targetUid, bool bypassClamp = false, float rangeFactor = 1f)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem)
            || !_cfg.GetCVar(CCVars.DoMassContests)
            || !TryComp<PhysicsComponent>(targetUid, out var targetPhysics)
            || performerPhysics.Mass == 0
            || targetPhysics.InvMass == 0)
            return 1f;

        return ContestClamp(ContestClampOverride(bypassClamp)
            ? performerPhysics.Mass * targetPhysics.InvMass
            : Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass,
                1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor));
    }

    /// <inheritdoc cref="MassContest(EntityUid, EntityUid, bool, float)"/>
    public float MassContest(PhysicsComponent performerPhysics, PhysicsComponent targetPhysics, bool bypassClamp = false, float rangeFactor = 1f)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem)
            || !_cfg.GetCVar(CCVars.DoMassContests)
            || performerPhysics.Mass == 0
            || targetPhysics.InvMass == 0)
            return 1f;

        return ContestClamp(ContestClampOverride(bypassClamp)
            ? performerPhysics.Mass * targetPhysics.InvMass
            : Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass,
                1 - _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor,
                1 + _cfg.GetCVar(CCVars.MassContestsMaxPercentage) * rangeFactor));
    }

    #endregion
    #region Stamina Contests

    /// <summary>
    ///     Outputs 1 minus the percentage of an Entity's Stamina, with a Range of [Epsilon, 1 - 0.25 * rangeFactor], or a range of [Epsilon, 1 - Epsilon] if bypassClamp is true.
    ///     This will never return a value >1.
    /// </summary>
    /// <remarks>
    ///     bypassClamp is a deprecated input intended for supporting legacy Nyanotrasen systems. Do not use it if you don't know what you're doing.
    /// </remarks>
    public float StaminaContest(EntityUid performer, bool bypassClamp = false, float rangeFactor = 1f)
    {
        if (!TryComp<StaminaComponent>(performer, out var perfStamina)
            || perfStamina.StaminaDamage == 0)
            return 1f;

        return StaminaContest(perfStamina, bypassClamp, rangeFactor);
    }

    /// <inheritdoc cref="StaminaContest(EntityUid, bool, float)"/>
    public float StaminaContest(StaminaComponent perfStamina, bool bypassClamp = false, float rangeFactor = 1f)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem)
            || !_cfg.GetCVar(CCVars.DoStaminaContests))
            return 1f;

        return ContestClamp(ContestClampOverride(bypassClamp)
            ? 1 - perfStamina.StaminaDamage / perfStamina.CritThreshold
            : 1 - Math.Clamp(perfStamina.StaminaDamage / perfStamina.CritThreshold, 0, 0.25f * rangeFactor));
    }

    /// <summary>
    ///     Outputs the ratio of percentage of an Entity's Stamina and a Target Entity's Stamina, with a Range of [Epsilon, 0.25 * rangeFactor], or a range of [Epsilon, +inf] if bypassClamp is true.
    ///     This does NOT produce the same kind of outputs as a Single-Entity StaminaContest. 2Entity StaminaContest returns the product of two Solo Stamina Contests, and so its values can be very strange.
    /// </summary>
    /// <remarks>
    ///     bypassClamp is a deprecated input intended for supporting legacy Nyanotrasen systems. Do not use it if you don't know what you're doing.
    /// </remarks>
    public float StaminaContest(EntityUid performer, EntityUid target, bool bypassClamp = false, float rangeFactor = 1f)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem)
            || !_cfg.GetCVar(CCVars.DoStaminaContests)
            || !TryComp<StaminaComponent>(performer, out var perfStamina)
            || !TryComp<StaminaComponent>(target, out var targetStamina))
            return 1f;

        return ContestClamp(ContestClampOverride(bypassClamp)
            ? (1 - perfStamina.StaminaDamage / perfStamina.CritThreshold)
                / (1 - targetStamina.StaminaDamage / targetStamina.CritThreshold)
            : (1 - Math.Clamp(perfStamina.StaminaDamage / perfStamina.CritThreshold, 0, 0.25f * rangeFactor))
                / (1 - Math.Clamp(targetStamina.StaminaDamage / targetStamina.CritThreshold, 0, 0.25f * rangeFactor)));
    }

    #endregion

    #region Health Contests

    /// <summary>
    ///     Outputs 1 minus the percentage of an Entity's Health, with a Range of [Epsilon, 1 - 0.25 * rangeFactor], or a range of [Epsilon, 1 - Epsilon] if bypassClamp is true.
    ///     This will never return a value >1.
    /// </summary>
    /// <remarks>
    ///     bypassClamp is a deprecated input intended for supporting legacy Nyanotrasen systems. Do not use it if you don't know what you're doing.
    /// </remarks>
    public float HealthContest(EntityUid performer, bool bypassClamp = false, float rangeFactor = 1f)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem)
            || !_cfg.GetCVar(CCVars.DoHealthContests)
            || !TryComp<DamageableComponent>(performer, out var damage)
            || !TryComp<MobThresholdsComponent>(performer, out var thresholdsComponent)
            || !_mobThreshold.TryGetThresholdForState(performer, Mobs.MobState.Critical, out var threshold, thresholdsComponent))
            return 1f;

        return ContestClamp(ContestClampOverride(bypassClamp)
            ? 1 - damage.TotalDamage.Float() / threshold.Value.Float()
            : 1 - Math.Clamp(damage.TotalDamage.Float() / threshold.Value.Float(), 0, 0.25f * rangeFactor));
    }

    /// <summary>
    ///     Outputs the ratio of percentage of an Entity's Health and a Target Entity's Health, with a Range of [Epsilon, 0.25 * rangeFactor], or a range of [Epsilon, +inf] if bypassClamp is true.
    ///     This does NOT produce the same kind of outputs as a Single-Entity HealthContest. 2Entity HealthContest returns the product of two Solo Health Contests, and so its values can be very strange.
    /// </summary>
    /// <remarks>
    ///     bypassClamp is a deprecated input intended for supporting legacy Nyanotrasen systems. Do not use it if you don't know what you're doing.
    /// </remarks>
    public float HealthContest(EntityUid performer, EntityUid target, bool bypassClamp = false, float rangeFactor = 1f)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem)
            || !_cfg.GetCVar(CCVars.DoHealthContests)
            || !TryComp<DamageableComponent>(performer, out var perfDamage)
            || !TryComp<DamageableComponent>(target, out var targetDamage)
            || !TryComp<MobThresholdsComponent>(performer, out var perfThresholdComp)
            || !TryComp<MobThresholdsComponent>(target, out var targetThresholdComp)
            || !_mobThreshold.TryGetThresholdForState(performer, Mobs.MobState.Critical, out var perfThreshold, perfThresholdComp)
            || !_mobThreshold.TryGetThresholdForState(target, Mobs.MobState.Critical, out var targetThreshold, targetThresholdComp))
            return 1f;

        return ContestClamp(ContestClampOverride(bypassClamp)
            ? (1 - perfDamage.TotalDamage.Float() / perfThreshold.Value.Float())
                / (1 - targetDamage.TotalDamage.Float() / targetThreshold.Value.Float())
            : (1 - Math.Clamp(perfDamage.TotalDamage.Float() / perfThreshold.Value.Float(), 0, 0.25f * rangeFactor))
                / (1 - Math.Clamp(targetDamage.TotalDamage.Float() / targetThreshold.Value.Float(), 0, 0.25f * rangeFactor)));
    }
    #endregion

    #region EVERY CONTESTS

    /// <summary>
    ///     EveryContest takes either the Sum or Product of all existing contests, for if you want to just check if somebody is absolutely fucked up.
    /// </summary>
    /// <returns>
    ///     If it's not immediately obvious that a function with 16 optional inputs is a joke, please take a step back and re-evaluate why you're using this function.
    ///     All prior warnings also apply here. Bypass the clamps at your own risk. By calling this function in your system, you hereby agree to forfeit your soul to VMSolidus if bugs occur.
    /// </returns>
    public float EveryContest(
        EntityUid performer,
        bool bypassClampMass = false,
        bool bypassClampStamina = false,
        bool bypassClampHealth = false,
        float rangeFactorMass = 1f,
        float rangeFactorStamina = 1f,
        float rangeFactorHealth = 1f,
        float weightMass = 1f,
        float weightStamina = 1f,
        float weightHealth = 1f,
        bool sumOrMultiply = false)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem))
            return 1f;

        var weightTotal = weightMass + weightStamina + weightHealth;
        var massMultiplier = weightMass / weightTotal;
        var staminaMultiplier = weightStamina / weightTotal;
        var healthMultiplier = weightHealth / weightTotal;

        return sumOrMultiply
            ? MassContest(performer, bypassClampMass, rangeFactorMass) * massMultiplier
                + StaminaContest(performer, bypassClampStamina, rangeFactorStamina) * staminaMultiplier
                + HealthContest(performer, bypassClampHealth, rangeFactorHealth) * healthMultiplier
            : ContestClamp(MassContest(performer, bypassClampMass, rangeFactorMass) * massMultiplier
                * StaminaContest(performer, bypassClampStamina, rangeFactorStamina) * staminaMultiplier
                * HealthContest(performer, bypassClampHealth, rangeFactorHealth) * healthMultiplier);
    }

    /// <summary>
    ///     EveryContest takes either the Sum or Product of all existing contests, for if you want to just check if somebody is absolutely fucked up.
    /// </summary>
    /// <returns>
    ///     If it's not immediately obvious that a function with 16 optional inputs is a joke, please take a step back and re-evaluate why you're using this function.
    ///     All prior warnings also apply here. Bypass the clamps at your own risk. By calling this function in your system, you hereby agree to forfeit your soul to VMSolidus if bugs occur.
    /// </returns>
    public float EveryContest(
        EntityUid performer,
        EntityUid target,
        bool bypassClampMass = false,
        bool bypassClampStamina = false,
        bool bypassClampHealth = false,
        float rangeFactorMass = 1f,
        float rangeFactorStamina = 1f,
        float rangeFactorHealth = 1f,
        float weightMass = 1f,
        float weightStamina = 1f,
        float weightHealth = 1f,
        bool sumOrMultiply = false)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem))
            return 1f;

        var weightTotal = weightMass + weightStamina + weightHealth;
        var massMultiplier = weightMass / weightTotal;
        var staminaMultiplier = weightStamina / weightTotal;
        var healthMultiplier = weightHealth / weightTotal;

        return sumOrMultiply
            ? MassContest(performer, target, bypassClampMass, rangeFactorMass) * massMultiplier
                + StaminaContest(performer, target, bypassClampStamina, rangeFactorStamina) * staminaMultiplier
                + HealthContest(performer, target, bypassClampHealth, rangeFactorHealth) * healthMultiplier
            : ContestClamp(MassContest(performer, target, bypassClampMass, rangeFactorMass) * massMultiplier
                * StaminaContest(performer, target, bypassClampStamina, rangeFactorStamina) * staminaMultiplier
                * HealthContest(performer, target, bypassClampHealth, rangeFactorHealth) * healthMultiplier);
    }
    #endregion
}
