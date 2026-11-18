using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Cards.Uncommon;

public class ArmedRobbery() : RikkuCard(
    0,
    CardType.Attack,
    CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    private const string CalculatedHits =
        "CalculatedHits";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            5m,
            ValueProp.Move),

        new CalculationBaseVar(
            0m),

        new CalculationExtraVar(
            1m),

        new CalculatedHitCountVar(
                CalculatedHits)
            .WithMultiplier((
                CardModel card,
                Creature? target) =>
            {
                return CombatManager.Instance
                    .History
                    .CardPlaysFinished
                    .Count(entry =>
                        entry.HappenedThisTurn(
                            card.CombatState) &&
                        entry.CardPlay.Player ==
                            card.Owner &&
                        entry.CardPlay.Card.Keywords.Contains(
                            RikkuEnums.Steal));
            })
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target == null)
            return;

        int hitCount =
            (int)((CalculatedHitCountVar)DynamicVars[
                CalculatedHits])
                .Calculate(
                    play.Target);

        var ownerCreature =
            Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Rikku rikku)
        {
            AudioHelper.PlayRandomAttack();

            await rikku.DashTo(
                ownerCreature,
                play.Target,
                distance: 400f,
                durationSeconds: 0.15f);

            rikku.PlayAnimation(
                ownerCreature,
                "attack");

            SfxCmd.Play(
                "res://Rikku/sfx/from_tifa/punch_swing_1.wav");

            await Task.Delay(50);

            await DamageCmd.Attack(
                    DynamicVars.Damage.BaseValue)
                .WithHitCount(
                    hitCount)
                .FromCard(
                    this,
                    play)
                .Targeting(
                    play.Target)
                .WithHitFx(
                    "vfx/vfx_attack_slash",
                    "res://Rikku/sfx/rikku_hit.wav")
                .Execute(
                    choiceContext);

            await rikku.Retreat(
                ownerCreature,
                duration: 0.2f);
        }
        else
        {
            await DamageCmd.Attack(
                    DynamicVars.Damage.BaseValue)
                .WithHitCount(
                    hitCount)
                .FromCard(
                    this,
                    play)
                .Targeting(
                    play.Target)
                .WithHitFx(
                    "vfx/vfx_attack_slash",
                    "res://Rikku/sfx/rikku_hit.wav")
                .Execute(
                    choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(
            2m);
    }
}