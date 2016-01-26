using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HitBox : MonoBehaviour {

	public GameObject HitEffect;
	public Fighter Owner;
	public List<Fighter> TargetsHit;

	public int hitBoxFrame;
	public int frameLeft;

	public float damage;
	public float hitStunDuration;
	public float baseKnockback;
	public float knockbackGrowth;

	public float shieldDamage;
	public float shieldHitStunDuration;
	public float shieldKnockback;

	public Fighter player;
	public float targetPercentage;

	void OnDisable() {
		TargetsHit.Clear ();
	}

	void Start() {
		Owner = GetComponentInParent<Fighter> ();
		TargetsHit = new List<Fighter>();
		player = GetComponentInParent<Fighter> ();
		frameLeft = hitBoxFrame;
	}
	/*
	void FixedUpdate() {
		frameLeft--;
		if (frameLeft <= 0) {
			//Destroy (this.gameObject);
			this.gameObject.SetActive(false);
		}
	}
	*/


	void OnTriggerEnter2D(Collider2D col)
	{
		//if (!isServer)
		//return;
		if (Owner != null) {
			if (ReferenceEquals(col.gameObject.GetComponentInParent<Fighter>(), Owner)) {
				return;
			}
		}
		if (TargetsHit.Contains (col.gameObject.GetComponentInParent<Fighter> ())) {
			return;
		}
		TargetsHit.Add (col.gameObject.GetComponentInParent<Fighter> ());

		if (col.gameObject.tag == "Shield") {
			ShieldController target = col.gameObject.GetComponentInParent<ShieldController> ();
			Debug.Log ("Hit " + col.gameObject.GetComponentInParent<Fighter> ().getName () + "'s shield!");
			target.TakeDamage (shieldDamage);
			target.HitStun (shieldHitStunDuration);
			OnShieldKnockBack (target);
			return;
		}
		if (col.gameObject.tag == "HurtBox") {
			HitController target = col.gameObject.GetComponent<HitController> ();
			Debug.Log ("Hit " + col.gameObject.GetComponentInParent<Fighter> ().getName () + "!");
			if (HitEffect != null) {
				GameObject hit = (GameObject)Instantiate (HitEffect, transform.position, Quaternion.identity);
				Destroy (hit, 1f);
			}

			target.TakeDamage (damage);
			target.HitStun (hitStunDuration);
			OnHitKnockBack (target);
		}
	}

	protected virtual void OnShieldKnockBack(ShieldController target) {
		
	}

	protected virtual void OnHitKnockBack(HitController target) {
		targetPercentage = target.GetPercentage ();
	}
}
