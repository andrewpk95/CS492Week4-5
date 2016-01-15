using UnityEngine;
using System.Collections;

public class PhysicsController : MonoBehaviour {

	Rigidbody2D rb;
	FighterController fighter;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		fighter = GetComponent<FighterController> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		rb.velocity = fighter.velocity;
	}
}
