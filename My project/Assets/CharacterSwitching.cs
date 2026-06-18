using UnityEngine;

public class CharacterSwitching : MonoBehaviour
{
    public PlayerController boyScript;
    public DogController dogScript;
    public GameObject boyCamera;
    public GameObject dogCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dogCamera.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleCharacter(!dogScript.isControlled);
        }
    }
    public void ToggleCharacter(bool toDog)
    {
        // Switch Logic
        boyScript.isControlled = !toDog;
        dogScript.isControlled = toDog;

        // Switch Cameras
        if (boyCamera != null) boyCamera.SetActive(!toDog);
        if (dogCamera != null) dogCamera.SetActive(toDog);

        Debug.Log(toDog ? "Now controlling Dog" : "Now controlling Boy");
    }
}
