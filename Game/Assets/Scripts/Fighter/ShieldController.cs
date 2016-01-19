using UnityEngine;
using System.Collections;

public class ShieldController : MonoBehaviour {

	//Attributes
	public float ShieldHealth = 30f;
	public float health;
	public float degenRate = 5f;
	public float regenRate = 5f;

	//Components
	Fighter fighter;
	public GameObject shield;

	// Use this for initialization
	void Start () {
		health = ShieldHealth;
		fighter = GetComponentInParent<Fighter> ();
	}
	
	// Update is called once per frame
	void Update () {
		//Update Shield Size
		float scale = Mathf.Sqrt(health / ShieldHealth);
		shield.transform.localScale = new Vector3 (1.5f * scale, 1.5f * scale, 1f);
	}

	void FixedUpdate () {
		//Decrease Shield Health over time
		if (shield.activeSelf) {
			TakeDamage (degenRate * Time.fixedDeltaTime);
		} else {
			Heal (regenRate * Time.fixedDeltaTime);
		}
	}

	public void Reset() {
		health = ShieldHealth;
	}

	public void TakeDamage(float damage) {
		health -= damage;
		if (health <= 0) {
			fighter.ShieldStun (5f);
			Reset ();
		}
		Clamp ();
	}

	public void HitStun(float duration) {
		fighter.HitStun (duration);
	}

	public void Knockback(Vector2 direction, float strength) {
		fighter.Launch (direction, strength);
	}

	public void Heal(float heal) {
		health += heal;
		Clamp ();
	}

	void Clamp() {
		health = Mathf.Clamp (health, 0f, ShieldHealth);
	}

	public float GetPercentage() {
		return health;
	}
}
