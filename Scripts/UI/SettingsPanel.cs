using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    [Header("설정 UI")]
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;
    
    public Toggle autoSaveToggle;
    public Toggle autoBattleToggle;
    public Toggle fullscreenToggle;
    
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI sfxVolumeText;
    public TextMeshProUGUI musicVolumeText;
    
    [Header("설정값")]
    public float masterVolume = 1f;
    public float sfxVolume = 1f;
    public float musicVolume = 1f;
    public bool autoSaveEnabled = true;
    public bool autoBattleEnabled = true;
    public bool fullscreenEnabled = true;
    
    private void Start()
    {
        LoadSettings();
        SetupUI();
    }
    
    private void SetupUI()
    {
        // 슬라이더 이벤트
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        // 토글 이벤트
        if (autoSaveToggle != null)
        {
            autoSaveToggle.isOn = autoSaveEnabled;
            autoSaveToggle.onValueChanged.AddListener(OnAutoSaveToggle);
        }
        
        if (autoBattleToggle != null)
        {
            autoBattleToggle.isOn = autoBattleEnabled;
            autoBattleToggle.onValueChanged.AddListener(OnAutoBattleToggle);
        }
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = fullscreenEnabled;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);
        }
        
        UpdateVolumeTexts();
    }
    
    private void OnMasterVolumeChanged(float value)
    {
        masterVolume = value;
        UpdateVolumeTexts();
        ApplyAudioSettings();
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        sfxVolume = value;
        UpdateVolumeTexts();
        ApplyAudioSettings();
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        musicVolume = value;
        UpdateVolumeTexts();
        ApplyAudioSettings();
    }
    
    private void OnAutoSaveToggle(bool value)
    {
        autoSaveEnabled = value;
        Debug.Log($"자동 저장: {value}");
    }
    
    private void OnAutoBattleToggle(bool value)
    {
        autoBattleEnabled = value;
        Debug.Log($"자동 전투: {value}");
    }
    
    private void OnFullscreenToggle(bool value)
    {
        fullscreenEnabled = value;
        Screen.fullScreen = value;
        Debug.Log($"전체화면: {value}");
    }
    
    private void UpdateVolumeTexts()
    {
        if (masterVolumeText != null)
            masterVolumeText.text = $"마스터 볼륨: {masterVolume:P0}";
        
        if (sfxVolumeText != null)
            sfxVolumeText.text = $"효과음: {sfxVolume:P0}";
        
        if (musicVolumeText != null)
            musicVolumeText.text = $"음악: {musicVolume:P0}";
    }
    
    private void ApplyAudioSettings()
    {
        // 오디오 설정 적용
        AudioListener.volume = masterVolume;
        // SFX와 음악은 별도 오디오 매니저에서 처리
    }
    
    private void LoadSettings()
    {
        // PlayerPrefs에서 설정 로드
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        autoSaveEnabled = PlayerPrefs.GetInt("AutoSave", 1) == 1;
        autoBattleEnabled = PlayerPrefs.GetInt("AutoBattle", 1) == 1;
        fullscreenEnabled = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    }
    
    public void SaveSettings()
    {
        // PlayerPrefs에 설정 저장
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetInt("AutoSave", autoSaveEnabled ? 1 : 0);
        PlayerPrefs.SetInt("AutoBattle", autoBattleEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Fullscreen", fullscreenEnabled ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log("설정이 저장되었습니다.");
    }
    
    public void ResetSettings()
    {
        // 기본 설정으로 리셋
        masterVolume = 1f;
        sfxVolume = 1f;
        musicVolume = 1f;
        autoSaveEnabled = true;
        autoBattleEnabled = true;
        fullscreenEnabled = true;
        
        SetupUI();
        Debug.Log("설정이 초기화되었습니다.");
    }
}
