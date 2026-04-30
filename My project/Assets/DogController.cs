using UnityEngine;
using System.Collections;

public class DogController : MonoBehaviour
{
    public enum DogState { Intro, Follow, MovingToWaypoint, Waiting, Alert, Fetching, Returning}
    public DogState currentState;

    [Header("Movement Settings")]
    public float stopDistance = 1.5f;
    public float runDistance = 5f;
    public float followDistance = 2f;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float rotationSpeed = 5f;

    [Header("Waypoint Settings")]
    public Transform waypoint;
    public float waitTimeAtWaypoint = 3f;
    private float waitTimer;

    [Header("Intro Playful Circle")]
    public bool playIntro = true;
    public float circleRadius = 1.5f;
    public float circleSpeed = 2f;
    public float introDuration = 4f;
    private float introTimer;
    private float circleAngle = 0f;

    [Header("Spirit Abilities")]
    public float senseRadius = 10f;
    public KeyCode distractKey = KeyCode.Q;
    private bool isDistracting = false;

    private Rigidbody dogRb;
    public float dogJumpForce = 5f;
    private bool isDogJumping = false;

    [Header("Fetch Settings")]
    public Transform mouthSocket; // Create an empty GameObject at the dog's mouth
    private Transform targetItem;
    private bool hasItem = false;

    public Transform player;
    private Animator anim;

    void Start()
    {
        dogRb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        currentState = playIntro ? DogState.Intro : DogState.Follow;
    }
    public void GoFetch(Transform item)
    {
        targetItem = item;
        currentState = DogState.Fetching;
    }
    void Update()
    {
        if (player == null) return;

        ScanForEnemies();

        switch (currentState)
        {
            case DogState.Intro: PlayfulCircle(); break;
            case DogState.Follow: FollowPlayer(); break;
            case DogState.MovingToWaypoint:
                if (waypoint != null) MoveToTarget(waypoint.position, runSpeed, true);
                break;
            case DogState.Waiting: HandleWaitingLogic(); break;
            case DogState.Alert: HandleAlertLogic(); break;
            case DogState.Fetching:
                HandleFetchLogic();
                break;
        }

        CheckForPlayerDistress();
        HandleHeightLogic();

    }
    void HandleFetchLogic()
    {
        if (targetItem == null)
        {
            currentState = DogState.Follow;
            return;
        }

        float distToItem = Vector3.Distance(transform.position, targetItem.position);
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // PHASE 1: RUN TO THE ITEM
        if (currentState == DogState.Fetching && !hasItem)
        {
            MoveToTarget(targetItem.position, runSpeed, true);

            if (distToItem < 1.0f)
            {
                fetchItem itemScript = targetItem.GetComponent<fetchItem>();
                if (itemScript != null)
                {
                    itemScript.OnPickedUp(mouthSocket);
                    hasItem = true;
                    currentState = DogState.Fetching; // Switch brain to "I am coming back"
                    
                }
            }
        }
        // PHASE 2: BRING IT BACK TO PLAYER
        else if (currentState == DogState.Fetching && hasItem)
        {
            // Aim for a point slightly in front of the player, not their feet
            Vector3 targetPos = player.position + (transform.position - player.position).normalized * 1.2f;
            MoveToTarget(targetPos, walkSpeed, false);

            // Use a slightly larger stopDistance (e.g., 1.8f) to ensure it triggers
            if (Vector3.Distance(transform.position, player.position) < 1.8f)
            {
                StopMovement();
                ExecuteHandover();
            }
        }
       
        
    }
    void HandleHeightLogic()
    {
        float verticalDiff = player.position.y - transform.position.y;

        // If player is significantly higher and we aren't already jumping
        if (verticalDiff > 1.5f && !isDogJumping)
        {
            StartCoroutine(DelayedJump());
        }
    }

