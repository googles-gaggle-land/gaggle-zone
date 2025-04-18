using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;

namespace Content.Shared.Gaggle.Painting;

/// <summary>
///     Paintable component. Google gaggling right now.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PaintableComponent : Component
{
    /// <summary>
    ///     The solution to be painted on.
    /// </summary>
    [DataField("solution")]
    public string SolutionName {get; set;} = "paint";
    
    /// <summary>
    ///     Whether or not this can be painted on
    /// </summary>
    [DataField("canPaint")]
    public bool CanPaint {get; set;} = true;
    
    /// <summary>
    ///     Maximum volume for solution
    /// </summary>
    [DataField("maxVol")]
    // TO DO gaggle - fix this bandaid fix
    public int MaxVolume {get; set;} = 30;

    /// <summary>
    ///     How much solution we can transfer in one interaction.
    /// </summary>
    [ViewVariables]
    public Entity<SolutionComponent>? Solution = null;
}