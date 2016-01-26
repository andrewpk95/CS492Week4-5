using UnityEngine;
using System.Collections;

public class MarioUpSmash : HitBox {
	
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
		Vector2 launchDirection = target.transform.position - player.getTransform ().position;
		if (launchDirection.x >= 0) {
			target.Launch (new Vector2 (1, 3), baseKnockback + knockbackGrowth * targetPercentage);
		} else {
			target.Launch (new Vector2 (-1, 3), baseKnockback + knockbackGrowth * targetPercentage);
		}
	}
}
