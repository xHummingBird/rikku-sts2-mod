using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Rikku.RikkuCode.Character;
using Rikku.RikkuCode.Mechanics;
using Rikku.RikkuCode.Potions;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Cards.Rare;

public class EquivalentExchange() : RikkuCard(
    0,
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
        await PowerCmd.Apply<EquivalentExchangePower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);

        List<PotionModel> potionOptions =
            ModelDb.PotionPool<RikkuPotionPool>()
                .GetUnlockedPotions(
                    Owner.UnlockState)
                .ToList();

        if (potionOptions.Count == 0)
            return;

        while (Owner.HasOpenPotionSlots)
        {
            PotionModel potion =
                Owner.RunState
                    .Rng
                    .CombatPotionGeneration
                    .NextItem(
                        potionOptions)
                    .ToMutable();

            PotionProcureResult result =
                await PotionCmd.TryToProcure(
                    potion,
                    Owner);

            if (!result.success)
                break;
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}