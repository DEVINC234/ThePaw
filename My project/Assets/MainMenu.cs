using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Scenes")]
    public string gameSceneName = "GameScene";

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("Rebinding UI")]
    public Text leftKeyText;
    public Text rightKeyText;
    public Text interactKeyText;

    private bool isRebinding = false;

    void Start()
    {
        ShowMainMenu();
        UpdateUI();
    }

    // --- Basic Navigation ---
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Exited");
    }

    // --- Key Rebinding Logic ---
    public void StartRebind(string action)
    {
        if (!isRebinding) StartCoroutine(RebindRoutine(action));
    }

    IEnumerator RebindRoutine(string action)
    {
        isRebinding = true;

        // Show the user we are waiting
        SetText(action, "...");

        // Wait for the mouse click to release
        yield return new WaitUntil(() => !Input.GetMouseButton(0));

        KeyCode detectedKey = KeyCode.None;
        while (detectedKey == KeyCode.None)
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
                {
                    // Ignore Mouse0 so the button click doesn't bind itself
                    if (Input.GetKeyDown(k) && k != KeyCode.Mouse0)
                    {
                        detectedKey = k;
                        break;
                    }
                }
            }
            yield return null;
        }

        // Save to the "Bridge"
        PlayerPrefs.SetString("Key_" + action, detectedKey.ToString());
        PlayerPrefs.Save();

        UpdateUI();
        isRebinding = false;
        Debug.Log("Saved " + action + " as " + detectedKey);
    }

    void UpdateUI()
    {
        if (leftKeyText != null) leftKeyText.text = PlayerPrefs.GetString("Key_Left", "A");
        if (rightKeyText != null) rightKeyText.text = PlayerPrefs.GetString("Key_Right", "D");
        if (interactKeyText != null) interactKeyText.text = PlayerPrefs.GetString("Key_Interact", "E");
    }

    void SetText(string action, string t)
    {
        if (action == "Left") leftKeyText.text = t;
        if (action == "Right") rightKeyText.text = t;
        if (action == "Interact") interactKeyText.text = t;
    }
}
