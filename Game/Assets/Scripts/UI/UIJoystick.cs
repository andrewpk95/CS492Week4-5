using UnityEngine;
using System.Collections;

/// <summary>
/// Joystick widget for NGUI.
/// </summary>
[RequireComponent(typeof(UIWidget))]
[RequireComponent(typeof(BoxCollider))]
public class UIJoystick : MonoBehaviour
{
    public UISprite joystickSprite;
    public Vector2 position;
    public bool isNormalize = false;
    public bool isKeyboardEmulation = true;

    bool lastKeyboardInput = false;
    int touchId = -10;
    public bool isPressed;
    public bool isInitialized = false;
    UIWidget widget;
    Plane plane;
    Vector2 innerPosition;
    Vector2 centerPosition;
    Vector2 lastPosition;
    float radius;

	//Emulate on Keyboard
	public KeyCode leftKey;
	public KeyCode rightKey;
	public KeyCode upKey;
	public KeyCode downKey;

    void OnEnable()
    {
        widget = GetComponent<UIWidget>();

        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            isKeyboardEmulation = false;
        }
    }

    /// <summary>
    /// Initialize joysticks. This can't performed in OnEnable() or Start() because NGUI 
    /// </summary>
    void Initialize()
    {
		centerPosition = UICamera.mainCamera.WorldToScreenPoint(transform.position);

        radius = (widget.height) * 0.5f * Screen.height / widget.root.manualHeight;

		Transform trans = UICamera.mainCamera.transform;
        plane = new Plane(trans.rotation * Vector3.back, UICamera.lastHit.point);

        isInitialized = true;
    }

    void OnPress(bool pressed)
    {
        if (pressed)
        {
            if (isInitialized == false)
            {
                Initialize();
            }

            touchId = UICamera.currentTouchID;
            innerPosition = UICamera.currentTouch.pos;

            ApplyMovement();

            isPressed = true;
        }
        else
        {
            if (touchId == UICamera.currentTouchID)
            {
                touchId = -10;
                ClearPosition();

                isPressed = false;
            }
        }
    }

    void Update()
    {
        if (isKeyboardEmulation)
        {
            EmulateKeyboard();
        }

		if (isPressed)
        {
            innerPosition = UICamera.GetTouch(touchId).pos;

            ApplyMovement();
        }
    }

    void EmulateKeyboard()
    {
        if (isInitialized == false)
        {
            Initialize();
        }

        bool isKeyboardInput = false;
        Vector2 keyboardDelta = Vector2.zero;

		if (Input.GetKey(upKey))
        {
			//isPressed = true;//
            keyboardDelta += new Vector2(0, 1);
            isKeyboardInput = true;
        }
		if (Input.GetKey(downKey))
        {
			//isPressed = true;//
            keyboardDelta += new Vector2(0, -1);
            isKeyboardInput = true;
        }
		if (Input.GetKey(rightKey))
        {
			//isPressed = true;//
            keyboardDelta += new Vector2(1, 0);
            isKeyboardInput = true;
        }
		if (Input.GetKey(leftKey))
        {
			//isPressed = true;//
            keyboardDelta += new Vector2(-1, 0);
            isKeyboardInput = true;
        }
        if (isKeyboardInput == true)
        {
            innerPosition = centerPosition + keyboardDelta * radius;
            ApplyMovement();
        }
        else
        {
            if (lastKeyboardInput == true)
            {
                // Release joystick.
				//isPressed = false;//
                ClearPosition();
            }
        }

        lastKeyboardInput = isKeyboardInput;
    }

    void ClearPosition()
    {
        position = Vector2.zero;
        innerPosition = centerPosition;

        ApplyMovement();
    }

    void ApplyMovement()
    {
        Vector2 positionDelta = innerPosition - centerPosition;
		UIDebugScript.write (joystickSprite.name + ": (" + position.x + ", " + position.y + ")");
        if (isNormalize == false)
        {
            if (positionDelta.x > radius)
            {
                positionDelta.x = radius;
            }
            if (positionDelta.x < -radius)
            {
                positionDelta.x = -radius;
            }
            if (positionDelta.y > radius)
            {
                positionDelta.y = radius;
            }
            if (positionDelta.y < -radius)
            {
                positionDelta.y = -radius;
            }
        }
        else
        {
            if (positionDelta.magnitude > radius)
            {
                positionDelta = positionDelta.normalized * radius;
            }
        }

        position.x = positionDelta.x / radius;
        position.y = positionDelta.y / radius;

        innerPosition = centerPosition + positionDelta;

		Ray ray = UICamera.mainCamera.ScreenPointToRay(innerPosition);
        float dist = 0.0f;

        if (plane.Raycast(ray, out dist))
        {
            Vector3 currentPosition = ray.GetPoint(dist);

            // Release joystick.
            joystickSprite.transform.position = currentPosition;
        }
    }
}
