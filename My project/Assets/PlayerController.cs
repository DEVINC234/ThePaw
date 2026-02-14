using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float normalSpeed = 5f;
    public float pushSpeed = 2f;
    private float currentSpeed;
    public Animator anim;

    [Header("Push Settings")]
    public float pushDistance = 1.2f;
    public LayerMask pushLayer;

    private pushable currentPushable;
    private bool isPushing = false;

    private DogController dog;

    void Start()
    {
        currentSpeed = normalSpeed;
        dog = FindObjectOfType<DogController>();
    }

    void Update()
    {
        HandleMovement();
        HandlePush();
    }

    void HandleMovement()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
            anim.SetFloat("Run", currentSpeed);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
            anim.SetFloat("Run", currentSpeed);
        }
        else
        {
            anim.SetFloat("Run", 0);
        }
    }

    void HandlePush()
    {
        RaycastHit hit;

        // Check if player is close enough and facing a pushable object
        if (Physics.Raycast(transform.position, transform.forward, out hit, pushDistance, pushLayer))
        {
            pushable Pushable = hit.collider.GetComponent<pushable>();
           
            if (Pushable != null)
            {
                // HOLD E to push
                if (Input.GetKey(KeyCode.E))
                {
                    if (!isPushing)
                    {
                        anim.SetBool("Push", true);
                        isPushing = true;
                        currentPushable = Pushable;
                        currentSpeed = pushSpeed;
                    }
                    
                    currentPushable.StartPush(transform.forward);
                    Vector3 targetPosition = currentPushable.transform.position - transform.forward * 1.2f;
                    transform.position = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
                    return;
                }
                else
                {
                    anim.SetBool("Push", false);
                }
            }
        }

        // If we reach here, stop pushing
        StopPushing();

        // If not pushing and E pressed once → trigger dog
        if (!isPushing && Input.GetKeyDown(KeyCode.E))
        {
            if (dog != null)
                dog.TriggerWaypoint();
        }
    }

    void StopPushing()
    {
        if (isPushing && currentPushable != null)
        {
            currentPushable.StopPush();
        }

        isPushing = false;
        currentPushable = null;
        currentSpeed = normalSpeed;
    }
}
