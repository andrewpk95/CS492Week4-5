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
	public float hitStunDuration;
	public bool isBusy;
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
	public Vector2 newVelocity;

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
		newVelocity = Vector2.zero;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!isLocalPlayer)
			return;
		rectBox = new Rect (hurtBox.bounds.min.x, hurtBox.bounds.min.y, hurtBox.bounds.size.x, hurtBox.bounds.size.y);
		isBusy = anim.GetBool ("isBusy");
		UpdateControl ();
		UpdatePhysics ();
		rb.velocity = velocity;
		Flip ();
		if (hitStunDuration > 0) {
			hitStunDuration -= Time.fixedDeltaTime;
		} else {
			isHitStunned = false;
		}
	}

	void UpdateControl() {
		if (!isBusy) {
			UpdateJoystick ();
			UpdateAttack ();
			UpdateGuard ();
			UpdateJump ();
		}
	}

	void UpdateJoystick() {
		//Update Movement by Joystick
		newVelocity.x = velocity.x;
		if (isGrounded) { //If it is on ground...
			isWalking = GameInputManager.isWalk;
			isDashing = GameInputManager.isDash;
			if (GameInputManager.isWalk) {
				Walk ();
			} else if (GameInputManager.isDash) {
				Dash ();
			} else {
				anim.SetTrigger ("Idle");
				Stop ();
			}
			if (velocity.x > 0) {
				isFacingRight = true;
			}
			if (velocity.x < 0) {
				isFacingRight = false;
			}
		} else { //If it is in air...
			if (GameInputManager.isWalk || GameInputManager.isDash) {
				AirMove ();
			} else {
				//No input deceleration
				Stop ();
			}
		}
		velocity.x = newVelocity.x;
	}

	void Walk() {
		anim.SetTrigger ("Walk");
		newVelocity.x += acceleration * GameInputManager.JoystickPosition.x;
		float clampWalkSpeed = walkSpeed * Mathf.Abs (GameInputManager.JoystickPosition.x);
		newVelocity.x = Mathf.Clamp (newVelocity.x, -clampWalkSpeed, clampWalkSpeed);
		anim.SetFloat("WalkSpeed", Mathf.Abs (GameInputManager.JoystickPosition.x));
	}

	void Dash() {
		anim.SetTrigger ("Dash");
		int modifier = GameInputManager.JoystickPosition.x > 0 ? 1 : -1;
		if ((modifier > 0 && velocity.x < 0) || (modifier < 0 && velocity.x > 0)) {
			newVelocity.x += deceleration * modifier;
		} else {
			newVelocity.x = dashSpeed * modifier;
		}
	}

	void AirMove() {
		newVelocity.x += acceleration * GameInputManager.JoystickPosition.x;
		float clampAirSpeed = airSpeed * Mathf.Abs (GameInputManager.JoystickPosition.x);
		newVelocity.x = Mathf.Clamp (newVelocity.x, -clampAirSpeed, clampAirSpeed);
	}

	void Stop() {
		//No input deceleration
		int modifier = velocity.x > 0 ? -1 : 1;
		if (Mathf.Abs (newVelocity.x) > deceleration) {
			newVelocity.x += deceleration * modifier;
		} else {
			newVelocity.x = 0f;
		}
	}

	void UpdateAttack() {
		//Update Attack input
		if (isGrounded) { //Ground Moves
			if (GameInputManager.inputType == InputType.AttackStrong) { //Smash Attacks
				GameInputManager.inputType = InputType.None;
				if (GameInputManager.inputDirection == Vector2.left) { //Left Side Smash
					isFacingRight = false;
					Flip ();
					LeftSideSmash ();
				} else if (GameInputManager.inputDirection == Vector2.right) { //Right Side Smash
					isFacingRight = true;
					Flip ();
					RightSideSmash ();
				} else if (GameInputManager.inputDirection == Vector2.up) { //Up Smash
					UpSmash ();
				} else if (GameInputManager.inputDirection == Vector2.down) { //Down Smash
					DownSmash ();
				}
			} else if (GameInputManager.inputType == InputType.AttackWeak) { //Weak Attacks
				GameInputManager.inputType = InputType.None;
				if (GameInputManager.inputDirection == Vector2.left) { //Left Weak Attack
					isFacingRight = false;
					Flip ();
					LeftWeakAttack ();
				} else if (GameInputManager.inputDirection == Vector2.right) { //Right Weak Attack
					isFacingRight = true;
					Flip ();
					RightWeakAttack ();
				} else if (GameInputManager.inputDirection == Vector2.up) { //Up Weak Attack
					UpWeakAttack ();
				} else if (GameInputManager.inputDirection == Vector2.down) { //Down Weak Attack
					DownWeakAttack ();
				}
			} else if (GameInputManager.inputType == InputType.AttackTap) { //Jab Attack
				GameInputManager.inputType = InputType.None;
				Jab ();
			}
		} else { //Air Moves
			if (GameInputManager.inputType == InputType.AttackStrong 
				|| GameInputManager.inputType == InputType.AttackWeak) {
				float modifier = isFacingRight ? 1f : -1f;
				Vector2 faceDirection = new Vector2 (0f, modifier);
				if (GameInputManager.inputDirection.x != 0) { 
					if (GameInputManager.inputDirection == faceDirection) { //Forward Air
						ForwardAir ();
					} else { //Back Air
						BackAir ();
					}
				} else if (GameInputManager.inputDirection == Vector2.up) { //Up Air
					UpAir ();
				} else if (GameInputManager.inputDirection == Vector2.down) { //Down Air
					DownAir ();
				}
			} else if (GameInputManager.inputType == InputType.AttackTap) { //Neutral Air
				NeutralAir ();
			}
		}
	}

	void LeftSideSmash() {
		Debug.Log (playerName + "'s Left Side Smash!");
		anim.SetTrigger ("SideSmash");
	}

	void RightSideSmash() {
		Debug.Log (playerName + "'s Right Side Smash!");
		anim.SetTrigger ("SideSmash");
	}

	void UpSmash() {
		Debug.Log (playerName + "'s Up Smash!");
	}

	void DownSmash() {
		Debug.Log (playerName + "'s Down Smash!");
	}

	void LeftWeakAttack() {
		Debug.Log (playerName + "'s Left Weak Attack!");
	}

	void RightWeakAttack() {
		Debug.Log (playerName + "'s Right Weak Attack!");
	}

	void UpWeakAttack() {
		Debug.Log (playerName + "'s Up Weak Attack!");
	}

	void DownWeakAttack() {
		Debug.Log (playerName + "'s Down Weak Attack!");
	}

	void Jab() {
		Debug.Log (playerName + "'s Jab Attack!");
	}

	void NeutralAir() {
		Debug.Log (playerName + "'s Neutral Air!");
	}

	void ForwardAir() {
		Debug.Log (playerName + "'s Forward Air!");
	}

	void BackAir() {
		Debug.Log (playerName + "'s Back Air!");
	}

	void UpAir() {
		Debug.Log (playerName + "'s Up Air!");
	}

	void DownAir() {
		Debug.Log (playerName + "'s Down Air!");
	}

	void UpdateGuard() {
		//Update Guarding
		if (isGrounded) { //Ground Guard Moves
			if (GameInputManager.inputType == InputType.Guard) {
				GameInputManager.inputType = InputType.None;
				Guard ();
			} else if (GameInputManager.inputType == InputType.Roll) {
				GameInputManager.inputType = InputType.None;
				if (GameInputManager.inputDirection == Vector2.left) { //Left Roll
					RollLeft ();
				} else if (GameInputManager.inputDirection == Vector2.right) { //Right Roll
					RollRight ();
				} else if (GameInputManager.inputDirection == Vector2.up
				           || GameInputManager.inputDirection == Vector2.down) { //Side Dodge
					SideDodge ();
				}
			}
		} else { //Air Guard Moves
			if (GameInputManager.inputType == InputType.Guard 
				|| GameInputManager.inputType == InputType.Roll) { //Air Dodge
				GameInputManager.inputType = InputType.None;
				AirDodge ();
			}
		}
	}

	void Guard() {
		Debug.Log (playerName + "'s Guard!");
	}

	void RollLeft() {
		Debug.Log (playerName + "'s Left Roll!");
	}

	void RollRight() {
		Debug.Log (playerName + "'s Right Roll!");
	}

	void SideDodge() {
		Debug.Log (playerName + "'s Side Dodge!");
	}

	void AirDodge() {
		Debug.Log (playerName + "'s Air Dodge!");
	}

	void UpdateJump() {
		//Update Jumping
		if (GameInputManager.inputType == InputType.JumpTap) {
			GameInputManager.inputType = InputType.None;
			JumpTap ();
		} else if (GameInputManager.inputType == InputType.JumpHold) {
			GameInputManager.inputType = InputType.None;
			JumpHold ();
		}
	}

	void JumpTap() {
		if (isGrounded) { //Single Jump
			Debug.Log (playerName + "'s Jump!");
			anim.SetTrigger ("Jump");
			isGrounded = false;
			isJumping = true;
			velocity = new Vector2 (velocity.x, jumpImpulse);
		} else if (!isDoubleJumping) { //Double Jump
			Debug.Log (playerName + "'s Double Jump!");
			anim.SetTrigger ("DoubleJump");
			velocity = new Vector2 (velocity.x, doubleJumpImpulse);
			isDoubleJumping = true;
		}
	}

	void JumpHold() {
		JumpTap ();
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

	void OnApplicationQuit() {
		PlayerContainer.Remove (player);
		Debug.Log (player.ToString () + " left the game.");
	}

	void Flip() {
		if (isFacingRight) {
			transform.rotation = Quaternion.Euler (Vector3.zero);
		} else {
			transform.rotation = Quaternion.Euler (Vector3.down * 180f);
		}
	}

	public void HitStun(float duration) {
		isHitStunned = true;
		hitStunDuration = duration;
	}

	public void Launch(Vector2 direction, float strength) {
		Vector2 dir = direction.normalized;
		velocity = dir * strength;
	}

	public void Die() {
		isDead = true;
		logic.OnPlayerDeath (this.gameObject);
	}

	public void Revive() {
		isDead = false;
	}

	public string getName() {
		return playerName;
	}

	public void setName(string name) {
		this.gameObject.name = name;
		playerName = name;
	}

	public bool facingRight() {
		return isFacingRight;
	}
}
