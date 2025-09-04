using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pauseMenuUI;
    public Button resumeButton;
    public Button quitButton;
    public Slider volumeSlider;

    [Header("Audio")]
    public AudioMixer audioMixer;

    private bool isPaused = false;
    private bool isQuitting = false;

    void Start()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        ConfigurarBotones();
        ConfigurarVolumen();
    }

    void ConfigurarBotones()
    {
        // Resume button setup
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }

        // Quit button setup
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);

            // Disable raycast on all child elements
            DesactivarTodosLosRaycast(quitButton.transform);

            // Create clickable area to ensure functionality
            CrearAreaClickeable();
        }
    }

    void DesactivarTodosLosRaycast(Transform parent)
    {
        Graphic[] graphics = parent.GetComponentsInChildren<Graphic>();
        foreach (Graphic graphic in graphics)
        {
            if (graphic.gameObject != quitButton.gameObject)
            {
                graphic.raycastTarget = false;
            }
        }
    }

    void CrearAreaClickeable()
    {
        GameObject clickArea = new GameObject("QuitClickArea");
        clickArea.transform.SetParent(quitButton.transform, false);

        RectTransform rectTransform = clickArea.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        Image invisibleImage = clickArea.AddComponent<Image>();
        invisibleImage.color = new Color(0, 0, 0, 0.01f);
        invisibleImage.raycastTarget = true;

        Button invisibleButton = clickArea.AddComponent<Button>();
        invisibleButton.targetGraphic = invisibleImage;
        invisibleButton.onClick.AddListener(QuitGame);
    }

    void ConfigurarVolumen()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(ChangeVolume);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isQuitting) return;

        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        if (isQuitting) return;

        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ChangeVolume(float volume)
    {
        if (isQuitting) return;

        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("GameVolume", volume);
    }

    public void QuitGame()
    {
        if (isQuitting) return;
        isQuitting = true;

        Time.timeScale = 1f;
        AudioListener.pause = false;
        PlayerPrefs.Save();

        StartCoroutine(QuitCoroutine());
    }

    private IEnumerator QuitCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.1f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !isPaused)
        {
            PauseGame();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !isPaused)
        {
            PauseGame();
        }
    }
}
