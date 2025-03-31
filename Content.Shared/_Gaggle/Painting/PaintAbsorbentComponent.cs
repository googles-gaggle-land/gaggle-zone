using Content.Shared.Audio;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Gaggle.Painting;

/// <summary>
/// For entities that can pick up paint from a bucket and paint
/// TOTALLY not copied and pasted. No.. no...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PaintAbsorbentComponent : Component
{
    public const string SolutionName = "paint";

    public Dictionary<Color, float> Progress = new();

    /// <summary>
    /// How much solution we can transfer in one interaction.
    /// </summary>
    [DataField("pickupAmount")]
    public FixedPoint2 PickupAmount = FixedPoint2.New(100);

    [DataField("pickupSound")]
    public SoundSpecifier PickupSound = new SoundPathSpecifier("/Audio/Effects/Fluids/watersplash.ogg")
    {
        Params = AudioParams.Default.WithVariation(SharedContentAudioSystem.DefaultVariation),
    };

    [DataField("transferSound")]
    public SoundSpecifier TransferSound = new SoundPathSpecifier("/Audio/_gaggle/Effects/paint.ogg")
    {
        Params = AudioParams.Default.WithVariation(SharedContentAudioSystem.DefaultVariation).WithVolume(-3f),
    };

    public static readonly SoundSpecifier DefaultTransferSound = new SoundPathSpecifier("/Audio/_gaggle/Effects/paint.ogg")
    {
        Params = AudioParams.Default.WithVariation(SharedContentAudioSystem.DefaultVariation).WithVolume(-3f),
    };
}