using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Rikku.RikkuCode.Character;
using Rikku.RikkuCode.Mechanics;
using Rikku.RikkuCode.Potions;

namespace Rikku.RikkuCode.Cards.Uncommon;

public class Use() : RikkuCard(
    1,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool IsPlayable =>
        Owner.HasOpenPotionSlots;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        List<PotionModel> potionOptions =
            ModelDb.PotionPool<RikkuPotionPool>()
                .GetUnlockedPotions(
                    Owner.UnlockState)
                .Where(potion =>
                    potion.CanBeGeneratedInCombat)
                .ToList();

        if (potionOptions.Count == 0)
            return;

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
            return;

        Creature? target =
            GetPotionTarget(
                potion);

        if (potion.TargetType == TargetType.AnyEnemy &&
            target == null)
        {
            return;
        }

        AutomaticPotionFix.EnqueueForOwner(potion, target);
    }

    private Creature? GetPotionTarget(
        PotionModel potion)
    {
        if (potion.TargetType == TargetType.AnyPlayer)
        {
            return Owner.Creature;
        }

        if (potion.TargetType == TargetType.AnyEnemy)
        {
            List<Creature> enemies =
                CombatState.HittableEnemies
                    .Where(potion.IsValidTarget)
                    .ToList();

            if (enemies.Count == 0)
                return null;

            return Owner.RunState
                .Rng
                .CombatCardSelection
                .NextItem(
                    enemies);
        }

        return null;
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}