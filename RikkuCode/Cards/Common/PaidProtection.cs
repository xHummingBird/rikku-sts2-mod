using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rikku.RikkuCode.Cards.Common;

public class PaidProtection() : RikkuCard(1, CardType.Skill,
    CardRarity.Common, TargetType.Self)
{
    private const string GoldCost =
        "GoldCost";
    
    protected override bool IsPlayable => Owner.Gold >= DynamicVars[GoldCost].IntValue;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(13, ValueProp.Move),
        new DynamicVar(GoldCost, 15m)
    
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        Owner.Gold -= DynamicVars[GoldCost].IntValue;
        await CommonActions.CardBlock(this, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[GoldCost].UpgradeValueBy(-5m);
    }
}