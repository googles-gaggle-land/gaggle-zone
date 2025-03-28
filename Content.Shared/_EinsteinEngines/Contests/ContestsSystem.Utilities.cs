using Content.Shared.CCVar;
using Robust.Shared.Serialization;

namespace Content.Shared._EinsteinEngines.Contests;
public sealed partial class ContestsSystem
{
    /// <summary>
    ///     Clamp a contest to a Range of [Epsilon, 32bit integer limit]. This exists to make sure contests are always "Safe" to divide by.
    /// </summary>
    private float ContestClamp(float input)
    {
        return Math.Clamp(input, float.Epsilon, float.MaxValue);
    }

    /// <summary>
    ///     Shorthand for checking if clamp overrides are allowed, and the bypass is used by a contest.
    /// </summary>
    private bool ContestClampOverride(bool bypassClamp)
    {
        return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp;
    }

    /// <summary>
    ///     Constructor for feeding options from a given set of ContestArgs into the ContestsSystem.
    ///     Just multiply by this and give it a user EntityUid and a ContestArgs variable. That's all you need to know.
    /// </summary>
    public float ContestConstructor(EntityUid user, ContestArgs args)
    {
        if (!_cfg.GetCVar(CCVars.DoContestsSystem))
            return 1;

        if (!args.DoEveryInteraction)
            return args.DoMassInteraction ? ((args.MassDisadvantage
                        ? MassContest(user, args.MassBypassClamp, args.MassRangeModifier)
                        : 1 / MassContest(user, args.MassBypassClamp, args.MassRangeModifier))
                            + args.MassOffset)
                                : 1
                    * (args.DoStaminaInteraction ? ((args.StaminaDisadvantage
                        ? StaminaContest(user, args.StaminaBypassClamp, args.StaminaRangeModifier)
                        : 1 / StaminaContest(user, args.StaminaBypassClamp, args.StaminaRangeModifier))
                            + args.StaminaOffset)
                                : 1)
                    * (args.DoHealthInteraction ? ((args.HealthDisadvantage
                        ? HealthContest(user, args.HealthBypassClamp, args.HealthRangeModifier)
                        : 1 / HealthContest(user, args.HealthBypassClamp, args.HealthRangeModifier))
                            + args.HealthOffset)
                                : 1);

        var everyContest = EveryContest(user,
                    args.MassBypassClamp,
                    args.StaminaBypassClamp,
                    args.HealthBypassClamp,
                    args.MassRangeModifier,
                    args.StaminaRangeModifier,
                    args.HealthRangeModifier,
                    args.EveryMassWeight,
                    args.EveryStaminaWeight,
                    args.EveryHealthWeight,
                    args.EveryInteractionSumOrMultiply);

        return !args.EveryDisadvantage ? everyContest : 1 / everyContest;
    }
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class ContestArgs
{
    /// <summary>
    ///     Controls whether this melee weapon allows for mass to factor into damage.
    /// </summary>
    [DataField]
    public bool DoMassInteraction;

    /// <summary>
    ///     When true, mass provides a disadvantage.
    /// </summary>
    [DataField]
    public bool MassDisadvantage;

    /// <summary>
    ///     When true, mass contests ignore clamp limitations for a melee weapon.
    /// </summary>
    [DataField]
    public bool MassBypassClamp;

    /// <summary>
    ///     Multiplies the acceptable range of outputs provided by mass contests for melee.
    /// </summary>
    [DataField]
    public float MassRangeModifier = 1;

    /// <summary>
    ///     The output of a mass contest is increased by this amount.
    /// </summary>
    [DataField]
    public float MassOffset;

    /// <summary>
    ///     Controls whether this melee weapon allows for stamina to factor into damage.
    /// </summary>
    [DataField]
    public bool DoStaminaInteraction;

    /// <summary>
    ///     When true, stamina provides a disadvantage.
    /// </summary>
    [DataField]
    public bool StaminaDisadvantage;

    /// <summary>
    ///     When true, stamina contests ignore clamp limitations for a melee weapon.
    /// </summary>
    [DataField]
    public bool StaminaBypassClamp;

    /// <summary>
    ///     Multiplies the acceptable range of outputs provided by mass contests for melee.
    /// </summary>
    [DataField]
    public float StaminaRangeModifier = 1;

    /// <summary>
    ///     The output of a stamina contest is increased by this amount.
    /// </summary>
    [DataField]
    public float StaminaOffset;

    /// <summary>
    ///     Controls whether this melee weapon allows for health to factor into damage.
    /// </summary>
    [DataField]
    public bool DoHealthInteraction;

    /// <summary>
    ///     When true, health contests provide a disadvantage.
    /// </summary>
    [DataField]
    public bool HealthDisadvantage;

    /// <summary>
    ///     When true, health contests ignore clamp limitations for a melee weapon.
    /// </summary>
    [DataField]
    public bool HealthBypassClamp;

    /// <summary>
    ///     Multiplies the acceptable range of outputs provided by mass contests for melee.
    /// </summary>
    [DataField]
    public float HealthRangeModifier = 1;

    /// <summary>
    ///     The output of health contests is increased by this amount.
    /// </summary>
    [DataField]
    public float HealthOffset;

    /// <summary>
    ///     Enables the EveryContest interaction for a melee weapon.
    ///     IF YOU PUT THIS ON ANY WEAPON OTHER THAN AN ADMEME, I WILL COME TO YOUR HOUSE AND SEND YOU TO MEET YOUR CREATOR WHEN THE PLAYERS COMPLAIN.
    /// </summary>
    [DataField]
    public bool DoEveryInteraction;

    /// <summary>
    ///     When true, EveryContest provides a disadvantage.
    /// </summary>
    [DataField]
    public bool EveryDisadvantage;

    /// <summary>
    ///     How much Mass is considered for an EveryContest.
    /// </summary>
    [DataField]
    public float EveryMassWeight = 1;

    /// <summary>
    ///     How much Stamina is considered for an EveryContest.
    /// </summary>
    [DataField]
    public float EveryStaminaWeight = 1;

    /// <summary>
    ///     How much Health is considered for an EveryContest.
    /// </summary>
    [DataField]
    public float EveryHealthWeight = 1;

    /// <summary>
    ///     When true, the EveryContest sums the results of all contests rather than multiplying them,
    ///     probably giving you a very, very, very large multiplier...
    /// </summary>
    [DataField]
    public bool EveryInteractionSumOrMultiply;
}
