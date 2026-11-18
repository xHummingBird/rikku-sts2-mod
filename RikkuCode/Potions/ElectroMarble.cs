using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Potions;

public class ElectroMarble : RikkuPotion
{
    public override PotionRarity Rarity =>
        PotionRarity.Common;

    public override PotionUsage Usage =>
        PotionUsage.CombatOnly;

    public override TargetType TargetType =>
        TargetType.AnyEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            8m,
            ValueProp.Unpowered)
    ];

    protected override async Task OnUse(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(
            target);

        DamageVar damage =
            DynamicVars.Damage;

        decimal damageAmount =
            GetMixablePotionDamage(
                Owner,
                damage.BaseValue);

        var rikku =
            Owner.Character as Character.Rikku;

        if (rikku != null)
        {
            rikku.PlayVfxOnTarget(
                target,
                "res://Rikku/scenes/vfx.tscn",
                "lightning_gem");
        }

        SfxCmd.Play(
            "event:/sfx/characters/defect/defect_lightning_passive");

        await CreatureCmd.Damage(
            choiceContext,
            target,
            damageAmount,
            damage.Props,
            Owner.Creature,
            null,
            null);
    }
}