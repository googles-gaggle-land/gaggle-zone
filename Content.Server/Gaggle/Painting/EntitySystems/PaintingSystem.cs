using System.Numerics;
using Content.Server.Popups;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Gaggle.Painting;
using Content.Shared.Nutrition.Components;

namespace Content.Server.Gaggle.Painting;

/// <inheritdoc/>
public sealed class PaintingSystem : SharedPaintingSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popups = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    private ISawmill? _paintingSawmill = null;

    public static readonly float PaintableVolume = 30;
    public static readonly float PainterVolume = 100;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PaintAbsorbentComponent, ComponentInit>(OnAbsorbentInit);
        SubscribeLocalEvent<PaintAbsorbentComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<PaintAbsorbentComponent, UserActivateInWorldEvent>(OnActivateInWorld);
        SubscribeLocalEvent<PaintAbsorbentComponent, SolutionContainerChangedEvent>(OnAbsorbentSolutionChange);
        
        SubscribeLocalEvent<PaintableComponent, ComponentInit>(OnPaintableInit);

        _paintingSawmill = LogManager.GetSawmill("gaggle.painting");
    }


    // absorbent stuff i copied
    private void OnAbsorbentInit(EntityUid uid, PaintAbsorbentComponent component, ComponentInit args)
    {
        _solutionContainerSystem.EnsureSolution(uid, component.SolutionName, out _, FixedPoint2.New(PainterVolume));

        UpdateAbsorbent(uid, component);
    }

    private void OnPaintableInit(EntityUid uid, PaintableComponent component, ComponentInit args)
    {
        _solutionContainerSystem.EnsureSolution(uid, component.SolutionName, out _, FixedPoint2.New(PaintableVolume));
    }

    private void OnAbsorbentSolutionChange(EntityUid uid, PaintAbsorbentComponent component, ref SolutionContainerChangedEvent args)
    {
        UpdateAbsorbent(uid, component);
    }

    private void UpdateAbsorbent(EntityUid uid, PaintAbsorbentComponent component)
    {
        if (!_solutionContainerSystem.TryGetSolution(uid, component.SolutionName, out _, out var solution))
            return;

        var oldProgress = component.Progress.ShallowClone();
        component.Progress.Clear();

        var water = solution.GetTotalPrototypeQuantity(PaintReagents);
        if (water > FixedPoint2.Zero)
        {
            component.Progress[solution.GetColorWithOnly(_prototype, PaintReagents)] = water.Float();
        }

        var otherColor = solution.GetColorWithout(_prototype, PaintReagents);
        var other = (solution.Volume - water).Float();

        if (other > 0f)
        {
            component.Progress[otherColor] = other;
        }

        var remainder = solution.AvailableVolume;

        if (remainder > FixedPoint2.Zero)
        {
            component.Progress[Color.DarkGray] = remainder.Float();
        }

        if (component.Progress.Equals(oldProgress))
            return;

        Dirty(uid, component);
    }

    private void OnActivateInWorld(EntityUid uid, PaintAbsorbentComponent component, UserActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        PaintAbsorbentHandle(uid, args.Target, uid, component);
        args.Handled = true;
    }

    private void OnAfterInteract(EntityUid uid, PaintAbsorbentComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Handled || args.Target == null)
            return;

        PaintAbsorbentHandle(args.User, args.Target.Value, args.Used, component);
        args.Handled = true;
    }

    private void PaintAbsorbentHandle(EntityUid user, EntityUid target, EntityUid used, PaintAbsorbentComponent component)
    {
        DebugLog($"used EntityUid: {used}");
        if (!_solutionContainerSystem.TryGetSolution(used, component.SolutionName, out var absorberSoln))
            return;

        if (TryComp<UseDelayComponent>(used, out var useDelay)
            && _useDelay.IsDelayed((used, useDelay)))
            return;

        // If it's a solution container try to grab from
        if (!TryTakePaint(user, used, target, component, useDelay, absorberSoln.Value))
        {
            // If it's paintable try painting
            if (!TryPaint(user, used, target, component, useDelay, absorberSoln.Value))
                return;
        }
    }

    private void DebugLog(string message)
    {
        if (_paintingSawmill is not null)
            _paintingSawmill.Debug(message);
    }

    private bool TryTakePaint(EntityUid user, EntityUid used, EntityUid target, PaintAbsorbentComponent component, UseDelayComponent? useDelay, Entity<SolutionComponent> absorberSoln)
    {
        if (!TryComp(target, out RefillableSolutionComponent? refillable))
            return false;
        
        if (TryComp(target, out OpenableComponent? openable))
            if (!openable.Opened)
                return false;
        
        if (!_solutionContainerSystem.TryGetRefillableSolution((target, refillable, null), out var refillableSoln, out var refillableSolution))
            return false;
        
        var absorberSolution = absorberSoln.Comp.Solution;
        
        var paintPulled = component.PickupAmount < absorberSolution.AvailableVolume ?
            component.PickupAmount :
            absorberSolution.AvailableVolume;
        
        // If the container has water, try cleaning the absorbent with the water
        bool hasCleanReagent = false;
        foreach (string reagentId in PaintCleanReagents)
        {
            if (refillableSolution.ContainsReagent(reagentId,null))
            {
                hasCleanReagent = true;
                break;
            }
        }

        if (hasCleanReagent)
        {
            // Try cleaning the paint off of the paint absorbent.
            var paintRemoved = absorberSolution.Volume < refillableSolution.Volume ?
                absorberSolution.Volume :
                refillableSolution.Volume;
            
            var waterFromRefillable = refillableSolution.SplitSolutionWithOnly(paintRemoved, PaintCleanReagents);
            var paintFromAbsorbent = absorberSolution.SplitSolutionWithOnly(paintRemoved, PaintReagents);
            if (paintFromAbsorbent.Volume > 0)
            {
                _solutionContainerSystem.TryAddSolution((Entity<SolutionComponent>)refillableSoln,paintFromAbsorbent);
                
                _solutionContainerSystem.UpdateChemicals(absorberSoln);
                _solutionContainerSystem.UpdateChemicals((Entity<SolutionComponent>)refillableSoln);
                return true;
            }
            else if (refillableSolution.Volume <= 0)
                _popups.PopupEntity(Loc.GetString("painting-system-absorbent-no-paint", ("target", target)), user, user);
            
            _solutionContainerSystem.TryAddSolution((Entity<SolutionComponent>)refillableSoln,waterFromRefillable);
            _solutionContainerSystem.TryAddSolution(absorberSoln,paintFromAbsorbent);
        }

        var paintFromRefillable = refillableSolution.SplitSolutionWithOnly(paintPulled, PaintReagents);
        if (paintFromRefillable.Volume <= 0)
        {
            // No paint available
            _popups.PopupEntity(Loc.GetString("painting-system-absorbent-no-paint", ("target", target)), user, user);
            return false;
        }

        _solutionContainerSystem.TryAddSolution(absorberSoln, paintFromRefillable);
        _solutionContainerSystem.UpdateChemicals((Entity<SolutionComponent>)refillableSoln);

        _audio.PlayPvs(component.PickupSound, target);
        if (useDelay is not null)
            _useDelay.TryResetDelay((used, useDelay));
        
        return true;
    }

    private bool TryPaint(EntityUid user, EntityUid used, EntityUid target, PaintAbsorbentComponent component, UseDelayComponent? useDelay, Entity<SolutionComponent> absorberSoln)
    {
        if (!TryComp<PaintableComponent>(target,out var paintable))
        {
            return false;
        }
        
        if (!_solutionContainerSystem.ResolveSolution(target,paintable.SolutionName,ref paintable.Solution,out var paintableSolution))
        {
            if (_paintingSawmill is not null)
                _paintingSawmill.Warning($"Unable to resolve {paintable.SolutionName} solution for Entity {target}");
            return false;
        }
        DebugLog($"Paintable solution {paintable.SolutionName} resolved");
        
        var transferAmount = component.PaintAmount < paintableSolution.AvailableVolume ?
            component.PaintAmount :
            paintableSolution.AvailableVolume;
        
        DebugLog($"PaintAbsorbentComponent.PaintAmount: {component.PaintAmount}");
        DebugLog($"paintableSolution.AvailableVolume: {paintableSolution.AvailableVolume}");
        DebugLog($"paintableSolution.MaxVolume: {paintableSolution.MaxVolume}");
        DebugLog($"paintableSolution.Volume: {paintableSolution.Volume}");
        DebugLog($"Transfer amount: {transferAmount}");
        
        var paintedSolution = _solutionContainerSystem.SplitSolution(absorberSoln, transferAmount);
        if (paintedSolution.Volume <= 0)
        {
            if (paintableSolution.AvailableVolume <= 0)
                _popups.PopupEntity(Loc.GetString("painting-system-absorbent-paint-full", ("target", target)), user, user);
            else
                _popups.PopupEntity(Loc.GetString("painting-system-no-paint"), user, user);
            return false;
        }
        _solutionContainerSystem.TryAddSolution((Entity<SolutionComponent>)paintable.Solution,paintedSolution);
                                                //????????????????????????? watever
        _solutionContainerSystem.UpdateChemicals((Entity<SolutionComponent>)paintable.Solution);
        
        // audio
        _audio.PlayPvs(component.TransferSound, target);
        if (useDelay != null)
            _useDelay.TryResetDelay((used, useDelay));
        
        // lunge
        var userXform = Transform(user);
        var targetPos = _transform.GetWorldPosition(target);
        var localPos = Vector2.Transform(targetPos, _transform.GetInvWorldMatrix(userXform));
        localPos = userXform.LocalRotation.RotateVec(localPos);

        _melee.DoLunge(user, used, Angle.Zero, localPos, null, false);
        
        return true;
    }
}
