using UnityEngine;

public class PlayerControler : MonoBehaviour {
    private CharacterController controller;

    [Header("Player Settings")]
    public float speed; // Скорость игрока
    public float crouchSpeed = 2f; // Скорость при присидание
    public float gravity = -9.81f; // Сила гравитаций
    public AudioSource footstepSource;
    public float stepInterval = 0.5f;
    public AudioClip[] footstepClips;

    [Header("Head Bob")]
    public bool bob;
    public Transform playerCamera;
    public float bobFrequency = 1.5f; 
    public float bobHeight = 0.05f;   
    public float walkingSpeedThreshold = 0.1f; 
    public float smoothReturnSpeed = 4f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1.0f; 
    public float standHeight = 2.0f;  
    public float crouchCameraHeight = 0.5f; 
    public float standCameraHeight = 1.8f; 
    public float crouchTransitionSpeed = 5f;

    private float targetHeight;
    private float currentSpeed; 
    private float headBobTimer = 0f;
    private Vector3 cameraInitialPosition;

    [Header("GroundCheck")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;
    bool isCrouching;
    bool crouchTransition;
    float stepTimer;

    private bool isSitting = false;
    public bool isWalking = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        targetHeight = standHeight;
        currentSpeed = speed;
        cameraInitialPosition = playerCamera.localPosition;
    }

    void Update()
    {
        Movement();
        HeadBob();  
    }

    void PlayFootstepSound()
    {
        stepTimer += Time.deltaTime;

        if(stepTimer >= stepInterval)
        {
            int randomIndex = Random.Range(0, footstepClips.Length);
            footstepSource.clip = footstepClips[randomIndex];
            footstepSource.Play();

            stepTimer = 0f;
        }

        if (isCrouching)
        {
            footstepSource.Stop();
        }
    }

    void Movement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            Crouch();
            isSitting = !isSitting;
        }

        SmoothCrouchTransition();

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (x + z != 0)
        {
            isWalking = true;
        }

        Vector3 move = transform.right * x + transform.forward * z;

        float currentSpeed = isCrouching ? crouchSpeed : speed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if(isGrounded && (x != 0 || z !=0))
        {
            PlayFootstepSound();
        }
        else
        {
            stepTimer = 0;
        }
    }

    void Crouch()
    {
        isCrouching = !isCrouching;
        crouchTransition = true;
        targetHeight = isCrouching ? crouchHeight : standHeight;
        currentSpeed = isCrouching ? crouchSpeed : speed;
    }

    void SmoothCrouchTransition()
    {
        if(crouchTransition)
        {
            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

            float targetCameraHeight = isCrouching ? crouchCameraHeight : standCameraHeight;
            Vector3 cameraPosition = playerCamera.localPosition;
            cameraPosition.y = Mathf.Lerp(playerCamera.localPosition.y, targetCameraHeight, Time.deltaTime * crouchTransitionSpeed);
            playerCamera.localPosition = cameraPosition;

            if(Mathf.Abs(controller.height - targetHeight) < 0.01f)
            {
                controller.height = targetHeight;
                crouchTransition = false;
            }
        }
    }

    void HeadBob()
    {
        float playerSpeed = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).magnitude;

        if (playerSpeed > walkingSpeedThreshold && isGrounded)
        {
            headBobTimer += Time.deltaTime * bobFrequency;

            playerCamera.localPosition = new Vector3(
                cameraInitialPosition.x,
                cameraInitialPosition.y + Mathf.Sin(headBobTimer) * bobHeight,
                cameraInitialPosition.z
            );
        }
        else
        {
            headBobTimer = 0;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, cameraInitialPosition, Time.deltaTime * smoothReturnSpeed);
        }
    }

    public bool IsCrouching()
    {
        return isCrouching;
    }
}


