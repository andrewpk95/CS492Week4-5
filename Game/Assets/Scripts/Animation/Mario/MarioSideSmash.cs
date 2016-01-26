using UnityEngine;
using System.Collections;

public class MarioSideSmash : HitBox {

	protected override void OnShieldKnockBack(ShieldController target) {
		if (player.facingRight ()) {
			target.Knockback (new Vector2 (1, 0), shieldKnockback);
		} else {
			target.Knockback (new Vector2 (-1, 0), shieldKnockback);
		}
	}

	protected override void OnHitKnockBack(HitController target) {
		base.OnHitKnockBack (target);
		if (player.facingRight ()) {
			target.Launch (new Vector2 (1, 1), baseKnockback + knockbackGrowth * targetPercentage);
		} else {
			target.Launch (new Vector2 (-1, 1), baseKnockback + knockbackGrowth * targetPercentage);
		}
	}
}
