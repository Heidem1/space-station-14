using Content.Server.SS220.Objectives.Systems;

namespace Content.Server.SS220.Objectives.Components;

/// <summary>
/// Component for a Acsension task for cultists
/// </summary>
[RegisterComponent, Access(typeof(MiGoAliveConditionSystem))]
public sealed partial class MiGoAliveConditionComponent : Component
{
    /// <summary>
    /// Amount of MiGo required to sacrifice somebody, made us task to clarify gameplay
    /// </summary>
    [DataField]
    public int reqMiGoAmount = 3;
}
