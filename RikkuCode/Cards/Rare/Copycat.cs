using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Rikku.RikkuCode.Cards.Rare;

public class Copycat() : RikkuCard(
    1,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        CardPile discardPile =
            PileType.Discard.GetPile(
                Owner);

        if (discardPile.Cards.Count == 0)
            return;

        IEnumerable<CardModel> selectedCards =
            await CardSelectCmd.FromCombatPile(
                choiceContext,
                discardPile,
                Owner,
                new CardSelectorPrefs(
                    SelectionScreenPrompt,
                    1));

        CardModel? selectedCard =
            selectedCards.FirstOrDefault();

        if (selectedCard == null)
            return;
        
        if (IsUpgraded)
        {
            CardCmd.Upgrade(
                selectedCard);
        }

        selectedCard.EnergyCost.SetThisTurn(
            0);

        await CardPileCmd.Add(
            selectedCards,
            PileType.Hand);
    }

    protected override void OnUpgrade()
    {
    }
}