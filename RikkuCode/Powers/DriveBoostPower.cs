using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rikku.RikkuCode.Mechanics;

namespace Rikku.RikkuCode.Powers;

public class DriveBoostPower : RikkuPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Owner.Player)
            return Task.CompletedTask;

        Flash();

        OverdriveManager.GainOverdrive(
            player,
            Amount);

        return Task.CompletedTask;
    }
}