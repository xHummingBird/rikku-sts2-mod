using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Rikku.RikkuCode.Character;
using Rikku.RikkuCode.Potions;

namespace Rikku.RikkuCode.Cards.Uncommon;

public class Bribe() : RikkuCard(
    1,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.Self)
{
    private const string GoldCost =
        "GoldCost";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(
            GoldCost,
            35m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override bool IsPlayable => Owner.Gold >= DynamicVars[GoldCost].IntValue && Owner.HasOpenPotionSlots;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        Owner.Gold -=
            DynamicVars[GoldCost].IntValue;

        List<PotionModel> potionOptions =
            ModelDb.PotionPool<RikkuPotionPool>()
                .GetUnlockedPotions(
                    Owner.UnlockState)
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

        await PotionCmd.TryToProcure(
            potion,
            Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[GoldCost].UpgradeValueBy(
            -10m);
    }
}