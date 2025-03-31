using Content.Client.Fluids.UI;
using Content.Client.Items;
using Content.Shared.Fluids;
using Content.Shared.Gaggle.Painting;

namespace Content.Client.Gaggle.Painting;

/// <inheritdoc/>
public sealed class PaintingSystem : SharedPaintingSystem
{
    public override void Initialize()
    {
        base.Initialize();
        Subs.ItemStatus<PaintAbsorbentComponent>(ent => new AbsorbentItemStatus(ent, EntityManager));
    }
}
