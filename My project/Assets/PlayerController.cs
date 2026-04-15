using UnityEngine;


public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float normalSpeed = 5f;
    public float pushSpeed = 2f;
    public Animator anim;

    [Header("Push/Pull Settings")]
    public float interactionDistance = 1.0f;
    public LayerMask pushLayer;

    // Dynamic Keys (Loaded from Menu)
    private KeyCode moveLeftKey;
    private KeyCode moveRightKey;
    private KeyCode interactionKey;

    private float currentSpeed;
    private bool isPushing = false;
    private pushable currentPushable;
    private DogController dog;
    private Rigidbody rb;

    void Start()
    {
        // Add this line to force the game to read the LATEST saves
        LoadControls();

        currentSpeed = normalSpeed;
        dog = FindObjectOfType<DogController>();
        rb = GetComponent<Rigidbody>();
    }

    public void LoadControls()
    {
        string debugLeft = PlayerPrefs.GetString("Key_Left", "DEFAULT_A");
        Debug.Log("<color=cyan>PLAYER SCRIPT LOADING:</color> Left Key found in prefs is: " + debugLeft);

        moveLeftKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_Left", "A"));
        moveRightKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_Right", "D"));
        interactionKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_Interact", "E"));
    }

    void Update()
    {
        HandlePushInput();
        HandleMovement();
    }

    void HandleMovement()
    {
        float moveInput = 0;

        // 1. Check Rebindable Keyboard Keys
        if (Input.GetKey(moveLeftKey)) moveInput = -1;
        if (Input.GetKey(moveRightKey)) moveInput = 1;

        // 2. Check Joystick (Horizontal Axis)
        // This allows for analog "creeping" or full "HST" throttle
        float joyInput = Input.GetAxis("Horizontal");

        // Combine them (clamped so you don't go double speed if pressing both)
        float combinedInput = Mathf.Clamp(moveInput + joyInput, -1f, 1f);

        if (Mathf.Abs(combinedInput) > 0.1f)
        {
            if (!isPushing)
            {
                // Smooth rotation based on combined input direction
                float targetY = (combinedInput < 0) ? 0 : 180;
                transform.rotation = Quaternion.Euler(0, targetY, 0);
            }

            // Move the player
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
            anim.SetFloat("Run", currentSpeed);
        }
        else
        {
            anim.SetFloat("Run", 0);
        }
    }

    void HandlePushInput()
    {
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, interactionDistance, pushLayer);

        // Uses the dynamic interaction key
        if (hitSomething && Input.GetKey(interactionKey))
        {
            pushable p = hit.collider.GetComponent<pushable>();
            if (p != null)
            {
                StartPushing(p);

                // Direction logic for the crate
                float moveDir = 0;
                if (Input.GetKey(moveLeftKey) || Input.GetAxis("Horizontal") < -0.1f) moveDir = 1;
                if (Input.GetKey(moveRightKey) || Input.GetAxis("Horizontal") > 0.1f) moveDir = -1;

                currentPushable.StartPush(transform.forward * moveDir);
                return;
            }
        }

        if (isPushing) StopPushing();

        // Trigger dog waypoint with dynamic key
        if (!isPushing && Input.GetKeyDown(interactionKey))
        {
            if (dog != null) dog.TriggerWaypoint();
        }
    }

    void StartPushing(pushable p)
    {
        isPushing = true;
        currentPushable = p;
        currentSpeed = pushSpeed;
        anim.SetBool("Push", true);
    }

    void StopPushing()
    {
        if (currentPushable != null) currentPushable.StopPush();
        isPushing = false;
        currentPushable = null;
        currentSpeed = normalSpeed;
        anim.SetBool("Push", false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * interactionDistance);
    }
}
