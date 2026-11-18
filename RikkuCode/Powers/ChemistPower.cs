using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Rikku.RikkuCode.Powers;

public class ChemistPower : RikkuPower
{
    private bool _triggeredThisTurn;

    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player == Owner.Player)
        {
            _triggeredThisTurn =
                false;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterPotionProcured(
        PotionModel potion)
    {
        if (potion.Owner != Owner.Player)
            return;

        await TryGainEnergy();
    }

    public override async Task AfterPotionUsed(
        PotionModel potion,
        Creature? target)
    {
        if (potion.Owner != Owner.Player)
            return;

        await TryGainEnergy();
    }

    private async Task TryGainEnergy()
    {
        if (_triggeredThisTurn)
            return;

        _triggeredThisTurn =
            true;

        Flash();

        await PlayerCmd.GainEnergy(
            Amount,
            Owner.Player);
    }
}