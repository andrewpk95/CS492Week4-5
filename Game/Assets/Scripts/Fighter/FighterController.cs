using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class FighterController : NetworkBehaviour, Fighter {

	public const float GRAVITY = 0.3f;

	//Basic information
	Rigidbody2D rb;
	public Player player;
	public GameLogic logic;
	public Animator anim;
	public SpriteRenderer sprite;

	//Status
	public bool isFacingRight;
	public bool isDead;
	public bool isWalking;
	public bool isDashing;
	public bool isHitStunned;
	public bool isBusy;
	public float busyDuration;
	public bool isJumping;
	public bool isDoubleJumping;
	public bool isGrounded;
	public bool isFalling;
	public bool isSpiked;

	//Attributes
	public string playerName;
	public float walkSpeed = 1f;
	public float dashSpeed = 2f;
	public float acceleration = 0.5f;
	public float deceleration = 0.5f;
	public float airSpeed = 1f;
	public float fallSpeed = 2f;
	public float jumpImpulse = 10f;
	public float doubleJumpImpulse = 10f;
	public int weight = 1;

	//Current Attributes
	public BoxCollider2D hurtBox;
	public Vector2 velocity;

	//Variables for detecting ground collision
	Rect rectBox;
	int numVerticalRays = 3;
	float margin = 0.02f;
	public bool isConnected;
	public LayerMask layerMask;

	// Use this for initialization
	void Start () {
		//Initialize
		rb = GetComponent<Rigidbody2D> ();
		GameObject g_logic = GameObject.FindGameObjectWithTag ("Logic");
		logic = g_logic.GetComponent<GameLogic> ();
		anim = GetComponent<Animator> ();
		sprite = GetComponent<SpriteRenderer> ();

		//Add to the PlayerContainer Object
		player = PlayerContainer.Add (this);
		setName (player.ToString ());
		Debug.Log (PlayerContainer.Get(player).getName() + " joined the game");

		//Set up Current Attributes
		velocity = Vector2.zero;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!isLocalPlayer)
			return;
		rectBox = new Rect (hurtBox.bounds.min.x, hurtBox.bounds.min.y, hurtBox.bounds.size.x, hurtBox.bounds.size.y);
		busyDuration -= Time.fixedDeltaTime;
		if (busyDuration <= 0) {
			busyDuration = 0;
			isBusy = false;
		}
		UpdateControl ();
		UpdatePhysics ();
		rb.velocity = velocity;
		if (isFacingRight) {
			sprite.flipX = false;
		} else {
			sprite.flipX = true;
		}
	}

	void UpdateControl() {
		if (!isBusy) {
			//Update Movement by Joystick
			float newVelocityX = velocity.x;
			if (isGrounded) { //If it is on ground...
				isWalking = GameInputManager.isWalk;
				isDashing = GameInputManager.isDash;
				if (GameInputManager.isWalk) {
					anim.SetTrigger ("Walk");
					newVelocityX += acceleration * GameInputManager.JoystickPosition.x;
					float clampWalkSpeed = walkSpeed * Mathf.Abs (GameInputManager.JoystickPosition.x);
					newVelocityX = Mathf.Clamp (newVelocityX, -clampWalkSpeed, clampWalkSpeed);
					anim.SetFloat("WalkSpeed", Mathf.Abs (GameInputManager.JoystickPosition.x));
				} else if (GameInputManager.isDash) {
					anim.SetTrigger ("Dash");
					int modifier = GameInputManager.JoystickPosition.x > 0 ? 1 : -1;
					if ((modifier > 0 && velocity.x < 0) || (modifier < 0 && velocity.x > 0)) {
						newVelocityX += deceleration * modifier;
					} else {
						newVelocityX = dashSpeed * modifier;
					}
				} else {
					anim.SetTrigger ("Idle");
					//No input deceleration
					int modifier = velocity.x > 0 ? -1 : 1;
					if (Mathf.Abs (newVelocityX) > deceleration) {
						newVelocityX += deceleration * modifier;
					} else {
						newVelocityX = 0f;
					}
				}
				if (velocity.x > 0) {
					isFacingRight = true;
				}
				if (velocity.x < 0) {
					isFacingRight = false;
				}
			} else { //If it is in air...
				if (GameInputManager.isWalk || GameInputManager.isDash) {
					newVelocityX += acceleration * GameInputManager.JoystickPosition.x;
					float clampAirSpeed = airSpeed * Mathf.Abs (GameInputManager.JoystickPosition.x);
					newVelocityX = Mathf.Clamp (newVelocityX, -clampAirSpeed, clampAirSpeed);
				} else {
					//No input deceleration
					int modifier = velocity.x > 0 ? -1 : 1;
					if (Mathf.Abs (newVelocityX) > deceleration) {
						newVelocityX += deceleration * modifier;
					} else {
						newVelocityX = 0f;
					}
				}
			}
			velocity.x = newVelocityX;

			//Update Attack input
			if (GameInputManager.isAttackButtonPressed) {

			}

			//Update Jumping
			if (GameInputManager.inputType == InputType.JumpTap) {
				Debug.Log ("JumpTap pressed");
				GameInputManager.inputType = InputType.None;
				isBusy = true;
				busyDuration = 0.1f;
				if (isGrounded) { //Single Jump
					anim.SetTrigger ("Jump");
					isGrounded = false;
					isJumping = true;
					velocity = new Vector2 (velocity.x, jumpImpulse);
				} else if (!isDoubleJumping) { //Double Jump
					anim.SetTrigger ("DoubleJump");
					velocity = new Vector2 (velocity.x, doubleJumpImpulse);
					isDoubleJumping = true;
				}
			} else if (GameInputManager.inputType == InputType.JumpHold) {
				Debug.Log ("JumpHold pressed");
				GameInputManager.inputType = InputType.None;
				isBusy = true;
				busyDuration = 0.1f;
				if (isGrounded) { //Single Jump
					anim.SetTrigger ("Jump");
					isGrounded = false;
					isJumping = true;
					velocity = new Vector2 (velocity.x, jumpImpulse);
				} else if (!isDoubleJumping) { //Double Jump
					anim.SetTrigger ("DoubleJump");
					velocity = new Vector2 (velocity.x, doubleJumpImpulse);
					isDoubleJumping = true;
				}
			}
		}
	}

	void UpdatePhysics() {
		//Apply Horizontal Movement
		if (isGrounded) { //On Ground

		} else {
			
		}

		//Apply Gravity
		if (!isGrounded) {
			if (isSpiked) {
				velocity = new Vector2 (velocity.x, velocity.y);
			} else {
				velocity = new Vector2 (velocity.x, Mathf.Max (velocity.y - GRAVITY, -fallSpeed));
			}
		}
		//Checking isFalling and isGrounded
		if (velocity.y < 0) {
			isFalling = true;
		}
		if (isGrounded || isFalling) {
			Vector2 startPoint = new Vector2 (rectBox.xMin + margin, rectBox.center.y);
			Vector2 endPoint = new Vector2 (rectBox.xMax - margin, rectBox.center.y);
			Debug.DrawLine (new Vector3 (startPoint.x, startPoint.y, 0), new Vector3 (endPoint.x, endPoint.y, 0), Color.black);
			Debug.DrawLine (new Vector3 (rectBox.min.x, rectBox.min.y, 0), new Vector3 (rectBox.max.x, rectBox.min.y, 0), Color.black);
			RaycastHit2D hitInfo;
			float rayDistance = rectBox.height / 2 + (isGrounded ? 0.1f : Mathf.Abs (velocity.y * Time.deltaTime));
			isConnected = false;

			for (int i = 0; i < numVerticalRays; i++) {
				float lerpAmount = (float)i / (float) (numVerticalRays - 1);
				Vector2 origin = Vector2.Lerp (startPoint, endPoint, lerpAmount);
				Debug.DrawLine (new Vector3 (origin.x, origin.y, 0), new Vector3 (origin.x, origin.y - rayDistance, 0), Color.red);
				hitInfo = Physics2D.Raycast (origin, Vector2.down, rayDistance, layerMask);
				isConnected = hitInfo.collider != null;
				if (isConnected) {
					if (!isGrounded) {
						anim.SetTrigger ("Land");
						isBusy = true;
						busyDuration = 0.2f;
					}
					isGrounded = true;
					isFalling = false;
					isJumping = false;
					isDoubleJumping = false;
					transform.Translate (Vector3.down * (hitInfo.distance - rectBox.height / 2));
					velocity = new Vector2 (velocity.x, 0);
					break;
				}
			}
			if (!isConnected) {
				isGrounded = false;
			}
		}
	}

	void OnTriggerEnter2D (Collider2D col) {
		if (col.gameObject.tag == "BlastZone") {
			Debug.Log (playerName + " entered the battlefield");
			isDead = false;
		}
		if (col.gameObject.tag == "Fighter") {
			Debug.Log ("Hit " + col.gameObject.name + "!");
			col.gameObject.GetComponent<FighterController> ().isHitStunned = true;
		}
	}

	void OnTriggerExit2D (Collider2D col) {
		if (col.gameObject.name == "BlastZone") {
			Debug.Log (playerName + " left the zone!");
			isDead = true;
			logic.OnPlayerDeath (this.gameObject);
		}
	}

	void OnApplicationQuit() {
		PlayerContainer.Remove (player);
		Debug.Log (player.ToString () + " left the game.");
	}

	public string getName() {
		return playerName;
	}

	public void setName(string name) {
		this.gameObject.name = name;
		playerName = name;
	}
}
