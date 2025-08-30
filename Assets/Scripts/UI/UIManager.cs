using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private bool settingsLoaded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleSettings()
    {
        if (!settingsLoaded)
        {
            SceneManager.LoadScene("SettingsScene", LoadSceneMode.Additive);
            settingsLoaded = true;
            Time.timeScale = 0f;
        }
        else
        {
            CloseSettings();
        }
    }

    public void CloseSettings()
    {
        if (SceneManager.GetSceneByName("SettingsScene").isLoaded)
        {
            SceneManager.UnloadSceneAsync("SettingsScene");
        }

        settingsLoaded = false;
        Time.timeScale = 1f;
    }
}
