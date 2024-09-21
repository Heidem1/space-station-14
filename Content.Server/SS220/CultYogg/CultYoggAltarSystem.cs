// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Server.Body.Systems;
using Content.Server.Destructible;
using Content.Server.SS220.GameTicking.Rules.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.SS220.CultYogg;
using Content.Shared.SS220.CultYogg.EntitySystems;

namespace Content.Server.SS220.CultYogg;

public sealed partial class CultYoggAltarSystem : SharedCultYoggAltarSystem
{
    [Dependency] private readonly BodySystem _body = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CultYoggAltarComponent, MiGoSacrificeDoAfterEvent>(OnDoAfter);
    }

    private void OnDoAfter(Entity<CultYoggAltarComponent> ent, ref MiGoSacrificeDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (args.Target == null)
            return;

        if (!TryComp<AppearanceComponent>(ent, out var appearanceComp))
            return;

        _body.GibBody(args.Target.Value, true);
        ent.Comp.Used = true;

        RemComp<StrapComponent>(ent);
        RemComp<DestructibleComponent>(ent);

        var query = EntityQueryEnumerator<GameRuleComponent, CultYoggRuleComponent>();

        while (query.MoveNext(out _, out var cultRule))
        {
            cultRule.AmountOfSacrifices++;

            if (cultRule.AmountOfSacrifices == cultRule.ReqAmountOfSacrifices)
                SpawnAtPosition("Nyarlathotep", Transform(ent).Coordinates);
        }

        UpdateAppearance(ent, ent.Comp, appearanceComp);
    }
}
