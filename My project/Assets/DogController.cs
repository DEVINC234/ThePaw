using UnityEngine;

public class DogController : MonoBehaviour
{
    [Header("Intro Playful Circle")]
    public bool playIntro = true;
    public float circleRadius = 1.5f;
    public float circleSpeed = 2f;
    public float introDuration = 4f;

    private float introTimer;
    private float circleAngle = 0f;

    public Transform player;
    public Transform waypoint;

    public float followDistance = 2f;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float rotationSpeed = 5f;

    private Animator anim;

    public enum DogState
    {
        Intro,
        Follow,
        MovingToWaypoint,
        Waiting
    }

    private DogState currentState;

    void Start()
    {
        anim = GetComponent<Animator>();
        currentState = playIntro ? DogState.Intro : DogState.Follow;
    }

    void Update()
    {
        switch (currentState)
        {
            case DogState.Intro:
                PlayfulCircle();
                break;

            case DogState.Follow:
                FollowPlayer();
                break;

            case DogState.MovingToWaypoint:
                if (waypoint != null)
                    MoveToTarget(waypoint.position, runSpeed, true);
                break;

            case DogState.Waiting:
                StopMovement();
                break;
        }
    }

    void FollowPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > followDistance)
        {
            MoveToTarget(player.position, walkSpeed, false);
        }
        else
        {
            StopMovement();
        }
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
            StopMovement();

            if (currentState == DogState.MovingToWaypoint)
                currentState = DogState.Waiting;
        }
    }

    void StopMovement()
    {
        anim.SetBool("Walk", false);
        anim.SetBool("Run", false);
    }

    public void TriggerWaypoint()
    {
        if (currentState == DogState.Follow)
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
        if (player == null) return;

        introTimer += Time.deltaTime;
        circleAngle += circleSpeed * Time.deltaTime;

        float x = Mathf.Cos(circleAngle) * circleRadius;
        float z = Mathf.Sin(circleAngle) * circleRadius;

        Vector3 circlePosition = player.position + new Vector3(x, 0f, z);

        Vector3 direction = (circlePosition - transform.position).normalized;

        transform.Translate(direction * walkSpeed * Time.deltaTime, Space.World);

        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        anim.SetBool("Walk", false);
        anim.SetBool("Run", true);

        if (introTimer >= introDuration)
        {
            playIntro = false;
            currentState = DogState.Follow;
            anim.SetBool("Run", false);
        }
    }
}
