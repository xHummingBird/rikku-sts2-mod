using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Mechanics;

namespace Rikku.RikkuCode.Cards.Common;

public class Grenade() : RikkuCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        PotionModel? discardedPotion = null;

        if (Owner.Potions.Any())
        {
            discardedPotion =
                await SynchronizedPotionChoice.SelectAndDiscard(
                    true,
                    this,
                    choiceContext,
                    RikkuPotionMechanic.SelectDiscardPotionLocally);
        }

        int hit = discardedPotion != null ? 3 : 1;
        
        if (ownerCreature != null && Owner?.Character is Character.Rikku rikku)
        {
            AudioHelper.PlayRandomFire();
            rikku.PlayAnimation(ownerCreature, "throw_mix");
            await Task.Delay(467);
            SfxCmd.Play("res://Rikku/sfx/from_tifa/punch_swing_3.wav", 0.8f);
            await Task.Delay(33);
            SfxCmd.Play("res://Rikku/sfx/from_killia/fire_impact (2).wav");
            foreach (var enemy in CombatState.HittableEnemies)
                rikku.PlayVfxOnTarget(enemy, "res://Rikku/scenes/vfx.tscn", "grenade_small");
        }
        await CommonActions.CardAttack(this, play.Target, hitCount: hit)
            .BeforeDamage(async delegate
            {
                var targets = base.CombatState.HittableEnemies;
                foreach (var target in targets)
                {
                    var vfx = NGroundFireVfx.Create(target);
                    if (vfx != null)
                    {
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                    }
                }
            })
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}