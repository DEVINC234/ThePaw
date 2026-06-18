using UnityEngine;
using UnityEngine.UI;

public class PlayerInvectory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public GameObject[] items; 
    private int currentIndex = 0;
    public Text itText;
    public bool hasBallInPocket;

    void Start()
    {
        if (items[1] != null)
        {
            items[1].SetActive(false);
            itText.gameObject.SetActive(false);
        }
    }

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
        hasBallInPocket = true;
        currentIndex = 0;      
        if(hasBallInPocket == true)
        {
            if (items[1] != null)
            {
                items[1].SetActive(true);
            }
        }
        //SwitchItem();

        if (itText != null)
        {
            itText.gameObject.SetActive(true);
        }
    }

    public void RemoveBallFromHand()
    {
        hasBallInPocket = false;
        if (items[1] != null)
        {
            items[1].SetActive(false); 
            itText.gameObject.SetActive(false);
        }
        currentIndex = 1;
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
        if (currentIndex < items.Length && items[currentIndex] != null)
        {
            return items[currentIndex].CompareTag("Ball") && items[currentIndex].activeSelf;
        }
        return false;
    }
}

