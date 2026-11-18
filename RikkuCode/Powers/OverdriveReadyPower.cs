using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using Rikku.RikkuCode.Cards.Ancient;

namespace Rikku.RikkuCode.Powers;

public class OverdriveReadyPower : RikkuPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    private IEnumerable<CardModel> GetMix()
    {
        var hand =
            PileType.Hand.GetPile(Owner.Player);

        return hand.Cards
            .OfType<Mix>();
    }

    public override async Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        var existingCard = Owner.Player.PlayerCombatState?
            .AllCards
            .OfType<Mix>()
            .FirstOrDefault();

        Flash();
        
        if (existingCard == null)
        {
            var card =
                CombatState.CreateCard<Mix>(
                    Owner.Player);

            await CardPileCmd.AddGeneratedCardToCombat(
                card,
                PileType.Hand,
                Owner.Player);
        }
        else if (existingCard.Pile?.Type != PileType.Hand)
        {
            await CardPileCmd.Add(
                existingCard,
                PileType.Hand);
        }
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != Owner.Side)
            return;

        var player = Owner.Player;
        var playerState = player.PlayerCombatState;

        if (playerState == null)
            return;

        if (playerState.AllCards
            .OfType<Mix>()
            .All(c => c.Pile?.Type != PileType.Hand))
        {
            var cards = playerState.AllCards
                .OfType<Mix>()
                .Where(c =>
                    c.Pile == null ||
                    c.Pile.Type != PileType.Hand);

            Flash();
            await CardPileCmd.Add(
                cards,
                PileType.Hand);
        }
    }
}
