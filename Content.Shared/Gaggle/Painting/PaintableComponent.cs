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
    ///     How much solution we can transfer in one interaction.
    /// </summary>
    [ViewVariables]
    public Entity<SolutionComponent>? Solution = null;
}