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

public class IceGem : RikkuPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.AllEnemies;

    protected override IEnumerable<DynamicVar> CanonicalVars => 
        [
            new DamageVar(12m, ValueProp.Unpowered)
        ];

    protected override async Task OnUse(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        DamageVar damage =
            DynamicVars.Damage;

        decimal damageAmount =
            GetMixablePotionDamage(Owner,
                damage.BaseValue);

        IReadOnlyList<Creature> targets =
            Owner.Creature.CombatState
                .HittableEnemies
                .ToList();

        if (targets.Count == 0)
            return;

        SfxCmd.Play(
            "res://Rikku/sfx/ice.wav");

        var rikku =
            Owner.Character as Character.Rikku;

        if (rikku != null)
        {
            foreach (Creature enemy in targets)
            {
                rikku.PlayVfxOnTarget(
                    enemy,
                    "res://Rikku/scenes/vfx.tscn",
                    "ice_gem");
            }
        }

        await CreatureCmd.Damage(
            choiceContext,
            targets,
            damageAmount,
            damage.Props,
            Owner.Creature,
            null,
            null);
    }
}