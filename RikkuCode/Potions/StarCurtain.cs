using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rikku.RikkuCode.Potions;

public class StarCurtain : RikkuPotion
{
    public override PotionRarity Rarity =>
        PotionRarity.Uncommon;

    public override PotionUsage Usage =>
        PotionUsage.CombatOnly;

    public override TargetType TargetType =>
        TargetType.AnyPlayer;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(
            8m,
            ValueProp.Unpowered)
    ];

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(
            StaticHoverTip.Block)
    ];

    protected override async Task OnUse(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(
            target);

        await CreatureCmd.GainBlock(
            target,
            DynamicVars.Block,
            null);

        await PowerCmd.Apply<BlockNextTurnPower>(
            choiceContext,
            target,
            DynamicVars.Block.IntValue,
            Owner.Creature,
            null);
    }
}