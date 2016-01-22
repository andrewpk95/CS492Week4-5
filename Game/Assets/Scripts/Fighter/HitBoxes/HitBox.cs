using UnityEngine;
using System.Collections;

public class HitBox : MonoBehaviour {

	public GameObject HitEffect;
	public GameObject Owner;

	public int hitBoxFrame;
	public int frameLeft;

	public float damage;
	public float hitStunDuration;
	public float baseKnockback;
	public float knockbackGrowth;

	public float shieldDamage;
	public float shieldHitStunDuration;
	public float shieldKnockback;

	void Start() {
		frameLeft = hitBoxFrame;
	}

	void FixedUpdate() {
		frameLeft--;
		if (frameLeft <= 0) {
			Destroy (this.gameObject);
		}
	}


	void OnTriggerEnter2D(Collider2D col)
	{
		//if (!isServer)
		//return;
		if (GameObject.ReferenceEquals (col.gameObject, Owner)) {
			return;
		}
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
			GameObject hit = (GameObject)Instantiate (HitEffect, transform.position, Quaternion.identity);
			Destroy (hit, 1f);
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
