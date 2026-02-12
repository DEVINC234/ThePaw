using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator anim;
    public float currentSpeed = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSpeed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, 0);
            anim.SetFloat("Run", moveSpeed);

        }
        else
        {
            anim.SetFloat("Run", 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 180, 0);
            anim.SetFloat("Run", moveSpeed);
        }
        if (Input.GetKey(KeyCode.E))
        {
            DogController dog = FindObjectOfType<DogController>();
            dog.TriggerWaypoint();

        }
    }
}
