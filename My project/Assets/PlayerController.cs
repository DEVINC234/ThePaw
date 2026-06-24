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

    [Header("Depth Constraints (2.5D Mode)")]
    public float maxBackgroundDepth = 4f;  
    public float maxForegroundDepth = -4f; 
    public float rotationSpeed = 15f;      

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
    [SerializeField]
    private fetchItem currentItem;
    public Rigidbody ballRB;

    private KeyCode moveLeftKey;
    private KeyCode moveRightKey;
    private KeyCode interactionKey;

    private float currentSpeed;
    private bool isPushing = false;
    private pushable currentPushable;
    private DogController dog;
    [SerializeField]
    private bool isHoldingBall = false;
    [SerializeField]
    private bool nearbyBall = false;

    public PlayerInvectory inv;
    public LayerMask gateLayer;
    public LayerMask BallLayer;

    [Header("Key")]
    public bool isHoldingKey;

    [Header("Switching Logic")]
    public bool isControlled = true;

    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        LoadControls();
        currentSpeed = normalSpeed;
        dog = FindObjectOfType<DogController>();

        if (rb != null)
        {
            rb.freezeRotation = true;
        }

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
        if (!isControlled)
        {
            anim.SetFloat("Run", 0);
            anim.SetBool("Push", false);
            if (rb != null && isGrounded) rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        if (isFrozenByFear)
        {
            HandleTraumaState();
            return;
        }

        GatherInput();
        HandlePushInput();

        if (Input.GetKeyDown(interactionKey) && !isHoldingBall)
        {
            StartCoroutine(GrabSequence());
        }

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

    void FixedUpdate()
    {
        if (!isControlled || isFrozenByFear) return;

        HandleMovementPhysics();
    }


    void GatherInput()
    {
        float moveX = 0f;
        float moveZ = 0f;

        
        if (Input.GetKey(moveLeftKey)) moveX = 1f;
        if (Input.GetKey(moveRightKey)) moveX = -1f;

        
        moveZ = Input.GetAxisRaw("Vertical");

        
        moveInput = new Vector3(moveZ, 0f, moveX).normalized;

        
        if (moveInput.magnitude > 0.01f && !isPushing)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleMovementPhysics()
    {
        Vector3 targetVelocity = moveInput * currentSpeed;

        if (moveInput.magnitude > 0.01f)
        {
            
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
            anim.SetFloat("Run", currentSpeed);
        }
        else
        {
            
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            anim.SetFloat("Run", 0f);
        }

        
        Vector3 boundaryPosition = transform.position;
        boundaryPosition.x = Mathf.Clamp(boundaryPosition.x, maxForegroundDepth, maxBackgroundDepth);
        transform.position = boundaryPosition;
    }

    

    IEnumerator GrabSequence()
    {
        if (nearbyBall == true && !isHoldingBall)
        {
            anim.SetBool("isGrabbing", true);

            yield return new WaitForSeconds(0.5f);

            Collider[] itemsFound = Physics.OverlapSphere(transform.position, grabDistance);

            foreach (var col in itemsFound)
            {
                fetchItem item = col.GetComponent<fetchItem>();
                if (item != null)
                {
                    
                    currentItem = item;
                    isHoldingBall = true;

                    
                    if (inv != null)
                    {
                        inv.CollectBall();
                    }
                    item.OnPickedUp(throwPoint);
                    break;
                }
            }
        }
        anim.SetBool("isGrabbing", false);
    }

    IEnumerator ThrowSequence()
    {
        yield return new WaitForSeconds(0.3f);
        anim.SetBool("Throw", true);

        if (isHoldingBall)
        {
            if (inv != null)
            {
                inv.RemoveBallFromHand();
            }

            interactionText.gameObject.SetActive(false);
            currentItem.transform.position = handSocket.position;

            Rigidbody itemRb = currentItem.GetComponent<Rigidbody>();
            currentItem.OnDropped();

            Vector3 throwDir = (transform.forward + Vector3.up * 0.2f).normalized;
            itemRb.AddForce(throwDir * throwForce, ForceMode.Impulse);

            if (dog != null) dog.GoFetch(currentItem.transform);

            currentItem = null;
            isHoldingBall = false;
        }
        anim.SetBool("Throw", false);
    }

    public void ReceiveBallFromDog(fetchItem returnedItem)
    {
        currentItem = returnedItem;
        isHoldingBall = true;

        if (inv != null)
        {
            inv.CollectBall();
        }
        interactionText.gameObject.SetActive(true);
    }

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

    void PerformJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void HandlePushInput()
    {
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, interactionDistance, pushLayer | gateLayer | BallLayer);
        if (hitSomething && Input.GetKey(interactionKey))
        {
            GateRattle gate = hit.collider.GetComponent<GateRattle>();
            if (gate != null)
            {
                gate.AttemptOpen(this.gameObject, anim);
            }

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
        if (isPushing) { StopPushing(); dog.StopMovement(); }
        if (!isPushing && Input.GetKeyDown(interactionKey)) { if (dog != null) dog.TriggerWaypoint(); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Key"))
        {
            isHoldingKey = true;
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Ball"))
        {
            nearbyBall = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            nearbyBall = false;
        }
    }

    void StartPushing(pushable p) { isPushing = true; currentPushable = p; currentSpeed = pushSpeed; anim.SetBool("Push", true); }
    void StopPushing() { if (currentPushable != null) currentPushable.StopPush(); isPushing = false; currentPushable = null; currentSpeed = normalSpeed; anim.SetBool("Push", false); }
    public void ForceUnfreeze() { isFrozenByFear = false; if (vignette != null) vignette.intensity.value = 0; }
    void HandleTraumaState() { if (vignette != null) vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0, Time.deltaTime); if (colorGrading != null) colorGrading.saturation.value = Mathf.Lerp(colorGrading.saturation.value, 0, Time.deltaTime); if (vignette.intensity.value < 0.05f) isFrozenByFear = false; }
}

