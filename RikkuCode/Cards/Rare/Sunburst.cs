using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Mechanics;

namespace Rikku.RikkuCode.Cards.Rare;

public class Sunburst() : RikkuCard(
    1,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            10m,
            ValueProp.Move),

        new PowerVar<VulnerablePower>(
            2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RikkuStaticHoverTips.DiscardPotion,
        HoverTipFactory.Static(
            StaticHoverTip.Block),
        HoverTipFactory.FromPower<ArtifactPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        PotionModel? discardedPotion =
            null;

        if (Owner.Potions.Any())
        {
            discardedPotion =
                await SynchronizedPotionChoice.SelectAndDiscard(
                    true,
                    this,
                    choiceContext,
                    RikkuPotionMechanic.SelectDiscardPotionLocally);
        }

        List<Creature> targets =
            CombatState.HittableEnemies
                .ToList();

        if (targets.Count == 0)
            return;

        if (discardedPotion != null)
        {
            foreach (Creature target in targets)
            {
                await CreatureCmd.LoseBlock(
                    choiceContext,
                    target,
                    target.Block,
                    Owner.Creature);

                if (target.HasPower<ArtifactPower>())
                {
                    await PowerCmd.Remove<ArtifactPower>(
                        target);
                }

                await PowerCmd.Apply<VulnerablePower>(
                    choiceContext,
                    target,
                    DynamicVars.Vulnerable.BaseValue,
                    Owner.Creature,
                    this);
            }
        }

        var ownerCreature =
            Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Rikku rikku)
        {
            Creature animationTarget =
                targets[
                    (targets.Count - 1) / 2];

            AudioHelper.PlayRandomFire();

            rikku.PlayAnimation(
                ownerCreature,
                "throw_mix");

            await Task.Delay(
                467);

            SfxCmd.Play(
                "res://Rikku/sfx/from_tifa/punch_swing_3.wav",
                0.8f);

            await Task.Delay(
                33);

            SfxCmd.Play(
                "res://Rikku/sfx/from_killia/fire_impact (1).wav");

            rikku.PlayVfxOnTarget(
                animationTarget,
                "res://Rikku/scenes/vfx.tscn",
                "grenade");
        }

        await CommonActions.CardAttack(this, play.Target)
            .BeforeDamage(async delegate
            {
                var targets = base.CombatState.HittableEnemies;
                foreach (var target in targets)
                {
                    var vfx = NGroundFireVfx.Create(target, VfxColor.Purple);
                    if (vfx != null)
                    {
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                    }
                }
            })
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(
            3m);

        DynamicVars.Vulnerable.UpgradeValueBy(
            1m);
    }
}