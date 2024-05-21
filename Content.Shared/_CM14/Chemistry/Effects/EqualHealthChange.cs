﻿using System.Text.Json.Serialization;
using Content.Shared._CM14.Damage;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Localizations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CM14.Chemistry.Effects;

public sealed partial class EqualHealthChange : ReagentEffect
{
    [DataField(required: true)]
    [JsonPropertyName("damage")]
    public List<(ProtoId<DamageGroupPrototype> Group, FixedPoint2 Amount)> Damage = new();

    [DataField]
    [JsonPropertyName("ignoreResistances")]
    public bool IgnoreResistances = true;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var damages = new List<string>();
        var heals = false;
        var deals = false;

        foreach (var (groupId, amount) in Damage)
        {
            if (!prototype.TryIndex(groupId, out var group))
                continue;

            var sign = FixedPoint2.Sign(amount);

            if (sign < 0)
                heals = true;
            if (sign > 0)
                deals = true;

            damages.Add(
                Loc.GetString("health-change-display",
                    ("kind", group.LocalizedName),
                    ("amount", MathF.Abs(amount.Float())),
                    ("deltasign", sign)
                ));
        }

        var healsordeals = heals ? (deals ? "both" : "heals") : (deals ? "deals" : "none");

        return Loc.GetString("reagent-effect-guidebook-health-change",
            ("chance", Probability),
            ("changes", ContentLocalizationManager.FormatList(damages)),
            ("healsordeals", healsordeals));
    }

    public override void Effect(ReagentEffectArgs args)
    {
        var damage = new DamageSpecifier();
        var cmDamageable = args.EntityManager.System<CMDamageableSystem>();
        foreach (var (group, amount) in Damage)
        {
            damage = cmDamageable.DistributeHealing(args.SolutionEntity, group, amount, damage);
        }

        args.EntityManager.System<DamageableSystem>()
            .TryChangeDamage(args.SolutionEntity, damage, IgnoreResistances, interruptsDoAfters: false);
    }
}
