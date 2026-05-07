using UnityEngine;
using UnityEngine.UI;

public class PlayerInvectory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public GameObject[] items; 
    private int currentIndex = 0;
    public Text itText;
    public bool hasBallInPocket;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hasBallInPocket)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll > 0f)
            {
                currentIndex++;
                if (currentIndex >= items.Length) currentIndex = 0;
                
                SwitchItem();
            }
            else if (scroll < 0f)
            {
                currentIndex--;
                if (currentIndex < 0) currentIndex = items.Length - 1;
                
                SwitchItem();
            }
            if(currentIndex == 0)
            {
                itText.gameObject.SetActive(true);
            }
             else
            {
                itText.gameObject.SetActive(false);
            }
        }
    }
    public void CollectBall()
    {
        hasBallInPocket = true; // Unlocks the scroll wheel
        currentIndex = 0;      // Move to the ball slot

        // Force the inventory to refresh and show the item
        //SwitchItem();

        if (itText != null)
        {
            itText.gameObject.SetActive(true);
        }
    }

    // Call this the moment the ball leaves the hand
    public void RemoveBallFromHand()
    {
        hasBallInPocket = false;
        if (items[1] != null)
        {
            items[1].SetActive(false); 
            itText.gameObject.SetActive(false);
        }
        currentIndex = 1; // Switch back to Empty Hand (GTA style)
        //itText.gameObject.SetActive(false);
    }
    public void SwitchItem()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
                items[i].SetActive(i == currentIndex);
        }
    }
    public bool IsHoldingBall()
    {
        // Check if current item is not the "Empty Hand" (index 0) 
        // and if the active item has the "Ball" tag
        if (currentIndex < items.Length && items[currentIndex] != null)
        {
            return items[currentIndex].CompareTag("Ball") && items[currentIndex].activeSelf;
        }
        return false;
    }
}

