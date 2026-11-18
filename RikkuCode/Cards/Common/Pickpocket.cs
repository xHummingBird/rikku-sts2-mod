using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Mechanics;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Cards.Common;

public class Pickpocket() : RikkuCard(
    1,
    CardType.Skill,
    CardRarity.Common,
    TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        RikkuEnums.Steal
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        List<Creature> targets =
            CombatState.HittableEnemies
                .ToList();

        bool stoleAnyGold =
            false;

        foreach (Creature target in targets)
        {
            int amountToSteal =
                StealHelper.GetStealAmount(
                    Owner.Creature,
                    target);

            if (amountToSteal <= 0)
                continue;

            Owner.Gold +=
                amountToSteal;

            stoleAnyGold =
                true;

            await PowerCmd.Apply<StolenGoldPower>(
                choiceContext,
                target,
                amountToSteal,
                Owner.Creature,
                this,
                false);
        }

        if (stoleAnyGold)
        {
            AudioHelper.PlayRandomSteal();
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(
            -1);
    }
}