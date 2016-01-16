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
