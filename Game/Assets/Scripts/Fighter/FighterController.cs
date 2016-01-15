using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class FighterController : NetworkBehaviour, Fighter {

	//Basic information
	public Player player;
	public GameLogic logic;

	//Status
	public bool isDead;
	public bool hitStunned;
	public bool isHitting;

	//Attributes
	public string name;
	public float walkSpeed;
	public float dashSpeed;
	public int weight;

	//Current Attributes
	public Vector2 velocity;

	// Use this for initialization
	void Start () {
		//Initialize
		GameObject g_logic = GameObject.FindGameObjectWithTag ("Logic");
		logic = g_logic.GetComponent<GameLogic> ();

		//Add to the PlayerContainer Object
		player = PlayerContainer.Add (this);
		setName (player.ToString ());
		Debug.Log (PlayerContainer.Get(player).getName() + "Added");

		//Set up Current Attributes
		velocity = Vector2.zero;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!isLocalPlayer)
			return;
		if (!isHitting && !hitStunned && !isDead) {
			velocity.x = GameInputManager.JoystickPosition.x;
			velocity.y = GameInputManager.JoystickPosition.y;
		}

		if (GameInputManager.isAttackButtonPressed) {

		}
	}

	void OnTriggerEnter2D (Collider2D col) {
		if (col.gameObject.name == "BlastZone") {
			Debug.Log (this.name + " entered the battlefield");
			isDead = false;
		}
		if (col.gameObject.tag == "Fighter") {
			Debug.Log ("Hit " + col.gameObject.name + "!");
			col.gameObject.GetComponent<FighterController> ().hitStunned = true;
		}
	}

	void OnTriggerExit2D (Collider2D col) {
		if (col.gameObject.name == "BlastZone") {
			Debug.Log (this.name + " left the zone!");
			isDead = true;
			logic.OnPlayerDeath (this.gameObject);
		}
	}

	public string getName() {
		return name;
	}

	public void setName(string name) {
		this.gameObject.name = name;
		this.name = name;
	}
}
