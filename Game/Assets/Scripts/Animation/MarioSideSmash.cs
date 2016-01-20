using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MarioSideSmash : NetworkBehaviour {

	public int hitBoxFrame = 5;
	public int frameLeft;

	public float damage = 15f;
	public float hitStunDuration = 0.5f;
	public float baseKnockback = 1f;
	public float knockbackGrowth = 0.1f;

	public float shieldDamage = 10f;
	public float shieldHitStunDuration = 0.1f;
	public float shieldKnockback = 1f;

	/*
	void OnEnable() {
		frameLeft = hitBoxFrame;
	}

	void Start() {
		
	}

	void FixedUpdate() {
		frameLeft--;
		if (frameLeft <= 0) {
			this.gameObject.SetActive (false);
		}
	}
	*/

	void OnTriggerEnter2D(Collider2D col)
	{
		//if (!isServer)
			//return;
		if (col.gameObject.tag == "Shield") {
			Fighter player = GetComponentInParent<Fighter> ();
			ShieldController target = col.gameObject.GetComponentInParent<ShieldController> ();
			Debug.Log ("Hit " + col.gameObject.GetComponentInParent<Fighter> ().getName () + "'s shield!");
			target.TakeDamage (shieldDamage);
			target.HitStun (shieldHitStunDuration);
			if (player.facingRight ()) {
				target.Knockback (new Vector2 (1, 0), shieldKnockback);
			} else {
				target.Knockback (new Vector2 (-1, 0), shieldKnockback);
			}
			return;
		}
		if (col.gameObject.tag == "HurtBox") {
			Fighter player = GetComponentInParent<Fighter> ();
			HitController target = col.gameObject.GetComponent<HitController> ();
			Debug.Log ("Hit " + col.gameObject.GetComponentInParent<Fighter> ().getName () + "!");
			float percentage = target.GetPercentage ();
			target.TakeDamage (damage);
			target.HitStun (hitStunDuration);
			if (player.facingRight ()) {
				target.Launch (new Vector2 (1, 1), baseKnockback + knockbackGrowth * percentage);
			} else {
				target.Launch (new Vector2 (-1, 1), baseKnockback + knockbackGrowth * percentage);
			}
		}
	}
}