    IEnumerator DelayedJump()
    {
        isDogJumping = true;

        // The 1-second delay you asked for
        yield return new WaitForSeconds(1.0f);

        // Apply the physics jump
        if (dogRb != null)
        {
            // Reset velocity so the jump is consistent
            dogRb.linearVelocity = new Vector3(dogRb.linearVelocity.x, 0, dogRb.linearVelocity.z);
            dogRb.AddForce(Vector3.up * dogJumpForce, ForceMode.Impulse);
        }

        // Wait a bit before allowing another jump to prevent "bunny hopping"
        yield return new WaitForSeconds(1.5f);
        isDogJumping = false;
    }

    void ExecuteHandover()
    {
        fetchItem itemScript = targetItem.GetComponent<fetchItem>();
        PlayerController playerScript = player.GetComponent<PlayerController>();

        if (itemScript != null && playerScript != null)
        {
            // SNAP TO PLAYER HAND - No Destroy() here, so the ball stays in the game
            itemScript.OnPickedUp(playerScript.handSocket);
            playerScript.ReceiveBallFromDog(itemScript);

            // RESET DOG FOR NEXT THROW
            hasItem = false;
            targetItem = null;
            currentState = DogState.Follow;

            Debug.Log("Ball handed back to player hand successfully.");
        }
    }
    void ScanForEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, senseRadius);
        bool enemyNearby = false;
        foreach (var hit in hits)
        {
            Enemy s = hit.GetComponent<Enemy>();
            if (s != null)
            {
                enemyNearby = true;
                if (Input.GetKeyDown(distractKey) && !isDistracting) StartCoroutine(DistractEnemy(s));
            }
        }

        if (enemyNearby && currentState == DogState.Follow) currentState = DogState.Alert;
        else if (!enemyNearby && currentState == DogState.Alert) currentState = DogState.Follow;
    }

    void HandleAlertLogic()
    {
        //anim.SetBool("IsAlert", true);
        FollowPlayer();
    }

    void FollowPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > runDistance) MoveToTarget(player.position, runSpeed, true);
        else if (distance > followDistance) MoveToTarget(player.position, walkSpeed, false);
        else StopMovement();
    }

    void HandleWaitingLogic()
    {
        StopMovement();
        waitTimer += Time.deltaTime;
        if (waitTimer >= waitTimeAtWaypoint || Vector3.Distance(transform.position, player.position) < followDistance)
            currentState = DogState.Follow;
    }

    void MoveToTarget(Vector3 targetPos, float speed, bool running)
    {
        Vector3 direction = targetPos - transform.position;
        direction.y = 0f;

        // Only move if we are further than 0.5 units away
        if (direction.magnitude > 0.1f)
        {
            direction.Normalize();
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            // Only rotate if the direction is significant
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            anim.SetBool("Walk", !running);
            anim.SetBool("Run", running);
        }
        else
        {
            StopMovement(); // If we are close enough, just stop!
        }
    }

    void CheckForPlayerDistress()
    {
        if (Vector3.Distance(transform.position, player.position) < stopDistance)
            player.GetComponent<PlayerController>().ForceUnfreeze();
    }

    IEnumerator DistractEnemy(Enemy s)
    {
        isDistracting = true;
        //anim.SetTrigger("Bark");
        s.DisableSentinel(4.0f);
        yield return new WaitForSeconds(5.0f);
        isDistracting = false;
    }

    void StopMovement() { anim.SetBool("Walk", false); anim.SetBool("Run", false); }// anim.SetBool("IsAlert", false); }
    public void TriggerWaypoint() { if (currentState == DogState.Follow) currentState = DogState.MovingToWaypoint; }

    void PlayfulCircle()
    {
        introTimer += Time.deltaTime;
        circleAngle += circleSpeed * Time.deltaTime;
        float x = Mathf.Cos(circleAngle) * circleRadius;
        float z = Mathf.Sin(circleAngle) * circleRadius;
        Vector3 circlePos = player.position + new Vector3(x, 0f, z);
        MoveToTarget(circlePos, walkSpeed, false);
        if (introTimer >= introDuration) { playIntro = false; currentState = DogState.Follow; }
    }
}
