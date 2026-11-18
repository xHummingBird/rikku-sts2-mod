using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Rikku.RikkuCode.Character;
using Rikku.RikkuCode.Potions;

namespace Rikku.RikkuCode.Powers;

public class StockpilePower : RikkuPower
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

        if (cardPlay.Card.Type != CardType.Power)
            return;

        List<PotionModel> potionOptions =
            ModelDb.PotionPool<RikkuPotionPool>()
                .GetUnlockedPotions(
                    Owner.Player.UnlockState)
                .Where(potion =>
                    potion.CanBeGeneratedInCombat)
                .ToList();

        if (potionOptions.Count == 0)
            return;

        Flash();

        for (int i = 0;
             i < Amount;
             i++)
        {
            if (!Owner.Player.HasOpenPotionSlots)
                break;

            PotionModel potion =
                Owner.Player.RunState
                    .Rng
                    .CombatPotionGeneration
                    .NextItem(
                        potionOptions)
                    .ToMutable();

            PotionProcureResult result =
                await PotionCmd.TryToProcure(
                    potion,
                    Owner.Player);

            if (!result.success)
                break;
        }
    }
}