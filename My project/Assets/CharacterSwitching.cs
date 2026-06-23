using UnityEngine;

public class CharacterSwitching : MonoBehaviour
{
    public PlayerController boyScript;
    public DogController dogScript;
    public GameObject boyCamera;
    public GameObject dogCamera;
    void Start()
    {
        dogCamera.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleCharacter(!dogScript.isControlled);
        }
    }
    public void ToggleCharacter(bool toDog)
    {
        boyScript.isControlled = !toDog;
        dogScript.isControlled = toDog;

        if (boyCamera != null) boyCamera.SetActive(!toDog);
        if (dogCamera != null) dogCamera.SetActive(toDog);

        Debug.Log(toDog ? "Now controlling Dog" : "Now controlling Boy");
    }
}
