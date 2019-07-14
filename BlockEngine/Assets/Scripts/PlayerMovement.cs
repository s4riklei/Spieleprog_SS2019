using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    private CharacterController characterController;

    private Vector3 moveDirection;

    [SerializeField]
    private Light flashlight = null;

    [SerializeField]
    public float movementSpeed = 5f;

    [SerializeField]
    public float cameraSpeed = 5f;

    [SerializeField]
    public bool yInverted = true;

    [SerializeField]
    public bool positionDebug = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        move();
        look();
        toggleLight();
        cursorLock();

        if (positionDebug)
        {
            print("forward: x: " + transform.forward.x + "  y: " + transform.forward.y + "  z: " + transform.forward.z);
            print("position: x: " + transform.position.x + "  y: " + transform.position.y + "  z: " + transform.position.z);
        }
    }

    void move()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float x = 0f;
            float y = 0f;
            float z = 0f;

            x += Input.GetKey(Constants.rightKey) ? 1 : 0;
            x += Input.GetKey(Constants.leftKey) ? -1 : 0;
            y += Input.GetKey(Constants.upKey) ? 1 : 0;
            y += Input.GetKey(Constants.downKey) ? -1 : 0;
            z += Input.GetKey(Constants.forwardKey) ? 1 : 0;
            z += Input.GetKey(Constants.backKey) ? -1 : 0;

            moveDirection = new Vector3(x, y, z);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= movementSpeed * Time.deltaTime;

            characterController.Move(moveDirection);
        }
    }

    void look()
    {

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Vector3 rotation = new Vector3(yInverted ? Input.GetAxis(Constants.mouseY) * -1 : Input.GetAxis(Constants.mouseY), Input.GetAxis(Constants.mouseX), 0f) * cameraSpeed;

            transform.Rotate(rotation);
            transform.localRotation = Quaternion.Euler(Mathf.Clamp(transform.localEulerAngles.x > 180 ? transform.localEulerAngles.x - 360 : transform.localEulerAngles.x, -80f, 80f), transform.localEulerAngles.y, 0f);

        }
    }

    void cursorLock()
    {
        if (Input.GetKeyDown(Constants.cursorLockKey))
        {
            Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;

            toggleChatUI();
        }
    }

    void toggleLight()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (flashlight != null)
            {
                if (Input.GetKeyDown(Constants.lightKey))
                {
                    flashlight.enabled = !flashlight.enabled;
                }
            }
        }
    }

    void toggleChatUI()
    {
        GameObject UIChatText = GameObject.Find("UIChatText");
        GameObject UIChatInput = GameObject.Find("UIChatInput");
        GameObject InputField = GameObject.Find("InputField");

        UIChatText.GetComponent<CanvasGroup>().alpha = Cursor.lockState == CursorLockMode.None ? 1f : 0f;
        UIChatInput.GetComponent<CanvasGroup>().alpha = Cursor.lockState == CursorLockMode.None ? 1f : 0f;
        InputField.GetComponent<UnityEngine.UI.InputField>().interactable = Cursor.lockState == CursorLockMode.None;
        if (Cursor.lockState == CursorLockMode.None)
        {
            InputField.GetComponent<UnityEngine.UI.InputField>().ActivateInputField();
        }
    }
}
