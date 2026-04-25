using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


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

    // Keys
    private KeyCode moveLeftKey;
    private KeyCode moveRightKey;
    private KeyCode interactionKey;

    private float currentSpeed;
    private bool isPushing = false;
    private pushable currentPushable;
    private DogController dog;

    void Start()
    {
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
        if (!isPushing && !isFrozenByFear && Input.GetKeyDown(KeyCode.G)) // Press G to throw
        {
            ThrowObject();
            anim.SetBool("Throw", true);
        }
        else
        {
            anim.SetBool("Throw", false);
        }
    }
    void ThrowObject()
    {
        GameObject ball = Instantiate(ballPrefab, throwPoint.position, throwPoint.rotation);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        Vector3 throwDir = (transform.forward + Vector3.up * 0.2f).normalized;
        rb.AddForce(throwDir * throwForce, ForceMode.Impulse);
        if (dog != null) dog.GoFetch(ball.transform);
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
