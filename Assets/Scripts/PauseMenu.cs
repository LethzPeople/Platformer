using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pauseMenuUI;
    public Button resumeButton;
    public Button quitButton;
    public Slider volumeSlider;

    [Header("Audio")]
    public AudioMixer audioMixer; // Opcional: para mejor control de audio

    private bool isPaused = false;
    private float currentVolume;

    void Start()
    {
        // Asegurarse de que el menú esté desactivado al inicio
        pauseMenuUI.SetActive(false);

        // Ocultar cursor al inicio del juego
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Configurar botones
        resumeButton.onClick.AddListener(ResumeGame);
        quitButton.onClick.AddListener(QuitGame);

        // Configurar slider de volumen
        SetupVolumeSlider();
    }

    void Update()
    {
        // Detectar presión de la tecla Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    void SetupVolumeSlider()
    {
        // Obtener volumen actual
        if (audioMixer != null)
        {
            audioMixer.GetFloat("MasterVolume", out currentVolume);
            volumeSlider.value = Mathf.Pow(10, currentVolume / 20);
        }
        else
        {
            currentVolume = AudioListener.volume;
            volumeSlider.value = currentVolume;
        }

        // Configurar rango del slider
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;

        // Agregar listener para cambios de volumen
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Pausar el tiempo del juego
        isPaused = true;

        // Mostrar y liberar el cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // NO pausar el audio - la música seguirá sonando
        // AudioListener.pause = true; <- Esta línea se elimina
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Reanudar el tiempo del juego
        isPaused = false;

        // Ocultar y bloquear el cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // NO reanudar audio porque nunca se pausó
        // AudioListener.pause = false; <- Esta línea se elimina
    }

    public void ChangeVolume(float volume)
    {
        if (audioMixer != null)
        {
            // Convertir a decibelios para AudioMixer
            float dbValue = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            audioMixer.SetFloat("MasterVolume", dbValue);
        }
        else
        {
            // Usar AudioListener si no hay AudioMixer
            AudioListener.volume = volume;
        }

        // Guardar configuración
        PlayerPrefs.SetFloat("GameVolume", volume);
        PlayerPrefs.Save();

        // NO cerrar el menú cuando se mueve el slider
        // El menú permanece abierto
    }

    public void QuitGame()
    {
        // Guardar configuración antes de salir
        PlayerPrefs.Save();

#if UNITY_EDITOR
        // Si estamos en el editor de Unity
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Si estamos en una build
            Application.Quit();
#endif
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Pausar automáticamente si la aplicación pierde el foco
        if (pauseStatus && !isPaused)
        {
            PauseGame();
        }
        // NO despausar automáticamente
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Manejar cuando la aplicación pierde foco
        if (!hasFocus && !isPaused)
        {
            PauseGame();
        }
        // NO despausar automáticamente cuando gana foco
    }
}