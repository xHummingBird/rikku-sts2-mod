using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rikku.RikkuCode.Cards.Uncommon;

public class StickySituation() : RikkuCard(
    3,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(
            18m,
            ValueProp.Move)
    ];

    protected override bool ShouldGlowGoldInternal =>
        GetHpDiscount() > 0;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardBlock(
            this,
            play);
    }

    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal cost,
        out decimal modifiedCost)
    {
        modifiedCost =
            cost;

        if (!ReferenceEquals(
                this,
                card) ||
            cost <= 0)
        {
            return false;
        }

        int discount =
            GetHpDiscount();

        modifiedCost =
            Math.Max(
                0,
                cost - discount);

        return modifiedCost !=
               cost;
    }

    private int GetHpDiscount()
    {
        Creature? ownerCreature =
            Owner?.Creature;

        if (ownerCreature == null ||
            ownerCreature.MaxHp <= 0)
        {
            return 0;
        }

        decimal missingHp =
            ownerCreature.MaxHp -
            ownerCreature.CurrentHp;

        int completeQuartersMissing =
            (int)Math.Floor(
                missingHp *
                4m /
                ownerCreature.MaxHp);

        return Math.Clamp(
            completeQuartersMissing,
            0,
            3);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(
            6m);
    }
}