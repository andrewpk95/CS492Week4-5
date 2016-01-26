using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerInput : NetworkBehaviour {

	public GameObject Mario;
	public GameObject Inkachu;
	public bool isHost;

	public GameInputManager inputManager;
	[SyncVar] public Player player;
	public GameObject g_fighter;
	public Fighter fighter;

	public Vector2 JoystickPosition;
	public bool isJoystickButtonPressed;
	public bool isAttackButtonPressed;
	public bool isGuardButtonPressed;
	public bool isJumpButtonPressed;
	public bool isWalk;
	public bool isDash;
	public InputType inputType;
	public Vector2 inputDirection;

	// Use this for initialization
	void Start () {
		if (!isLocalPlayer)
			return;
		inputManager = FindObjectOfType<GameInputManager> ();
		isHost = inputManager.isHost;
		//Reset ();

		CmdAdd (isHost);
		UpdateInput ();
	}

	[Command]
	void CmdAdd(bool host) {
		GameInputManager i = FindObjectOfType<GameInputManager> ();
		isHost = i.isHost;
		g_fighter = (GameObject)Instantiate (Mario);
		//g_fighter = (GameObject)Instantiate (Inkachu);
		fighter = g_fighter.GetComponent<Fighter> ();
		player = PlayerContainer.Add (fighter);
		FindObjectOfType<ResultContainer> ().Add (player);
		this.gameObject.name = player.ToString ();
		fighter.SetPlayer (player);
		fighter.setName (player.ToString ());
		fighter.SetInput (this);
		fighter.SetLocalClient (isHost);
		NetworkServer.Spawn (g_fighter);
		Debug.Log (PlayerContainer.Get(player).getName() + " joined the game");
		RpcAdd (player);
	}

	[ClientRpc]
	void RpcAdd(Player pl) {
		player = pl;
		this.gameObject.name = player.ToString ();
	}

	//[Command]
	void Remove() {
		bool successful = PlayerContainer.Remove (player);
		UIDebugScript.write (successful.ToString ());
		NetworkServer.Destroy (g_fighter);
		Debug.Log (player.ToString () + " quit the game.");
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!isLocalPlayer)
			return;
		UpdateInput ();
	}

	void UpdateInput() {
		JoystickPosition = inputManager.JoystickPosition;
		isJoystickButtonPressed = inputManager.isJoystickButtonPressed;
		isAttackButtonPressed = inputManager.isAttackButtonPressed;
		isGuardButtonPressed = inputManager.isGuardButtonPressed;
		isJumpButtonPressed = inputManager.isJumpButtonPressed;
		isWalk = inputManager.isWalk;
		isDash = inputManager.isDash;
		inputType = inputManager.inputType;
		inputDirection = inputManager.inputDirection;
		CmdUpdateInput(JoystickPosition,
			isJoystickButtonPressed,
			isAttackButtonPressed,
			isGuardButtonPressed,
			isJumpButtonPressed,
			isWalk,
			isDash,
			inputType,
			inputDirection);
	}

	[Command]
	void CmdUpdateInput(Vector2 JoystickPosition,
		bool isJoystickButtonPressed,
		bool isAttackButtonPressed,
		bool isGuardButtonPressed,
		bool isJumpButtonPressed,
		bool isWalk,
		bool isDash,
		InputType inputType,
		Vector2 inputDirection) {
		this.JoystickPosition = JoystickPosition;
		this.isJoystickButtonPressed = isJoystickButtonPressed;
		this.isAttackButtonPressed = isAttackButtonPressed;
		this.isGuardButtonPressed = isGuardButtonPressed;
		this.isJumpButtonPressed = isJumpButtonPressed;
		this.isWalk = isWalk;
		this.isDash = isDash;
		this.inputType = inputType;
		this.inputDirection = inputDirection;
	}

	public void Reset() {
		RpcReset ();
	}

	[ClientRpc]
	void RpcReset() {
		if (inputManager != null) {
			inputManager.inputType = InputType.None;
			inputType = InputType.None;
		}
	}

	void OnDestroy() {
		if (!isServer)
			return;
		Remove ();
	}
}
