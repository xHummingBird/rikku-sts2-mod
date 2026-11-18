using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Cards.Uncommon;

public class DeepPockets() : RikkuCard(
    1,
    CardType.Power,
    CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(RikkuEnums.Steal)
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<DeepPocketsPower>(
            1m)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<DeepPocketsPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["DeepPocketsPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DeepPocketsPower"]
            .UpgradeValueBy(
                1m);
    }
}