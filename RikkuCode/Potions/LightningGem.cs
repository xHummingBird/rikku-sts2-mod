using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Powers;
using Rikku.RikkuCode.Relics;

namespace Rikku.RikkuCode.Potions;


public class LightningGem : RikkuPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.AllEnemies;

    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(4m, ValueProp.Unpowered),
        new HitCountVar(4)
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

        var rikku =
            Owner.Character as Character.Rikku;

        for (int i = 0;
             i < DynamicVars.HitCount.IntValue;
             i++)
        {
            List<Creature> enemies =
                Owner.Creature.CombatState?
                    .HittableEnemies
                    .ToList()
                ?? [];

            if (enemies.Count == 0)
                break;

            Creature randomTarget =
                Owner.RunState
                    .Rng
                    .CombatCardSelection
                    .NextItem(
                        enemies);

            rikku?.PlayVfxOnTarget(
                randomTarget,
                "res://Rikku/scenes/vfx.tscn",
                "lightning_gem");

            SfxCmd.Play(
                "event:/sfx/characters/defect/defect_lightning_passive");

            await CreatureCmd.Damage(
                choiceContext,
                randomTarget,
                damageAmount,
                damage.Props,
                Owner.Creature,
                null,
                null);
        }
    }
}