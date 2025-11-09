using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 설정 UI
    /// 텍스트 속도, 음량, 언어 등 게임 설정 관리
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject settingsPanel;

        [Header("Text Speed")]
        public Slider textSpeedSlider;
        public TMP_Text textSpeedValueText;

        [Header("Auto Play Speed")]
        public Slider autoSpeedSlider;
        public TMP_Text autoSpeedValueText;

        [Header("Volume")]
        public Slider bgmVolumeSlider;
        public TMP_Text bgmVolumeValueText;
        public Slider sfxVolumeSlider;
        public TMP_Text sfxVolumeValueText;
        public Slider voiceVolumeSlider;
        public TMP_Text voiceVolumeValueText;

        [Header("Display")]
        public Toggle fullscreenToggle;
        public TMP_Dropdown resolutionDropdown;

        [Header("Language")]
        public TMP_Dropdown languageDropdown;

        [Header("Buttons")]
        public Button closeButton;
        public Button applyButton;
        public Button resetButton;

        [Header("References")]
        public DialogueUI dialogueUI;

        private GameSettings currentSettings;

        void Start()
        {
            // 설정 로드
            LoadSettings();

            // 버튼 이벤트
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseSettings);

            if (applyButton != null)
                applyButton.onClick.AddListener(ApplySettings);

            if (resetButton != null)
                resetButton.onClick.AddListener(ResetToDefault);

            // 슬라이더 이벤트
            if (textSpeedSlider != null)
                textSpeedSlider.onValueChanged.AddListener(OnTextSpeedChanged);

            if (autoSpeedSlider != null)
                autoSpeedSlider.onValueChanged.AddListener(OnAutoSpeedChanged);

            if (bgmVolumeSlider != null)
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            if (voiceVolumeSlider != null)
                voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);

            // 토글 이벤트
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);

            // 드롭다운 초기화
            InitializeDropdowns();

            // UI 업데이트
            UpdateUI();

            // 초기 상태
            settingsPanel.SetActive(false);
        }

        /// <summary>
        /// 드롭다운 초기화
        /// </summary>
        void InitializeDropdowns()
        {
            // 해상도 드롭다운
            if (resolutionDropdown != null)
            {
                resolutionDropdown.ClearOptions();
                var resolutions = new System.Collections.Generic.List<string>
                {
                    "1920x1080",
                    "1280x720",
                    "1600x900",
                    "2560x1440"
                };
                resolutionDropdown.AddOptions(resolutions);
                resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }

            // 언어 드롭다운
            if (languageDropdown != null)
            {
                languageDropdown.ClearOptions();
                var languages = new System.Collections.Generic.List<string>
                {
                    "한국어",
                    "English",
                    "日本語"
                };
                languageDropdown.AddOptions(languages);
                languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            }
        }

        /// <summary>
        /// 설정 열기
        /// </summary>
        public void OpenSettings()
        {
            settingsPanel.SetActive(true);
            UpdateUI();
        }

        /// <summary>
        /// 설정 닫기
        /// </summary>
        public void CloseSettings()
        {
            settingsPanel.SetActive(false);
        }

        /// <summary>
        /// 설정 로드
        /// </summary>
        void LoadSettings()
        {
            currentSettings = GameSettings.Load();
        }

        /// <summary>
        /// 설정 저장
        /// </summary>
        void SaveSettings()
        {
            currentSettings.Save();
        }

        /// <summary>
        /// UI 업데이트
        /// </summary>
        void UpdateUI()
        {
            // 텍스트 속도
            if (textSpeedSlider != null)
            {
                textSpeedSlider.value = currentSettings.textSpeed;
                UpdateTextSpeedLabel(currentSettings.textSpeed);
            }

            // Auto 속도
            if (autoSpeedSlider != null)
            {
                autoSpeedSlider.value = currentSettings.autoPlayDelay;
                UpdateAutoSpeedLabel(currentSettings.autoPlayDelay);
            }

            // 볼륨
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.value = currentSettings.bgmVolume;
                UpdateBGMVolumeLabel(currentSettings.bgmVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = currentSettings.sfxVolume;
                UpdateSFXVolumeLabel(currentSettings.sfxVolume);
            }

            if (voiceVolumeSlider != null)
            {
                voiceVolumeSlider.value = currentSettings.voiceVolume;
                UpdateVoiceVolumeLabel(currentSettings.voiceVolume);
            }

            // 전체화면
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = currentSettings.isFullscreen;
            }

            // 해상도
            if (resolutionDropdown != null)
            {
                resolutionDropdown.value = currentSettings.resolutionIndex;
            }

            // 언어
            if (languageDropdown != null)
            {
                languageDropdown.value = (int)currentSettings.language;
            }
        }

        /// <summary>
        /// 설정 적용
        /// </summary>
        void ApplySettings()
        {
            // DialogueUI 업데이트
            if (dialogueUI != null)
            {
                dialogueUI.textSpeed = currentSettings.textSpeed;
                dialogueUI.autoPlayDelay = currentSettings.autoPlayDelay;
            }

            // 전체화면 적용
            Screen.fullScreen = currentSettings.isFullscreen;

            // 해상도 적용
            ApplyResolution(currentSettings.resolutionIndex);

            // 볼륨 적용 (AudioManager가 있다면)
            // AudioManager.Instance?.SetBGMVolume(currentSettings.bgmVolume);

            // 설정 저장
            SaveSettings();

            Debug.Log("Settings applied and saved");
        }

        /// <summary>
        /// 기본값으로 초기화
        /// </summary>
        void ResetToDefault()
        {
            currentSettings = GameSettings.GetDefault();
            UpdateUI();
            ApplySettings();
        }

        // 이벤트 핸들러
        void OnTextSpeedChanged(float value)
        {
            currentSettings.textSpeed = value;
            UpdateTextSpeedLabel(value);
        }

        void OnAutoSpeedChanged(float value)
        {
            currentSettings.autoPlayDelay = value;
            UpdateAutoSpeedLabel(value);
        }

        void OnBGMVolumeChanged(float value)
        {
            currentSettings.bgmVolume = value;
            UpdateBGMVolumeLabel(value);
            // AudioManager.Instance?.SetBGMVolume(value);
        }

        void OnSFXVolumeChanged(float value)
        {
            currentSettings.sfxVolume = value;
            UpdateSFXVolumeLabel(value);
            // AudioManager.Instance?.SetSFXVolume(value);
        }

        void OnVoiceVolumeChanged(float value)
        {
            currentSettings.voiceVolume = value;
            UpdateVoiceVolumeLabel(value);
            // AudioManager.Instance?.SetVoiceVolume(value);
        }

        void OnFullscreenToggled(bool isOn)
        {
            currentSettings.isFullscreen = isOn;
        }

        void OnResolutionChanged(int index)
        {
            currentSettings.resolutionIndex = index;
        }

        void OnLanguageChanged(int index)
        {
            currentSettings.language = (GameLanguage)index;
        }

        // 라벨 업데이트
        void UpdateTextSpeedLabel(float value)
        {
            if (textSpeedValueText != null)
            {
                // 0.01 ~ 0.1 범위를 "느림" ~ "빠름"으로 표시
                string speedText = value < 0.03f ? "Very Fast" :
                                   value < 0.05f ? "Fast" :
                                   value < 0.07f ? "Normal" :
                                   value < 0.09f ? "Slow" : "Very Slow";
                textSpeedValueText.text = speedText;
            }
        }

        void UpdateAutoSpeedLabel(float value)
        {
            if (autoSpeedValueText != null)
            {
                autoSpeedValueText.text = $"{value:F1}s";
            }
        }

        void UpdateBGMVolumeLabel(float value)
        {
            if (bgmVolumeValueText != null)
            {
                bgmVolumeValueText.text = $"{(int)(value * 100)}%";
            }
        }

        void UpdateSFXVolumeLabel(float value)
        {
            if (sfxVolumeValueText != null)
            {
                sfxVolumeValueText.text = $"{(int)(value * 100)}%";
            }
        }

        void UpdateVoiceVolumeLabel(float value)
        {
            if (voiceVolumeValueText != null)
            {
                voiceVolumeValueText.text = $"{(int)(value * 100)}%";
            }
        }

        void ApplyResolution(int index)
        {
            switch (index)
            {
                case 0: Screen.SetResolution(1920, 1080, currentSettings.isFullscreen); break;
                case 1: Screen.SetResolution(1280, 720, currentSettings.isFullscreen); break;
                case 2: Screen.SetResolution(1600, 900, currentSettings.isFullscreen); break;
                case 3: Screen.SetResolution(2560, 1440, currentSettings.isFullscreen); break;
            }
        }
    }

    /// <summary>
    /// 게임 설정 데이터
    /// </summary>
    [System.Serializable]
    public class GameSettings
    {
        public float textSpeed = 0.05f;
        public float autoPlayDelay = 2f;
        public float bgmVolume = 0.7f;
        public float sfxVolume = 0.8f;
        public float voiceVolume = 1.0f;
        public bool isFullscreen = true;
        public int resolutionIndex = 0; // 0: 1920x1080
        public GameLanguage language = GameLanguage.Korean;

        private const string SETTINGS_KEY = "GameSettings";

        /// <summary>
        /// 설정 저장
        /// </summary>
        public void Save()
        {
            string json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(SETTINGS_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 설정 로드
        /// </summary>
        public static GameSettings Load()
        {
            if (PlayerPrefs.HasKey(SETTINGS_KEY))
            {
                string json = PlayerPrefs.GetString(SETTINGS_KEY);
                return JsonUtility.FromJson<GameSettings>(json);
            }

            return GetDefault();
        }

        /// <summary>
        /// 기본 설정
        /// </summary>
        public static GameSettings GetDefault()
        {
            return new GameSettings();
        }
    }
}
