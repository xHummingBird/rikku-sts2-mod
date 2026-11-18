using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Relics;

public class ChemistGloves : RikkuRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    public override async Task BeforeCombatStartLate()
    {
        Flash();

        await PowerCmd.Apply<StashPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            1m,
            Owner.Creature,
            null);
    }
}