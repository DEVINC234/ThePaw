using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Light visionLight;
    public float range = 10f;
    public float angle = 45f;
    public float chaseRange = 6f; 
    public float moveSpeed = 2f;
    public Transform[] waypoints;
    private int currentWaypoint = 0;

    private bool isDisabled = false;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
    }
    void Update()
    {
        if (isDisabled) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer < chaseRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
        //CheckVision();
        SearchForPlayer();
    }
    void ChasePlayer()
    {
        // Bully slowly walks toward the player
        Vector3 targetPos = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * 1.2f * Time.deltaTime);
        transform.LookAt(targetPos);
    }
    void CheckVision()
    {
        Vector3 dir = (playerTransform.position - transform.position);
        // If player is inside the cone, trigger the trauma
        if (Vector3.Angle(transform.forward, dir) < 25f && dir.magnitude < range)
        {
            //playerTransform.GetComponent<PlayerController>().GetSpotted();
        }
    }
    void Patrol()
    {
        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypoint].position, 2f * Time.deltaTime);
        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) < 0.2f)
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        transform.LookAt(waypoints[currentWaypoint]);
    }

    void SearchForPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 dirToPlayer = player.transform.position - transform.position;

        if (Vector3.Angle(transform.forward, dirToPlayer) < angle / 2 && dirToPlayer.magnitude < range)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, dirToPlayer.normalized, out hit, range))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    //player.GetComponent<PlayerController>().GetSpotted();
                    visionLight.color = Color.red;
                }
            }
        }
        else { visionLight.color = Color.white; }
    }

    public void DisableSentinel(float duration)
    {
        StartCoroutine(TempDisable(duration));
    }

    System.Collections.IEnumerator TempDisable(float d)
    {
        isDisabled = true;
        visionLight.enabled = false;
        yield return new WaitForSeconds(d);
        isDisabled = false;
        visionLight.enabled = true;
    }
}
