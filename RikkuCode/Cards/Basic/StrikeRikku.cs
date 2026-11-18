using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Cards.Basic;

public class StrikeRikku() : RikkuCard(1, CardType.Attack,
    CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Rikku rikku)
        {
            AudioHelper.PlayRandomAttack();
            await rikku.DashTo(ownerCreature, play.Target, distance: 400f, durationSeconds: 0.15F);
            rikku.PlayAnimation(ownerCreature, "attack");
            SfxCmd.Play("res://Rikku/sfx/from_tifa/punch_swing_1.wav");
            await Task.Delay(50);
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx("vfx/vfx_attack_slash", "res://Rikku/sfx/rikku_hit.wav")
                .Execute(choiceContext);
            await rikku.Retreat(ownerCreature, duration: 0.2F);
        }
        else  await CommonActions.CardAttack(this, play.Target)
            .WithHitFx("vfx/vfx_attack_slash", "res://Rikku/sfx/rikku_hit.wav")
            .Execute(choiceContext);
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}