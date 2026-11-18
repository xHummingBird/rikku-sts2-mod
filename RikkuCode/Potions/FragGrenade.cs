using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Potions;

public class FragGrenade : RikkuPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.AnyEnemy;
    
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
                "grenade_small");
            SfxCmd.Play(
                "res://Rikku/sfx/from_killia/fire_impact (2).wav");
        }
        
        var vfx =
            NGroundFireVfx.Create(target, VfxColor.Red);

        if (vfx != null)
        {
            NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
        }

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