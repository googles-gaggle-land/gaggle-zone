using Content.Shared.Interaction;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Toggleable;
using Content.Shared.Tools.Systems;
using Robust.Shared.Random;

namespace Content.Shared._gaggle.Weapons.Melee.Gaslight;

public sealed class GaslightSystem : EntitySystem
{
    [Dependency] private readonly SharedRgbLightControllerSystem _rgbSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GaslightComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<GaslightComponent, InteractUsingEvent>(OnInteractUsing);
    }
    // Used to pick a random color for the blade on map init.
    private void OnMapInit(Entity<GaslightComponent> entity, ref MapInitEvent args)
    {
        if (entity.Comp.ColorOptions.Count != 0)
        {
            entity.Comp.ActivatedColor = _random.Pick(entity.Comp.ColorOptions);
            Dirty(entity);
        }

        if (!TryComp(entity, out AppearanceComponent? appearanceComponent))
            return;

        _appearance.SetData(entity, ToggleableLightVisuals.Color, entity.Comp.ActivatedColor, appearanceComponent);
    }

    // Used to make the blade multicolored when using a multitool on it.
    private void OnInteractUsing(Entity<GaslightComponent> entity, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!_toolSystem.HasQuality(args.Used, SharedToolSystem.PulseQuality))
            return;

        args.Handled = true;
        entity.Comp.Hacked = !entity.Comp.Hacked;

        if (entity.Comp.Hacked)
        {
            var rgb = EnsureComp<RgbLightControllerComponent>(entity);
            _rgbSystem.SetCycleRate(entity, entity.Comp.CycleRate, rgb);
        }
        else
            RemComp<RgbLightControllerComponent>(entity);

        Dirty(entity);
    }
}
