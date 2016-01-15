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

	public enum InputType
	{
		None,
		AttackTap,
		AttackStrong,
		Guard,
		Roll,
		JumpTap,
		JumpSwipe
	}

	//Final Output for other classes to use
	public static Vector2 JoystickPosition;
	public static bool isAttackButtonPressed;
	public static bool isGuardButtonPressed;
	public static bool isJumpButtonPressed;
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
		isAttackButtonPressed = AttackButton.isPressed;
		isGuardButtonPressed = GuardButton.isPressed;
		isJumpButtonPressed = JumpButton.isPressed;
		AttackButtonProcess ();
		GuardButtonProcess ();
		JumpButtonProcess ();
	}

	void AttackButtonProcess() {

	}

	void GuardButtonProcess() {

	}

	void JumpButtonProcess() {

	}
}
