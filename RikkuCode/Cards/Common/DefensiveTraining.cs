using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Mechanics;

namespace Rikku.RikkuCode.Cards.Common;

public class DefensiveTraining() : RikkuCard(
    1,
    CardType.Skill,
    CardRarity.Common,
    TargetType.Self)
{
    private const string Overdrive =
        "Overdrive";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(
            8m,
            ValueProp.Move),

        new DynamicVar(
            Overdrive,
            6m)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardBlock(
            this,
            play);

        OverdriveManager.GainOverdrive(
            Owner,
            DynamicVars[Overdrive].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(
            4m);

        DynamicVars[Overdrive].UpgradeValueBy(
            2m);
    }
}