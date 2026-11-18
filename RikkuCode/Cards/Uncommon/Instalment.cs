using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rikku.RikkuCode.Cards.Uncommon;

public class Instalment() : RikkuCard(
    1,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.Self)
{
    private const string BlockNextTurn =
        "BlockNextTurn";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(
            5m,
            ValueProp.Move),

        new DynamicVar(
            BlockNextTurn,
            5m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(
            StaticHoverTip.Block),

        HoverTipFactory.FromPower<BlockNextTurnPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardBlock(
            this,
            play);

        await PowerCmd.Apply<BlockNextTurnPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Block.PreviewValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(
            2m);

        DynamicVars[BlockNextTurn].UpgradeValueBy(
            2m);
    }
}