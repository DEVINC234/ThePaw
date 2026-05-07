using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float normalSpeed = 5f;
    public float pushSpeed = 2f;
    public Animator anim;

    [Header("UI Settings")]
    public Text interactionText;
    public float detectionRange = 2.5f;

    [Header("Trauma Visuals")]
    public PostProcessVolume traumaVolume;
    public float fearFreezeDuration = 3.0f;
    private bool isFrozenByFear = false;
    private Vignette vignette;
    private ColorGrading colorGrading;

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
    public float checkDistance = 0.2f;
    public LayerMask groundLayer;
    private Rigidbody rb;
    private bool isGrounded;

    [Header("Interaction Settings")]
    public Transform handSocket;
    public float grabDistance = 2.0f;
    private fetchItem currentItem;

    private KeyCode moveLeftKey;
    private KeyCode moveRightKey;
    private KeyCode interactionKey;

    private float currentSpeed;
    private bool isPushing = false;
    private pushable currentPushable;
    private DogController dog;
    private bool isHoldingBall = false;

    public PlayerInvectory inv;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        LoadControls();
        currentSpeed = normalSpeed;
        dog = FindObjectOfType<DogController>();
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

        // MODIFIED GRAB CHECK
        if (Input.GetKeyDown(interactionKey) && !isHoldingBall)
        {
            StartCoroutine(GrabSequence());
        }

        // MODIFIED THROW CHECK (Checks Inventory state)
        if (inv != null && inv.IsHoldingBall() && Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(ThrowSequence());
            anim.SetBool("Throw", true);
        }
        else
        {
            anim.SetBool("Throw", false);
        }

        isGrounded = Physics.Raycast(transform.position, Vector3.down, checkDistance, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            anim.SetBool("isJumping", true);
            PerformJump();
        }
        else
        {
            anim.SetBool("isJumping", false);
        }

        UpdateInteractionUI();
    }

    // --- CORE MODIFICATIONS BELOW ---

    IEnumerator GrabSequence()
    {
        anim.SetBool("isGrabbing", true);
        yield return new WaitForSeconds(0.5f);

        Collider[] itemsFound = Physics.OverlapSphere(transform.position, grabDistance);
        foreach (var col in itemsFound)
        {
            fetchItem item = col.GetComponent<fetchItem>();
            if (item != null)
            {
                // 1. Logic Hand-off
                currentItem = item;
                isHoldingBall = true;

                // 2. Tell Inventory to take over (Unlock Scroll + Show Hand-Ball)
                if (inv != null)
                {
                    inv.CollectBall();
                }
                item.OnPickedUp(throwPoint);
                // 3. Deactivate World Object
               
                break;
            }
        }
        anim.SetBool("isGrabbing", false);
    }

    IEnumerator ThrowSequence()
    {
        yield return new WaitForSeconds(0.3f);

        if (currentItem != null)
        {
            // 1. Tell Inventory the ball is gone (Lock Scroll + Hide Hand-Ball)
            if (inv != null)
            {
                inv.RemoveBallFromHand();
            }

            interactionText.gameObject.SetActive(false);
            // 2. Re-activate the world ball at hand position
            //currentItem.gameObject.SetActive(true);
            currentItem.transform.position = handSocket.position;

            // 3. Physic Release
            Rigidbody itemRb = currentItem.GetComponent<Rigidbody>();
            currentItem.OnDropped();

            Vector3 throwDir = (transform.forward + Vector3.up * 0.2f).normalized;
            itemRb.AddForce(throwDir * throwForce, ForceMode.Impulse);

            // 4. Dog Logic
            if (dog != null) dog.GoFetch(currentItem.transform);

            // 5. Reset local state
            currentItem = null;
            isHoldingBall = false;
        }
    }

    public void ReceiveBallFromDog(fetchItem returnedItem)
    {
        currentItem = returnedItem;
        isHoldingBall = true;

        // Sync with inventory so the ball appears in hand automatically
        if (inv != null)
        {
            inv.CollectBall();
        }
        interactionText.gameObject.SetActive(true);
        //returnedItem.gameObject.SetActive(false);
        Debug.Log("Ball received from dog. Ready to throw!");
    }

    // [Keeping the rest of your original methods for Movement, Push, and Trauma]
    void UpdateInteractionUI()
    {
        if (interactionText == null) return;
        if (isHoldingBall)
        {
            interactionText.text = "Press [G] to Throw";
            return;
        }
        Collider[] items = Physics.OverlapSphere(transform.position, detectionRange);
        bool canGrab = false;
        foreach (var col in items)
        {
            if (col.GetComponent<fetchItem>() != null) { canGrab = true; break; }
        }
        interactionText.text = canGrab ? "Press [E] to Grab" : "";
    }

    void PerformJump() { rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); }

    void HandleMovement()
    {
        float moveInput = 0;
        if (Input.GetKey(moveLeftKey)) moveInput = 1;
        if (Input.GetKey(moveRightKey)) moveInput = -1;
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            if (!isPushing) { float targetY = (moveInput < 0) ? 180 : 0; transform.rotation = Quaternion.Euler(0, targetY, 0); }
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
            anim.SetFloat("Run", currentSpeed);
        }
        else anim.SetFloat("Run", 0);
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
        if (!isPushing && Input.GetKeyDown(interactionKey)) { if (dog != null) dog.TriggerWaypoint(); }
    }

    void StartPushing(pushable p) { isPushing = true; currentPushable = p; currentSpeed = pushSpeed; anim.SetBool("Push", true); }
    void StopPushing() { if (currentPushable != null) currentPushable.StopPush(); isPushing = false; currentPushable = null; currentSpeed = normalSpeed; anim.SetBool("Push", false); }
    public void ForceUnfreeze() { isFrozenByFear = false; if (vignette != null) vignette.intensity.value = 0; }
    void HandleTraumaState() { if (vignette != null) vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0, Time.deltaTime); if (colorGrading != null) colorGrading.saturation.value = Mathf.Lerp(colorGrading.saturation.value, 0, Time.deltaTime); if (vignette.intensity.value < 0.05f) isFrozenByFear = false; }
}

