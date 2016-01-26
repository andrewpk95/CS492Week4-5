using UnityEngine;
using System.Collections;

public class MarioUpAir : HitBox {

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
		if (Vector2.Angle(Vector2.up, launchDirection) >= 45) {
			if (launchDirection.x >= 0)
				launchDirection = new Vector2 (1, 1);
			else
				launchDirection = new Vector2 (-1, 1);
		}
		target.Launch (launchDirection, baseKnockback + knockbackGrowth * targetPercentage);
	}
}
