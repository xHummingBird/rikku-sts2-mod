using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Cards.Rare;

public class Luck() : RikkuCard(
    0,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        int strength =
            Owner.RunState
                .Rng
                .CombatCardSelection
                .NextInt(
                    1,
                    5);
        
        await PowerCmd.Apply<LuckStrengthPower>(
            choiceContext,
            Owner.Creature,
            strength,
            Owner.Creature,
            this);
        
        int dexterity =
            Owner.RunState
                .Rng
                .CombatCardSelection
                .NextInt(
                    1,
                    5);

        await PowerCmd.Apply<LuckDexterityPower>(
            choiceContext,
            Owner.Creature,
            dexterity,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}