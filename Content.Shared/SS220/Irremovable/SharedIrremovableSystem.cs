// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.Hands;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;

namespace Content.Shared.SS220.Irremovable;

public sealed partial class SharedIrremovableSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IrremovableComponent, GotEquippedEvent>(GotEquipped);
        SubscribeLocalEvent<IrremovableComponent, GotEquippedHandEvent>(GotPickuped);
        SubscribeLocalEvent<MobStateChangedEvent>(OnDeath);
        SubscribeLocalEvent<DropAllIrremovableEvent>(OnRemoveAll);
    }

    private void OnDeath(MobStateChangedEvent ev)
    {
        if (ev.NewMobState == MobState.Dead)
            RemoveItems(ev.Target);
    }

    private void OnRemoveAll(ref DropAllIrremovableEvent ev)
    {
        RemoveItems(ev.Target);
    }

    private void GotPickuped(Entity<IrremovableComponent> entity, ref GotEquippedHandEvent args)
    {
        if (!entity.Comp.InHandItem)
            return;

        EnsureComp<UnremoveableComponent>(entity, out var comp);
        comp.DeleteOnDrop = false;
    }

    private void GotEquipped(Entity<IrremovableComponent> entity, ref GotEquippedEvent args)
    {
        if (args.SlotFlags == SlotFlags.POCKET)
            return; // we don't want to make unremovable pocket item

        EnsureComp<UnremoveableComponent>(entity, out var comp);
        comp.DeleteOnDrop = false;
    }

    private void RemoveItems(EntityUid target)
    {
        if (!_inventory.TryGetSlots(target, out var slots))
            return;

        // trying to unequip all item's with component
        foreach (var slot in _inventory.GetHandOrInventoryEntities(target))
        {
            if (!TryComp<IrremovableComponent>(slot, out var irremovableComp))
                continue;

            if (!irremovableComp.ShouldDropOnDeath)
                continue;

            RemComp<UnremoveableComponent>(slot);
            _transform.DropNextTo(slot, target);
        }
    }
}

/// <summary>
///     Raised when we need to remove all irremovable objects
/// </summary>
[ByRefEvent, Serializable]
public sealed class DropAllIrremovableEvent : EntityEventArgs
{
    public readonly EntityUid Target;

    public DropAllIrremovableEvent(EntityUid target)
    {
        Target = target;
    }
}
