using System.Linq;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared.Fluids;

namespace Content.Shared.Gaggle.Painting;

/// <summary>
/// Logic for painting.
/// Haha not copied from AbsorbentSystem NOOOO nooo. no.
/// </summary>
public abstract class SharedPaintingSystem : EntitySystem
{
    /// <summary>
    /// All reagents that are considered to be paint.
    /// </summary>
    public static readonly string[] PaintReagents = {
        "PaintRed",
        "PaintOrange",
        "PaintYellow",
        "PaintGreen",
        "PaintCyan",
        "PaintBlue",
        "PaintPurple",
        "PaintMagenta",
        "PaintPink",
        "PaintBrown",
        "PaintWhite",
        "PaintGrey",
        "PaintBlack"
    };
    
    /// <summary>
    /// All reagents that can be removed from a bucket to clean off an entity with the PaintAbsorbent component.
    /// </summary>
    public static readonly string[] PaintCleanReagents = {
        "Water"
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PaintAbsorbentComponent, ComponentGetState>(OnAbsorbentGetState);
        SubscribeLocalEvent<PaintAbsorbentComponent, ComponentHandleState>(OnAbsorbentHandleState);
    }

    private void OnAbsorbentHandleState(EntityUid uid, PaintAbsorbentComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not PaintAbsorbentComponentState state)
            return;

        if (component.Progress.OrderBy(x => x.Key.ToArgb()).SequenceEqual(state.Progress))
            return;

        component.Progress.Clear();
        foreach (var item in state.Progress)
        {
            component.Progress.Add(item.Key, item.Value);
        }
    }

    private void OnAbsorbentGetState(EntityUid uid, PaintAbsorbentComponent component, ref ComponentGetState args)
    {
        args.State = new PaintAbsorbentComponentState(component.Progress);
    }

    [Serializable, NetSerializable]
    protected sealed class PaintAbsorbentComponentState : ComponentState
    {
        public Dictionary<Color, float> Progress;

        public PaintAbsorbentComponentState(Dictionary<Color, float> progress)
        {
            Progress = progress;
        }
    }
}