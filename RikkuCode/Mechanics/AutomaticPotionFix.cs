using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace Rikku.RikkuCode.Mechanics
{
    internal static class AutomaticPotionFix
    {
        internal static void EnqueueForOwner(PotionModel potion, Creature target)
        {
            if (LocalContext.IsMe(potion.Owner) && RunManager.Instance.NetService.Type != NetGameType.Replay)
                potion.EnqueueManualUse(target);
        }
    }
}