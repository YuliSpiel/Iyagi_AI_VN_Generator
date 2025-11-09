using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// LLM API를 호출하여 실시간으로 스토리를 생성하는 클래스
/// OpenAI API (GPT-4) 또는 Anthropic API (Claude) 지원
/// </summary>
public class LLMStoryGenerator : MonoBehaviour
{
    public enum LLMProvider
    {
        OpenAI,
        Anthropic
    }

    [Header("Configuration")]
    [Tooltip("자동으로 Resources/LLMConfig에서 설정을 로드합니다. 비어있으면 자동 로드됩니다.")]
    public LLMConfig config;

    [Header("Runtime Override (Optional)")]
    [Tooltip("런타임에서 config 대신 이 값들을 사용하려면 설정하세요")]
    public bool useOverride = false;

    [Header("API Settings (Override Only)")]
    public LLMProvider provider = LLMProvider.OpenAI;

    [Tooltip("OpenAI: sk-..., Anthropic: sk-ant-...")]
    public string apiKey = "";

    [Tooltip("OpenAI: gpt-4, gpt-3.5-turbo / Anthropic: claude-3-sonnet-20240229")]
    public string model = "gpt-4o-mini";

    [Range(0f, 2f)]
    public float temperature = 0.8f;

    [Range(100, 4000)]
    public int maxTokens = 1000;

    [Header("Available Resources (Override Only)")]
    [Tooltip("사용 가능한 캐릭터 목록")]
    public List<string> availableCharacters = new List<string> { "Hans", "Heilner" };

    [Tooltip("사용 가능한 배경 목록")]
    public List<string> availableBackgrounds = new List<string> { "riverside", "council", "black" };

    [Tooltip("사용 가능한 표정/포즈 목록")]
    public List<string> availableLooks = new List<string> { "Normal_Normal" };

    private void Awake()
    {
        // config가 설정되지 않았으면 자동으로 로드
        if (config == null && !useOverride)
        {
            config = LLMConfig.LoadConfig();
        }
    }

    private const string OPENAI_URL = "https://api.openai.com/v1/chat/completions";
    private const string ANTHROPIC_URL = "https://api.anthropic.com/v1/messages";

    /// <summary>
    /// LLM에게 스토리 생성을 요청합니다.
    /// </summary>
    /// <param name="userPrompt">사용자가 원하는 스토리 방향</param>
    /// <param name="previousContext">이전 대화 컨텍스트 (선택사항)</param>
    /// <param name="onSuccess">성공 시 생성된 스토리 JSON 반환</param>
    /// <param name="onError">실패 시 에러 메시지 반환</param>
    public void GenerateStory(string userPrompt, string previousContext, Action<string> onSuccess, Action<string> onError)
    {
        StartCoroutine(GenerateStoryCoroutine(userPrompt, previousContext, onSuccess, onError));
    }

    private IEnumerator GenerateStoryCoroutine(string userPrompt, string previousContext, Action<string> onSuccess, Action<string> onError)
    {
        // config 또는 override 값 사용
        string currentApiKey = useOverride ? apiKey : (config != null ? config.apiKey : "");

        if (string.IsNullOrEmpty(currentApiKey))
        {
            onError?.Invoke("API Key is not set. Please configure LLMConfig in Resources folder or enable useOverride.");
            yield break;
        }

        LLMProvider currentProvider = useOverride ? provider : (config != null ? config.provider : LLMProvider.OpenAI);

        string systemPrompt = BuildSystemPrompt();
        string fullPrompt = BuildFullPrompt(userPrompt, previousContext);

        Debug.Log($"[LLMStoryGenerator] Sending request to {currentProvider}...");
        Debug.Log($"Prompt: {fullPrompt}");

        UnityWebRequest request = null;

        switch (currentProvider)
        {
            case LLMProvider.OpenAI:
                request = CreateOpenAIRequest(systemPrompt, fullPrompt);
                break;
            case LLMProvider.Anthropic:
                request = CreateAnthropicRequest(systemPrompt, fullPrompt);
                break;
        }

        if (request == null)
        {
            onError?.Invoke("Failed to create API request.");
            yield break;
        }

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            string error = $"API Error: {request.error}\nResponse: {request.downloadHandler.text}";
            Debug.LogError($"[LLMStoryGenerator] {error}");
            onError?.Invoke(error);
        }
        else
        {
            string responseText = request.downloadHandler.text;
            Debug.Log($"[LLMStoryGenerator] Response: {responseText}");

            string storyJson = ExtractStoryFromResponse(responseText, currentProvider);

            if (!string.IsNullOrEmpty(storyJson))
            {
                Debug.Log($"[LLMStoryGenerator] Generated story JSON:\n{storyJson}");
                onSuccess?.Invoke(storyJson);
            }
            else
            {
                onError?.Invoke("Failed to extract story from LLM response.");
            }
        }

