using Content.Server.Chat.Systems;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Content.Shared.Gaggle.Traits.Assorted;
using Content.Shared.StatusEffect;
using Content.Server.Drunk;

namespace Content.Server.Gaggle.EntityEffects.Effects;

/// <summary>
///     Tries to make someone hiccup, i have no idea how else i could do this without doing something like this im sorry
///     totally not copy pasted :eyes:
/// </summary>
[UsedImplicitly]
public sealed partial class DrunkHiccup : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;

    [DataField("minimumTime")]
    /// <summary>
    ///     The minimum amount of time in seconds to hiccup.
    /// </summary>
    public float MinDrunkTime = 80;

    [DataField]
    public bool Force = false;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var uid = args.TargetEntity;

        if (!Force && !args.EntityManager.HasComponent<DrunkHiccupComponent>(uid))
            return;

        if (!args.EntityManager.TryGetComponent<StatusEffectsComponent>(uid, out var status))
            return;

        var statusSys = args.EntityManager.System<StatusEffectsSystem>();
        if (!statusSys.TryGetTime(uid, DrunkSystem.DrunkKey, out var time, status))
            return;

        var timeLeft = (float) (time.Value.Item2 - time.Value.Item1).TotalSeconds;
        if (timeLeft < MinDrunkTime)
            return;

        var chatSys = args.EntityManager.System<ChatSystem>();
        chatSys.TryEmoteWithChat(uid, "Hiccup", ChatTransmitRange.GhostRangeLimit);
    }
}
