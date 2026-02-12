using UnityEngine;

public class DogController : MonoBehaviour
{
    public Transform player;
    public Transform waypoint;

    public float followDistance = 2f;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float rotationSpeed = 5f;

    private Animator anim;
    private bool goToWaypoint = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (goToWaypoint && waypoint != null)
        {
            MoveToTarget(waypoint.position, runSpeed, true);
        }
        else
        {
            FollowPlayer();
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

            // Move
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            // Rotate smoothly
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            // Animator control
            anim.SetBool("Walk", !running);
            anim.SetBool("Run", running);
        }
        else
        {
            StopMovement();
            goToWaypoint = false;
        }
    }

    void StopMovement()
    {
        anim.SetBool("Walk", false);
        anim.SetBool("Run", false);
    }

    // Call this when puzzle starts
    public void TriggerWaypoint()
    {
        goToWaypoint = true;
    }
}
