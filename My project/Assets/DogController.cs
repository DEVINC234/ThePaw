using UnityEngine;
using System.Collections;

public class DogController : MonoBehaviour
{
    public enum DogState { Intro, Follow, MovingToWaypoint, Waiting, Alert, Fetching }
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

    [Header("Fetch Settings")]
    public Transform mouthSocket; // Create an empty GameObject at the dog's mouth
    private Transform targetItem;
    private bool hasItem = false;

    public Transform player;
    private Animator anim;

    void Start()
    {
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

    }
    void HandleFetchLogic()
    {
        if (targetItem == null)
        {
            currentState = DogState.Follow;
            return;
        }

        float distToItem = Vector3.Distance(transform.position, targetItem.position);

        if (!hasItem)
        {
            // 1. Run to the item
            MoveToTarget(targetItem.position, runSpeed, true);

            if (distToItem < 1.0f)
            {
                // 2. Pick it up (Limited anim: we just snap it to mouth)
                fetchItem itemScript = targetItem.GetComponent<fetchItem>();
                if (itemScript != null)
                {
                    itemScript.OnPickedUp(mouthSocket);
                    hasItem = true;
                    anim.SetTrigger("Bark"); // Small visual feedback
                }
            }
        }
        else
        {
            // Dog brings it back to the player
            MoveToTarget(player.position, walkSpeed, false);

            if (Vector3.Distance(transform.position, player.position) < stopDistance)
            {
                targetItem.GetComponent<fetchItem>().OnDropped();

                Destroy(targetItem.gameObject);

                hasItem = false;
                targetItem = null;
                currentState = DogState.Follow;

                Debug.Log("Item returned and collected by player.");
            }
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

        if (direction.magnitude > 0.1f)
        {
            direction.Normalize();
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            anim.SetBool("Walk", !running);
            anim.SetBool("Run", running);
        }
        else
        {
            if (currentState == DogState.MovingToWaypoint)
            {
                currentState = DogState.Waiting;
                waitTimer = 0f;
            }
            StopMovement();
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
