// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.SS220.CultYogg.Components;

[RegisterComponent, NetworkedComponent]

/// <summary>
/// Used to mark object us corrupted for exorcism
/// </summary>
public sealed partial class CultYoggCorruptedComponent : Component
{
    /// <summary>
    /// Prototype ID of the original entity, <see langword="null"/> if none.
    /// Note that this field is required to reverse corruption.
    /// </summary>
    [DataField]
    public ProtoId<EntityPrototype>? OriginalPrototypeId;
    /// <summary>
    /// Prototype ID of the corruption recipe used to currupt entity, <see langword="null"/> if none.
    /// Note that this field is required to reverse corruption.
    /// </summary>
    [DataField]
    public ProtoId<CultYoggCorruptedPrototype>? Recipe;
}
