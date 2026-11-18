using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Powers;

public class LuckyGirlPower : RikkuPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player)
            return;

        if (!cardPlay.Card.Keywords.Contains(
                RikkuEnums.Steal))
        {
            return;
        }

        Flash();

        await CardPileCmd.Draw(
            choiceContext,
            Amount,
            Owner.Player);
    }
}