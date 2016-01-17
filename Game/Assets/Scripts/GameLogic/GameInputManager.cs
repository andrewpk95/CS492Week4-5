using UnityEngine;
using System.Collections;

public class GameInputManager : MonoBehaviour {

	//Input UI
	public GameObject g_Joystick;
	public GameObject g_AttackButton;
	public GameObject g_GuardButton;
	public GameObject g_JumpButton;

	UIJoystick Joystick;
	UIJoystick AttackButton;
	UIJoystick GuardButton;
	UIJoystick JumpButton;

	//Variables to process input
	public bool joystickInput;
	public float joystickTapTime;
	public float joystickHoldTime;
	public Vector2 joystickInitialPosition;

	public bool attackInput;
	public float attackTapTime;
	public float attackHoldTime;
	public Vector2 attackInitialPosition;
	public Vector2 attackFinalPosition;

	public bool guardInput;
	public float guardTapTime;
	public float guardHoldTime;
	public Vector2 guardInitialPosition;

	public bool jumpInput;
	public float jumpTapTime;
	public float jumpHoldTime;

	//Final Output for other classes to use
	public static Vector2 JoystickPosition;
	public static bool isJoystickButtonPressed;
	public static bool isAttackButtonPressed;
	public static bool isGuardButtonPressed;
	public static bool isJumpButtonPressed;
	public static bool isWalk;
	public static bool isDash;
	public static InputType inputType;
	public static Vector2 inputDirection;

	// Use this for initialization
	void Start () {
		Joystick = g_Joystick.GetComponent<UIJoystick> ();
		AttackButton = g_AttackButton.GetComponent<UIJoystick> ();
		GuardButton = g_GuardButton.GetComponent<UIJoystick> ();
		JumpButton = g_JumpButton.GetComponent<UIJoystick> ();
	}
	
	// Update is called once per frame
	void Update () {
		JoystickPosition = Joystick.position;
		isJoystickButtonPressed = Joystick.isPressed;
		isAttackButtonPressed = AttackButton.isPressed;
		isGuardButtonPressed = GuardButton.isPressed;
		isJumpButtonPressed = JumpButton.isPressed;
		JoystickProcess ();
		AttackButtonProcess ();
		GuardButtonProcess ();
		JumpButtonProcess ();
	}

	void JoystickProcess() {
		if (isJoystickButtonPressed && !joystickInput) {
			joystickInput = true;
			joystickInitialPosition = Joystick.position;
		}
		if (!isJoystickButtonPressed) {
			joystickInput = false;	
			isWalk = false;
			isDash = false;
			joystickHoldTime = 0f;
		}
		if (JoystickPosition.magnitude > 0.9) {
			isWalk = false;
			isDash = true;
		} else if (JoystickPosition.magnitude > 0.2) {
			isWalk = true;
			isDash = false;
		}
		if (joystickInput) {
			joystickHoldTime += Time.deltaTime;
		}
	}

	void AttackButtonProcess() {
		if (isAttackButtonPressed && !attackInput) {
			attackInput = true;
			attackInitialPosition = AttackButton.position;
		}
		if ((!isAttackButtonPressed && attackInput) || attackHoldTime > attackTapTime) {
			attackInput = false;
			Vector2 delta;
			float angle; 
			if (attackFinalPosition.magnitude > 0.9f) {
				delta = attackFinalPosition;
				angle = Vector2.Angle (Vector2.up, attackFinalPosition);
			} else {
				delta = attackFinalPosition - attackInitialPosition;
				angle = Vector2.Angle (Vector2.up, delta);
			}
			if (angle < 45) { //Up attack
				inputDirection = Vector2.up;
			} else if (angle < 135) { //Side attack
				if (delta.x > 0) {
					inputDirection = Vector2.right;
				} else {
					inputDirection = Vector2.left;
				}
			} else { //Down attack
				inputDirection = Vector2.down;
			}
			if (delta.magnitude > 0.7f) {
				inputType = InputType.AttackStrong;
			} else if (delta.magnitude > 0.3f) {
				inputType = InputType.AttackWeak;
			} else {
				inputType = InputType.AttackTap;
			}
			attackHoldTime = 0f;
		}
		if (attackInput) {
			attackHoldTime += Time.deltaTime;
			attackFinalPosition = AttackButton.position;
		}
	}

	void GuardButtonProcess() {

	}

	void JumpButtonProcess() {
		if (isJumpButtonPressed && !jumpInput) {
			jumpInput = true;
		}
		if (!isJumpButtonPressed && jumpInput || jumpHoldTime > jumpTapTime) {
			jumpInput = false;
			if (jumpHoldTime > jumpTapTime) {
				inputType = InputType.JumpHold;
			} else {
				inputType = InputType.JumpTap;
			}
			jumpHoldTime = 0f;
		}
		if (jumpInput) {
			jumpHoldTime += Time.deltaTime;
		}
	}
}
