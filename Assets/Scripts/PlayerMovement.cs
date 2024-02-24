using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class PlayerMovement : MonoBehaviour
{
    public float mouseSensitivity = 3f;
    public Transform playerBody;
    private Camera mainCamera;
    float xRotation = 0f;
    private bool cursorLocked;
    public CharacterController controller;
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public float rotationSpeed;
    public Quaternion newResetAngle;

    [SerializeField] private Animator Animator;
    Vector3 velocity;
    bool isGrounded;
    private bool isPraying;
    private PhotonView view;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        if (view.IsMine)
        {
            Debug.Log("Setting up camera for my player");
            mainCamera = GetComponentInChildren<Camera>();
            mainCamera.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            cursorLocked = true;
            controller = GetComponent<CharacterController>();
        }
        else
        {
            Debug.Log("Disabling camera for other player's prefab");
            Camera otherPlayerCamera = GetComponentInChildren<Camera>();
            if (otherPlayerCamera != null)
                otherPlayerCamera.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    void PlayMusic()
    {
        VideoPlayer audioSource = FindObjectOfType<VideoPlayer>();
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    [PunRPC]
    void TriggerDanceAnimation()
    {
        isPraying = true;
        Animator.SetTrigger("Dance");
        GetComponentInChildren<MouseLook>()
            .ToggleFaceFocus(true); // Assuming MouseLook is attached to a child of the player
    }

    public void OnDanceAnimationEnd()
    {
        isPraying = false;
        GetComponentInChildren<MouseLook>().ToggleFaceFocus(false); // Return camera control to normal
    }

    void Update()
    {
        if (!view.IsMine || isPraying)
            return;

        if (Input.GetKeyDown(KeyCode.N))
        {
            view.RPC("PlayMusic", RpcTarget.All);
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            //controller.slopeLimit = 45.0f;
            velocity.y = -2f;
        }

        if (Input.GetButton("Vertical") || Input.GetButton("Horizontal"))
        {
            newResetAngle = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0);
            playerBody.transform.rotation = newResetAngle;
            Animator.SetBool("IsWalking", true);
        }
        else
        {
            Animator.SetBool("IsWalking", false);
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        if (move.magnitude > 1)
            move /= move.magnitude;

        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)

        {
            // controller.slopeLimit = 100.0f;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Animator.SetTrigger("Jump");
        }


        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.F))
        {
            view.RPC("TriggerDanceAnimation", RpcTarget.All);
        }

        if (Input.GetKeyDown(KeyCode.Escape) && Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            cursorLocked = false;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            cursorLocked = true;
        }
    }
}