        request.Dispose();
    }

    private string BuildSystemPrompt()
    {
        // config 또는 override 값 사용
        List<string> currentCharacters = useOverride ? availableCharacters :
            (config != null && config.availableCharacters.Length > 0 ? new List<string>(config.availableCharacters) : availableCharacters);

        List<string> currentBackgrounds = useOverride ? availableBackgrounds :
            (config != null && config.availableBackgrounds.Length > 0 ? new List<string>(config.availableBackgrounds) : availableBackgrounds);

        List<string> currentLooks = useOverride ? availableLooks :
            (config != null && config.availableLooks.Length > 0 ? new List<string>(config.availableLooks) : availableLooks);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("You are a visual novel story writer. You will generate story scenes that can be rendered as a visual novel.");
        sb.AppendLine();
        sb.AppendLine("Available resources:");
        sb.AppendLine($"- Characters: {string.Join(", ", currentCharacters)}");
        sb.AppendLine($"- Backgrounds: {string.Join(", ", currentBackgrounds)}");
        sb.AppendLine($"- Character looks: {string.Join(", ", currentLooks)}");
        sb.AppendLine();
        sb.AppendLine("Output ONLY a valid JSON array of dialogue entries. Each entry should have:");
        sb.AppendLine("- char1Name: character name (optional)");
        sb.AppendLine("- char1Look: character look/pose (default: Normal_Normal)");
        sb.AppendLine("- char1Pos: Left, Center, or Right (optional)");
        sb.AppendLine("- char1Size: Small, Medium, or Large (default: Medium)");
        sb.AppendLine("- char2Name: second character (optional)");
        sb.AppendLine("- char2Look, char2Pos, char2Size: same as char1");
        sb.AppendLine("- bg: background name (optional)");
        sb.AppendLine("- nameTag: speaker name");
        sb.AppendLine("- lineEng: dialogue in English");
        sb.AppendLine("- lineKr: dialogue in Korean");
        sb.AppendLine("- choices: array of choice objects with textEng, textKr (optional)");
        sb.AppendLine();
        sb.AppendLine("Example output:");
        sb.AppendLine("[{\"char1Name\":\"Hans\",\"char1Pos\":\"Center\",\"bg\":\"riverside\",\"nameTag\":\"Hans\",\"lineEng\":\"What a beautiful day.\",\"lineKr\":\"정말 아름다운 날이야.\"}]");
        sb.AppendLine();
        sb.AppendLine("Generate 3-8 dialogue entries that create an engaging short story. Use ONLY the available characters and backgrounds.");

        return sb.ToString();
    }

    private string BuildFullPrompt(string userPrompt, string previousContext)
    {
        StringBuilder sb = new StringBuilder();

        if (!string.IsNullOrEmpty(previousContext))
        {
            sb.AppendLine("Previous story context:");
            sb.AppendLine(previousContext);
            sb.AppendLine();
        }

        sb.AppendLine("User request:");
        sb.AppendLine(userPrompt);
        sb.AppendLine();
        sb.AppendLine("Generate the story as a JSON array:");

        return sb.ToString();
    }

    private UnityWebRequest CreateOpenAIRequest(string systemPrompt, string userPrompt)
    {
        // config 또는 override 값 사용
        string currentApiKey = useOverride ? apiKey : (config != null ? config.apiKey : "");
        string currentModel = useOverride ? model : (config != null ? config.model : "gpt-4o-mini");
        float currentTemp = useOverride ? temperature : (config != null ? config.temperature : 0.8f);
        int currentMaxTokens = useOverride ? maxTokens : (config != null ? config.maxTokens : 1000);

        string jsonData = $@"{{
            ""model"": ""{currentModel}"",
            ""messages"": [
                {{""role"": ""system"", ""content"": {EscapeJson(systemPrompt)}}},
                {{""role"": ""user"", ""content"": {EscapeJson(userPrompt)}}}
            ],
            ""temperature"": {currentTemp},
            ""max_tokens"": {currentMaxTokens}
        }}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(OPENAI_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {currentApiKey}");

        return request;
    }

    private UnityWebRequest CreateAnthropicRequest(string systemPrompt, string userPrompt)
    {
        // config 또는 override 값 사용
        string currentApiKey = useOverride ? apiKey : (config != null ? config.apiKey : "");
        string currentModel = useOverride ? model : (config != null ? config.model : "claude-3-sonnet-20240229");
        float currentTemp = useOverride ? temperature : (config != null ? config.temperature : 0.8f);
        int currentMaxTokens = useOverride ? maxTokens : (config != null ? config.maxTokens : 1000);

        string jsonData = $@"{{
            ""model"": ""{currentModel}"",
            ""max_tokens"": {currentMaxTokens},
            ""system"": {EscapeJson(systemPrompt)},
            ""messages"": [
                {{""role"": ""user"", ""content"": {EscapeJson(userPrompt)}}}
            ],
            ""temperature"": {currentTemp}
        }}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(ANTHROPIC_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-api-key", currentApiKey);
        request.SetRequestHeader("anthropic-version", "2023-06-01");

        return request;
    }

    private string ExtractStoryFromResponse(string responseJson, LLMProvider currentProvider)
    {
        try
        {
            // OpenAI 응답 파싱
            if (currentProvider == LLMProvider.OpenAI)
            {
                // "content": "..." 부분 추출
                int contentStart = responseJson.IndexOf("\"content\":");
                if (contentStart == -1) return null;

                contentStart = responseJson.IndexOf("\"", contentStart + 10) + 1;
                int contentEnd = responseJson.IndexOf("\"", contentStart);

                while (contentEnd > 0 && responseJson[contentEnd - 1] == '\\')
                {
                    contentEnd = responseJson.IndexOf("\"", contentEnd + 1);
                }

                if (contentEnd == -1) return null;

                string content = responseJson.Substring(contentStart, contentEnd - contentStart);
                content = UnescapeJson(content);

                // JSON 배열 추출
                return ExtractJsonArray(content);
            }
            // Anthropic 응답 파싱
            else if (currentProvider == LLMProvider.Anthropic)
            {
                // "text": "..." 부분 추출
                int textStart = responseJson.IndexOf("\"text\":");
                if (textStart == -1) return null;

                textStart = responseJson.IndexOf("\"", textStart + 7) + 1;
                int textEnd = responseJson.IndexOf("\"", textStart);

                while (textEnd > 0 && responseJson[textEnd - 1] == '\\')
                {
                    textEnd = responseJson.IndexOf("\"", textEnd + 1);
                }

                if (textEnd == -1) return null;

                string text = responseJson.Substring(textStart, textEnd - textStart);
                text = UnescapeJson(text);

                return ExtractJsonArray(text);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LLMStoryGenerator] Failed to extract story: {e.Message}");
        }

        return null;
    }

    private string ExtractJsonArray(string text)
    {
        // JSON 배열 찾기 [...]
        int arrayStart = text.IndexOf('[');
        int arrayEnd = text.LastIndexOf(']');

        if (arrayStart != -1 && arrayEnd != -1 && arrayEnd > arrayStart)
        {
            return text.Substring(arrayStart, arrayEnd - arrayStart + 1);
        }

        return null;
    }

    private string EscapeJson(string text)
    {
        if (string.IsNullOrEmpty(text)) return "\"\"";

        text = text.Replace("\\", "\\\\")
                   .Replace("\"", "\\\"")
                   .Replace("\n", "\\n")
                   .Replace("\r", "\\r")
                   .Replace("\t", "\\t");

        return $"\"{text}\"";
    }

    private string UnescapeJson(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";

        return text.Replace("\\\"", "\"")
                   .Replace("\\n", "\n")
                   .Replace("\\r", "\r")
                   .Replace("\\t", "\t")
                   .Replace("\\\\", "\\");
    }
}

[Serializable]
public class LLMStoryResponse
{
    public List<LLMDialogueEntry> dialogues;
}

[Serializable]
public class LLMDialogueEntry
{
    public string char1Name;
    public string char1Look;
    public string char1Pos;
    public string char1Size;
    public string char2Name;
    public string char2Look;
    public string char2Pos;
    public string char2Size;
    public string bg;
    public string nameTag;
    public string lineEng;
    public string lineKr;
    public List<LLMChoice> choices;
}

[Serializable]
public class LLMChoice
{
    public string textEng;
    public string textKr;
}
