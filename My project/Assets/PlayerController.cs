using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;


public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float normalSpeed = 5f;
    public float pushSpeed = 2f;
    public Animator anim;

    [Header("Trauma Visuals")]
    public PostProcessVolume traumaVolume;
    public float fearFreezeDuration = 3.0f;
    private bool isFrozenByFear = false;
    private Vignette vignette;
    private ColorGrading colorGrading; // New for Black and White

    [Header("Push/Pull Settings")]
    public float interactionDistance = 1.0f;
    public LayerMask pushLayer;

    [Header("Fetch Settings")]
    public GameObject ballPrefab;
    public Transform throwPoint; 
    public float throwForce = 12f;
    public float verticalArc = 2f;

    [Header("Physics & Jump")]
    public float jumpForce = 7f;
    public float checkDistance = 0.2f; // Distance for ground check
    public LayerMask groundLayer;
    private Rigidbody rb;
    private bool isGrounded;

    [Header("Interaction Settings")]
    public Transform handSocket; // The empty object in your RightHand bone
    public float grabDistance = 2.0f;
    private fetchItem currentItem;

    // Keys
    private KeyCode moveLeftKey;
    private KeyCode moveRightKey;
    private KeyCode interactionKey;

    private float currentSpeed;
    private bool isPushing = false;
    private pushable currentPushable;
    private DogController dog;
    private bool isHoldingBall = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        LoadControls();
        currentSpeed = normalSpeed;
        dog = FindObjectOfType<DogController>();
        // Setup Post Processing
        if (traumaVolume != null)
        {
            traumaVolume.profile.TryGetSettings(out vignette);
            traumaVolume.profile.TryGetSettings(out colorGrading);
        }
    }

    public void LoadControls()
    {
        moveLeftKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_Left", "A"));
        moveRightKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_Right", "D"));
        interactionKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_Interact", "E"));
    }

    void Update()
    {
        if (isFrozenByFear)
        {
            HandleTraumaState();
            return;
        }

        HandlePushInput();
        HandleMovement();

        if (Input.GetKeyDown(KeyCode.E) && currentItem == null)
        {
            anim.SetBool("isGrabbing", true);
            StartCoroutine(GrabSequence());
        }
        else
        {
            anim.SetBool("isGrabbing", false);
        }


        // Check for "G" to Throw (Only if holding something)
        if (Input.GetKeyDown(KeyCode.G) && currentItem != null)
        {
            StartCoroutine(ThrowSequence());
            anim.SetBool("Throw", true);
        }
        else
        {
            anim.SetBool("Throw", false);
        }
        // 1. Check if the player is touching the ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, checkDistance, groundLayer);

        // 2. Jump Input
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            anim.SetBool("isJumping", true);
            PerformJump();
        }
        else
        {
            anim.SetBool("isJumping", false);
        }

        
    }
    void PerformJump()
    {
        // We reset vertical velocity first so double-jumps don't stack weirdly
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

        // Apply the upward force
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    IEnumerator GrabSequence()
    {
        // Set the bool to true to start the animation
        anim.SetBool("isGrabbing", true);

        // Wait for the hand to reach the item (adjust time to match your anim)
        yield return new WaitForSeconds(0.5f);

        Collider[] items = Physics.OverlapSphere(transform.position, grabDistance);
        foreach (var col in items)
        {
            fetchItem item = col.GetComponent<fetchItem>();
            if (item != null)
            {
                item.OnPickedUp(handSocket);
                currentItem = item;
                isHoldingBall = true;
                break;
            }
        }

        // Turn the bool off so he returns to Idle (holding the ball)
        anim.SetBool("isGrabbing", false);
    }

    IEnumerator ThrowSequence()
    {

        yield return new WaitForSeconds(0.3f);

        if (currentItem != null)
        {
            // 3. Physic Release
            Rigidbody itemRb = currentItem.GetComponent<Rigidbody>();
            currentItem.OnDropped();

            Vector3 throwDir = (transform.forward + Vector3.up * 0.2f).normalized;
            itemRb.AddForce(throwDir * throwForce, ForceMode.Impulse);

            // 4. Tell the Dog to go get it!
            if (dog != null) dog.GoFetch(currentItem.transform);

            currentItem = null; // Clear player's hand
        }
    }
    public void ReceiveBallFromDog(fetchItem returnedItem)
    {
        currentItem = returnedItem;
        isHoldingBall = true;
        // Optional: Play a "catch" animation or sound here
        Debug.Log("Ball received from dog. Ready to throw!");
    }
    public void GetSpotted()
    {
        if (!isFrozenByFear)
        {
            isFrozenByFear = true;
            // Fully close vignette and drain color
            if (vignette != null) vignette.intensity.value = 0.6f;
            if (colorGrading != null) colorGrading.saturation.value = -100f; // Black and White

            if (isPushing) StopPushing();
            anim.SetFloat("Run", 0);
        }
    }

    void HandleTraumaState()
    {
        // Slowly return to normal over time
        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0, Time.deltaTime);

        if (colorGrading != null)
            colorGrading.saturation.value = Mathf.Lerp(colorGrading.saturation.value, 0, Time.deltaTime);

        if (vignette.intensity.value < 0.05f) isFrozenByFear = false;
    }

    void HandleMovement()
    {
        float moveInput = 0;
        if (Input.GetKey(moveLeftKey)) moveInput = 1;
        if (Input.GetKey(moveRightKey)) moveInput = -1;

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            if (!isPushing)
            {
                float targetY = (moveInput < 0) ? 180 : 0;
                transform.rotation = Quaternion.Euler(0, targetY, 0);
            }
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

        if (hitSomething && Input.GetKey(interactionKey))
        {
            pushable p = hit.collider.GetComponent<pushable>();
            if (p != null)
            {
                StartPushing(p);
                float moveDir = 0;
                if (Input.GetKey(moveLeftKey)) moveDir = 1;
                if (Input.GetKey(moveRightKey)) moveDir = -1;
                currentPushable.StartPush(transform.forward * moveDir);
                return;
            }
        }

        if (isPushing) StopPushing();

        // THE "E" TO TRIGGER DOG WAYPOINT
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

    public void ForceUnfreeze()
    {
        isFrozenByFear = false;
        if (vignette != null) vignette.intensity.value = 0;
    }
}
