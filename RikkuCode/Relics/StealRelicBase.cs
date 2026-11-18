using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Mechanics;
using Rikku.RikkuCode.Potions;

namespace Rikku.RikkuCode.Relics;

public abstract class StealRelicBase : RikkuRelic
{
    [SavedProperty]
    public int StoredOverdrive { get; internal set; }

    [SavedProperty]
    public bool TriggerOverkill { get; internal set; }

    public virtual int MaxOverdrive => 100;

    protected virtual int OverdrivePerTurn => 5;

    protected virtual int OverdrivePerCard => 3;

    protected virtual int OverdrivePerStealCard => 6;

    public override bool ShowCounter => false;

    public override Task BeforeCombatStart()
    {
        Status = RelicStatus.Normal;
        TriggerOverkill = false;

        var room = Owner.RunState.CurrentRoom;
        
        OverdriveManager.ResetOverdrive(Owner);
        
        BattleTheme.Play(
            RunManager.Instance.NetService.NetId,
            Owner.RunState.CurrentActIndex + 1,
            room.RoomType);

        return Task.CompletedTask;
    }
    
    public override async Task AfterPotionUsed(
        PotionModel potion,
        Creature? target)
    {
        if (potion.Owner != Owner)
            return;

        GainOverdrive(5);

        if (Owner.Creature == null)
            return;

        await OverdriveManager.CheckOverdriveReady(
            Owner.Creature,
            null,
            Owner.Creature,
            null);
    }
    
    public override async Task AfterCombatVictory(
        CombatRoom room)
    {
        Creature? creature = Owner?.Creature;
    
        if (creature == null)
            return;
    
        // Only the client controlling Tidus runs local visuals/audio.
        if (!LocalContext.IsMe(creature))
            return;
        
        // Only boss combats play the fanfare and delay Tidus's rewards.
        bool isBoss =
            room.RoomType == RoomType.Boss;
        
        BattleTheme.PlayVictoryFanfare(
            RunManager.Instance.NetService.NetId,
            isBoss);
    }

    public override async Task AfterSideTurnStartLate(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!CombatManager.Instance.IsInProgress)
            return;

        if (Owner?.Creature == null)
            return;

        if (side != Owner.Creature.Side)
            return;

        if (Owner.Creature.IsDead)
            return;

        int amount = CalculateTurnStartGain();

        amount = ModifyTurnStartOverdriveGain(amount);

        GainOverdrive(amount);

        await OverdriveManager.CheckOverdriveReady(
            Owner.Creature,
            null,
            Owner.Creature,
            null);
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (!CombatManager.Instance.IsInProgress)
            return;

        CardModel? card = cardPlay.Card;

        if (card == null)
            return;

        if (card.Owner != Owner)
            return;

        int amount;

        if (CardPerformsSteal(card))
        {
            amount = CalculateStealCardGain(card);

            amount = ModifyStealCardOverdriveGain(
                card,
                amount);
        }
        else
        {
            amount = CalculateCardGain(card);

            amount = ModifyCardOverdriveGain(
                card,
                amount);
        }

        amount = ModifyTotalCardOverdriveGain(
            card,
            amount);

        GainOverdrive(amount);

        await OverdriveManager.CheckOverdriveReady(
            Owner.Creature,
            null,
            Owner.Creature,
            null);
    }

    protected virtual int CalculateTurnStartGain()
    {
        return OverdrivePerTurn;
    }

    protected virtual int CalculateCardGain(
        CardModel card)
    {
        return OverdrivePerCard;
    }

    protected virtual int CalculateStealCardGain(
        CardModel card)
    {
        return OverdrivePerStealCard;
    }

    // =====================================================================
    // OVERRIDE POINTS
    // =====================================================================

    protected virtual int ModifyTurnStartOverdriveGain(
        int amount)
    {
        return amount;
    }

    protected virtual int ModifyCardOverdriveGain(
        CardModel card,
        int amount)
    {
        return amount;
    }

    protected virtual int ModifyStealCardOverdriveGain(
        CardModel card,
        int amount)
    {
        return amount;
    }

    protected virtual int ModifyTotalCardOverdriveGain(
        CardModel card,
        int amount)
    {
        return amount;
    }

    /// <summary>
    /// Called whenever this relic's stored Overdrive changes.
    /// Godhand or other upgraded relics can override this for UI
    /// or additional effects.
    /// </summary>
    protected virtual void OnOverdriveChanged(
        int previousAmount,
        int newAmount)
    {
    }

    // =====================================================================
    // MANAGER COMMUNICATION
    // =====================================================================

    internal void NotifyOverdriveChanged(
        int previousAmount,
        int newAmount)
    {
        OnOverdriveChanged(
            previousAmount,
            newAmount);
    }

    // =====================================================================
    // HELPERS
    // =====================================================================

    protected virtual bool CardPerformsSteal(
        CardModel card)
    {
        return card.CanonicalKeywords.Contains(
            RikkuEnums.Steal);
    }

    private void GainOverdrive(
        int requestedAmount)
    {
        requestedAmount = Math.Max(
            0,
            requestedAmount);

        if (requestedAmount <= 0)
            return;

        OverdriveManager.GainOverdrive(
            Owner,
            requestedAmount);
    }
    
    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != Owner.Creature)
            return;

        if (!target.Powers.All(
                power => power.ShouldOwnerDeathTriggerFatal()))
        {
            return;
        }

        int overkillThreshold =
            (int)Math.Ceiling(target.MaxHp * 0.20m);
        
        
        decimal hpDamage =
            Math.Max(amount - target.Block, 0);

        bool lethal =
            hpDamage >= target.CurrentHp;
        

        bool meetsOverkillThreshold =
            amount >= overkillThreshold;

        bool overkill =
            lethal && meetsOverkillThreshold;

        if (!overkill)
            return;

        TriggerOverkill = true;

        if (dealer?.Player?.Character is Character.Rikku rikku)
        {
           rikku.PlayVfxOnTarget(
                target,
                "res://Rikku/scenes/vfx.tscn",
                "overkill"
            );
        }
    }
    
    public override async Task BeforePotionUsed(
        PotionModel potion,
        Creature? target)
    {
        if (potion.Owner != Owner)
            return;

        Creature? creature =
            Owner.Creature;

        if (creature == null)
            return;

        if (Owner.Character is not Character.Rikku rikku)
            return;

        switch (potion)
        {
            case FireGem:
            case BombFragment:
                AudioHelper.PlayRandomFire();
                break;

            case AntarcticWind:
            case IceGem:
                AudioHelper.PlayRandomIce();
                break;

            case LightningGem:
            case ElectroMarble:
                AudioHelper.PlayRandomThunder();
                break;

            case FishScale:
            case WaterGem:
                AudioHelper.PlayRandomWater();
                break;

            default:
                AudioHelper.PlayRandomMix();
                break;
        }

        rikku.PlayAnimation(
            creature,
            "throw_item");

        SfxCmd.Play(
            "res://Rikku/sfx/from_tifa/punch_swing_3.wav",
            0.8f);

        await Task.CompletedTask;
    }
    
    public override bool TryModifyRewards(
        Player player,
        List<Reward> rewards,
        AbstractRoom? room)
    {
        if (player != Owner)
            return false;

        if (room == null)
            return false;

        if (!room.RoomType.IsCombatRoom())
            return false;

        if (!TriggerOverkill)
            return false;

        rewards.Add(
            new GoldReward(
                10,
                player
            )
        );
        
        rewards.Add(
            new PotionReward(player));
        
        if (room?.RoomType == RoomType.Boss)
            rewards.Add(new RelicReward(player));
        return true;
    }
}