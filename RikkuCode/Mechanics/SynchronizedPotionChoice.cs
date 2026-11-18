using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Rikku.RikkuCode.Mechanics
{
    internal static class SynchronizedPotionChoice
    {
        internal static async Task<(PotionModel, PotionModel)> SelectMix(
            bool canCancel, CardModel card, PlayerChoiceContext context,
            Func<bool, Task<(PotionModel, PotionModel)>> selectLocally)
        {
            Player player = card.Owner;
            var sync = RunManager.Instance.PlayerChoiceSynchronizer;
            uint choiceId = sync.ReserveChoiceId(player);
            await context.SignalPlayerChoiceBegun(player, default);

            try
            {
                if (ShouldChooseLocally(player))
                {
                    var (primary, secondary) = await selectLocally(canCancel);
                    int first = FindOwnedSlot(player, primary);
                    int second = FindOwnedSlot(player, secondary);
                    if (first < 0 || second < 0 || first == second)
                    {
                        sync.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndexes(new List<int>()));
                        return (null, null);
                    }

                    sync.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndexes(new List<int> { first, second }));
                    return (primary, secondary);
                }

                var slots = (await sync.WaitForRemoteChoice(player, choiceId)).AsIndexes();
                if (slots.Count != 2 || slots[0] == slots[1])
                    return (null, null);

                return (PotionAt(player, slots[0]), PotionAt(player, slots[1]));
            }
            finally
            {
                await context.SignalPlayerChoiceEnded();
            }
        }

        internal static async Task<PotionModel> SelectAndDiscard(
            bool canCancel, CardModel card, PlayerChoiceContext context,
            Func<bool, Task<PotionModel>> selectLocally)
        {
            Player player = card.Owner;
            var sync = RunManager.Instance.PlayerChoiceSynchronizer;
            uint choiceId = sync.ReserveChoiceId(player);
            await context.SignalPlayerChoiceBegun(player, default);

            PotionModel potion;
            try
            {
                if (ShouldChooseLocally(player))
                {
                    potion = await selectLocally(canCancel);
                    int slot = FindOwnedSlot(player, potion);
                    if (slot < 0)
                        potion = null;

                    sync.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndexes(slot < 0 ? new List<int>() : new List<int> { slot }));
                }
                else
                {
                    var slots = (await sync.WaitForRemoteChoice(player, choiceId)).AsIndexes();
                    potion = slots.Count == 1 ? PotionAt(player, slots[0]) : null;
                }
            }
            finally
            {
                await context.SignalPlayerChoiceEnded();
            }

            if (potion != null)
                await PotionCmd.Discard(potion);

            return potion;
        }

        private static bool ShouldChooseLocally(Player player)
        {
            return LocalContext.IsMe(player) &&
                RunManager.Instance.NetService.Type != MegaCrit.Sts2.Core.Multiplayer.Game.NetGameType.Replay;
        }

        private static int FindOwnedSlot(Player player, PotionModel potion)
        {
            if (potion == null || potion.Owner != player)
                return -1;

            for (int slot = 0; slot < player.PotionSlots.Count; slot++)
                if (ReferenceEquals(player.PotionSlots[slot], potion))
                    return slot;

            return -1;
        }

        private static PotionModel PotionAt(Player player, int slot)
        {
            if (slot < 0 || slot >= player.PotionSlots.Count)
                return null;

            return player.PotionSlots[slot];
        }
    }
}


