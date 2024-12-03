using System.Numerics;
using Content.Client.Computer;
using Content.Client.UserInterface.Controls;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.SS220.CruiseControl;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Map;

namespace Content.Client.Shuttles.UI;

[GenerateTypedNameReferences]
public sealed partial class ShuttleConsoleWindow : FancyWindow,
    IComputerWindow<ShuttleBoundUserInterfaceState>
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    private ShuttleConsoleMode _mode = ShuttleConsoleMode.Nav;

    public event Action<MapCoordinates, Angle>? RequestFTL;
    public event Action<NetEntity, Angle>? RequestBeaconFTL;

    public event Action<NetEntity, NetEntity>? DockRequest;
    public event Action<NetEntity>? UndockRequest;

    public event Action<bool, float>? SetCruiseControl; // SS220 Cruise-control

    public ShuttleConsoleWindow()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        // Mode switching
        NavModeButton.OnPressed += NavPressed;
        MapModeButton.OnPressed += MapPressed;
        DockModeButton.OnPressed += DockPressed;

        // Modes are exclusive
        var group = new ButtonGroup();

        NavModeButton.Group = group;
        MapModeButton.Group = group;
        DockModeButton.Group = group;

        NavModeButton.Pressed = true;
        SetupMode(_mode);

        MapContainer.RequestFTL += (coords, angle) =>
        {
            RequestFTL?.Invoke(coords, angle);
        };

        MapContainer.RequestBeaconFTL += (ent, angle) =>
        {
            RequestBeaconFTL?.Invoke(ent, angle);
        };

        DockContainer.DockRequest += (entity, netEntity) =>
        {
            DockRequest?.Invoke(entity, netEntity);
        };

        DockContainer.UndockRequest += entity =>
        {
            UndockRequest?.Invoke(entity);
        };

        // SS220 Cruise-control begin
        EnableCruiseButton.OnPressed += CruiseToggled;
        CruiseControlSlider.OnValueChanged += CruiseControlSliderChanged;
        // SS220 Cruise-control end
    }

    // SS220 Cruise-control begin
    private bool _sliderDebounce = false;

    private void UpdateInterface()
    {
        EnableCruiseButton.Text = EnableCruiseButton.Pressed ? "Вкл" : "Выкл";
        CruiseControlSlider.Disabled = !EnableCruiseButton.Pressed;

        if (!EnableCruiseButton.Pressed)
        {
            _sliderDebounce = true;
            CruiseControlSlider.Value = 0;
            _sliderDebounce = false;
        }
    }

    private void GetCruiseState(EntityUid shuttle)
    {
        if (_entManager.TryGetComponent<ShuttleCruiseControlComponent>(shuttle, out var cruiseControl))
        {
            EnableCruiseButton.Pressed = true;
            _sliderDebounce = true;
            CruiseControlSlider.Value = cruiseControl.LinearInput.Length();
            _sliderDebounce = false;
        }
        else
        {
            EnableCruiseButton.Pressed = false;
        }

        UpdateInterface();
    }

    private void CruiseToggled(BaseButton.ButtonEventArgs args)
    {
        UpdateInterface();
        SetCruiseControl?.Invoke(EnableCruiseButton.Pressed, CruiseControlSlider.Value);
    }

    private void CruiseControlSliderChanged(Robust.Client.UserInterface.Controls.Range args)
    {
        if (_sliderDebounce)
            return;

        if (!EnableCruiseButton.Pressed)
            return;

        SetCruiseControl?.Invoke(true, CruiseControlSlider.Value);
    }
    // SS220 Cruise-control end

    private void ClearModes(ShuttleConsoleMode mode)
    {
        if (mode != ShuttleConsoleMode.Nav)
        {
            NavContainer.Visible = false;
        }

        if (mode != ShuttleConsoleMode.Map)
        {
            MapContainer.Visible = false;
            MapContainer.SetMap(MapId.Nullspace, Vector2.Zero);
        }

        if (mode != ShuttleConsoleMode.Dock)
        {
            DockContainer.Visible = false;
        }
    }

    private void NavPressed(BaseButton.ButtonEventArgs obj)
    {
        SwitchMode(ShuttleConsoleMode.Nav);
    }

    private void MapPressed(BaseButton.ButtonEventArgs obj)
    {
        SwitchMode(ShuttleConsoleMode.Map);
    }

    private void DockPressed(BaseButton.ButtonEventArgs obj)
    {
        SwitchMode(ShuttleConsoleMode.Dock);
    }

    private void SetupMode(ShuttleConsoleMode mode)
    {
        switch (mode)
        {
            case ShuttleConsoleMode.Nav:
                NavContainer.Visible = true;
                break;
            case ShuttleConsoleMode.Map:
                MapContainer.Visible = true;
                MapContainer.Startup();
                break;
            case ShuttleConsoleMode.Dock:
                DockContainer.Visible = true;
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void SwitchMode(ShuttleConsoleMode mode)
    {
        if (_mode == mode)
            return;

        _mode = mode;
        ClearModes(mode);
        SetupMode(_mode);
    }

    public enum ShuttleConsoleMode : byte
    {
        Nav,
        Map,
        Dock,
    }

    public void UpdateState(EntityUid owner, ShuttleBoundUserInterfaceState cState)
    {
        var coordinates = _entManager.GetCoordinates(cState.NavState.Coordinates);
        NavContainer.SetShuttle(coordinates?.EntityId);
        NavContainer.SetConsole(owner);
        MapContainer.SetShuttle(coordinates?.EntityId);
        MapContainer.SetConsole(owner);

        NavContainer.UpdateState(cState.NavState);
        MapContainer.UpdateState(cState.MapState);
        DockContainer.UpdateState(coordinates?.EntityId, cState.DockState);

        // SS220 Cruise-control begin
        if (coordinates.HasValue)
            GetCruiseState(coordinates.Value.EntityId);
        // SS220 Cruise-control end
    }
}
