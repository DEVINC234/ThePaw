using UnityEngine;

public class DogController : MonoBehaviour
{
    [Header("Movement Distances")]
    public float stopDistance = 1.5f;   // Distance to stop walking
    public float runDistance = 5f;     // Distance where dog starts running to catch up
    public float followDistance = 2f;  // Minimum distance to start following

    [Header("Speeds")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float rotationSpeed = 8f;

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

    public Transform player;
    private Animator anim;
    private CharacterController controller;

    public enum DogState { Intro, Follow, MovingToWaypoint, Waiting }
    private DogState currentState;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        currentState = playIntro ? DogState.Intro : DogState.Follow;
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case DogState.Intro:
                PlayfulCircle();
                break;

            case DogState.Follow:
                HandleFollowLogic();
                break;

            case DogState.MovingToWaypoint:
                if (waypoint != null)
                    MoveToTarget(waypoint.position, runSpeed);
                break;

            case DogState.Waiting:
                HandleWaitingLogic();
                break;
        }
    }

    void HandleFollowLogic()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > runDistance)
        {
            // Player is far away, sprint to catch up!
            MoveToTarget(player.position, runSpeed);
        }
        else if (distance > followDistance)
        {
            // Player is moving, just walk along
            MoveToTarget(player.position, walkSpeed);
        }
        else if (distance <= stopDistance)
        {
            StopMovement();
        }
    }

    void HandleWaitingLogic()
    {
        StopMovement();

        // Timer logic to return to player automatically
        waitTimer += Time.deltaTime;
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // Return to follow if time is up OR if player gets close
        if (waitTimer >= waitTimeAtWaypoint || distToPlayer < followDistance)
        {
            ReturnToPlayer();
        }
    }

    void MoveToTarget(Vector3 targetPos, float currentSpeed)
    {
        Vector3 direction = targetPos - transform.position;
        direction.y = 0f;

        if (direction.magnitude > 0.1f)
        {
            direction.Normalize();

            // Using Move instead of Translate for better physics interaction
            if (controller != null)
                controller.SimpleMove(direction * currentSpeed);
            else
                transform.Translate(direction * currentSpeed * Time.deltaTime, Space.World);

            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            // Set animations based on actual speed
            bool isRunning = currentSpeed > walkSpeed + 0.1f;
            anim.SetBool("Walk", !isRunning);
            anim.SetBool("Run", isRunning);
        }
        else
        {
            // If we reached the target and were heading to a waypoint, start waiting
            if (currentState == DogState.MovingToWaypoint)
            {
                currentState = DogState.Waiting;
                waitTimer = 0f; // Reset timer
            }
            StopMovement();
        }
    }

    void StopMovement()
    {
        anim.SetBool("Walk", false);
        anim.SetBool("Run", false);
    }

    public void TriggerWaypoint()
    {
        if (currentState == DogState.Follow && waypoint != null)
        {
            currentState = DogState.MovingToWaypoint;
        }
    }

    public void ReturnToPlayer()
    {
        currentState = DogState.Follow;
    }

    void PlayfulCircle()
    {
        introTimer += Time.deltaTime;
        circleAngle += circleSpeed * Time.deltaTime;

        float x = Mathf.Cos(circleAngle) * circleRadius;
        float z = Mathf.Sin(circleAngle) * circleRadius;

        Vector3 circlePos = player.position + new Vector3(x, 0f, z);
        MoveToTarget(circlePos, walkSpeed);

        if (introTimer >= introDuration)
        {
            playIntro = false;
            currentState = DogState.Follow;
        }
    }
}
