using UnityEngine;

public class descr : MonoBehaviour
{
    public float speed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.A)) 
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }    
}
