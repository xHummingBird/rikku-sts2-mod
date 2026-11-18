using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Cards.Common;

public class CheapShot() : RikkuCard(
    2,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            10m,
            ValueProp.Move)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target == null)
            return;

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

            await CommonActions.CardAttack(
                    this,
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
            await CommonActions.CardAttack(
                    this,
                    play.Target)
                .WithHitFx(
                    "vfx/vfx_attack_slash",
                    "res://Rikku/sfx/rikku_hit.wav")
                .Execute(
                    choiceContext);
        }

        CardModel? stealCard =
            PileType.Draw
                .GetPile(Owner)
                .Cards
                .Where(card =>
                    card.Keywords.Contains(
                        RikkuEnums.Steal) &&
                    !card.Keywords.Contains(
                        CardKeyword.Unplayable))
                .ToList()
                .StableShuffle(
                    Owner.RunState.Rng.Shuffle)
                .FirstOrDefault();

        if (stealCard == null)
        {
            stealCard =
                PileType.Draw
                    .GetPile(Owner)
                    .Cards
                    .Where(card =>
                        card.Keywords.Contains(
                            RikkuEnums.Steal))
                    .ToList()
                    .StableShuffle(
                        Owner.RunState.Rng.Shuffle)
                    .FirstOrDefault();
        }

        if (stealCard != null)
        {
            await CardCmd.AutoPlay(
                choiceContext,
                stealCard,
                null);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(
            4m);
    }
}