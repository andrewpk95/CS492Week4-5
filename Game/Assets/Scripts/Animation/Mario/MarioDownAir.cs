using UnityEngine;
using System.Collections;

public class MarioDownAir : HitBox {

	protected override void OnShieldKnockBack(ShieldController target) {
		Vector2 launchDirection = target.transform.position - player.getTransform ().position;
		if (launchDirection.x >= 0) {
			target.Knockback (new Vector2 (1, 0), shieldKnockback);
		} else {
			target.Knockback (new Vector2 (-1, 0), shieldKnockback);
		}
	}

	protected override void OnHitKnockBack(HitController target) {
		base.OnHitKnockBack (target);
		if (player.facingRight ()) {
			target.Launch (new Vector2 (3, 2), baseKnockback + knockbackGrowth * targetPercentage);
		} else {
			target.Launch (new Vector2 (-3, 2), baseKnockback + knockbackGrowth * targetPercentage);
		}
	}
}
