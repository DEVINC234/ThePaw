using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    [Header("Base Offset")]
    public Vector3 offset;

    [Header("X Offset Values")]
    public float idleX = -3.5f;
    public float moveX = -4.3f;

    public float followSpeed = 5f;
    public float shiftSpeed = 3f;

    private Vector3 lastPlayerPos;

    void Start()
    {
        lastPlayerPos = player.position;
    }

    void LateUpdate()
    {
        float movement = Vector3.Distance(player.position, lastPlayerPos);

        float targetX = movement > 0.01f ? moveX : idleX;

        // Smoothly change X offset
        offset.x = Mathf.Lerp(offset.x, targetX, shiftSpeed * Time.deltaTime);

        // Follow player
        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        lastPlayerPos = player.position;
    }
}
