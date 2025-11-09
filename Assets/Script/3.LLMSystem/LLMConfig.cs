using UnityEngine;

/// <summary>
/// LLM API 설정을 저장하는 ScriptableObject
/// Resources 폴더에 저장하여 자동으로 로드됩니다.
/// </summary>
[CreateAssetMenu(fileName = "LLMConfig", menuName = "LLM/LLM Configuration", order = 1)]
public class LLMConfig : ScriptableObject
{
    [Header("API Settings")]
    [Tooltip("OpenAI: sk-..., Anthropic: sk-ant-...")]
    public string apiKey = "";

    [Tooltip("OpenAI: gpt-4, gpt-4o-mini, gpt-3.5-turbo / Anthropic: claude-3-sonnet-20240229")]
    public string model = "gpt-4o-mini";

    [Header("Provider Settings")]
    public LLMStoryGenerator.LLMProvider provider = LLMStoryGenerator.LLMProvider.OpenAI;

    [Range(0f, 2f)]
    public float temperature = 0.8f;

    [Range(100, 4000)]
    public int maxTokens = 1000;

    [Header("Available Resources")]
    [Tooltip("사용 가능한 캐릭터 목록")]
    public string[] availableCharacters = new string[] { "Hans", "Heilner" };

    [Tooltip("사용 가능한 배경 목록")]
    public string[] availableBackgrounds = new string[] { "riverside", "council", "black" };

    [Tooltip("사용 가능한 표정/포즈 목록")]
    public string[] availableLooks = new string[] { "Normal_Normal" };

    /// <summary>
    /// Resources 폴더에서 LLMConfig를 자동으로 로드합니다.
    /// 파일이 없으면 기본값으로 새로 생성합니다.
    /// </summary>
    public static LLMConfig LoadConfig()
    {
        LLMConfig config = Resources.Load<LLMConfig>("LLMConfig");

        if (config == null)
        {
            Debug.LogWarning("[LLMConfig] LLMConfig not found in Resources folder. Using default settings.");
            config = CreateInstance<LLMConfig>();
        }

        return config;
    }
}
