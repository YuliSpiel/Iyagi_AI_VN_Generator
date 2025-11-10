using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace IyagiAI.AISystem
{
    /// <summary>
    /// Gemini API 클라이언트
    /// 스토리 생성, 대화 생성 등 텍스트 생성 담당
    /// Rate Limit 자동 재시도 기능 포함
    /// </summary>
    public class GeminiClient : MonoBehaviour
    {
        private string apiKey;
        private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        [Header("Rate Limit Settings")]
        [SerializeField] private int maxRetryAttempts = 3;
        [SerializeField] private float retryDelaySeconds = 60f;

        /// <summary>
        /// API 키 초기화
        /// </summary>
        public void Initialize(string key)
        {
            this.apiKey = key;
        }

        /// <summary>
        /// 텍스트 생성 요청 (Rate Limit 자동 재시도 포함)
        /// </summary>
        /// <param name="prompt">프롬프트</param>
        /// <param name="onSuccess">성공 콜백 (생성된 텍스트)</param>
        /// <param name="onError">실패 콜백 (에러 메시지)</param>
        public IEnumerator GenerateContent(string prompt, System.Action<string> onSuccess, System.Action<string> onError)
        {
            yield return GenerateContentWithRetry(prompt, onSuccess, onError, 0);
        }

        /// <summary>
        /// Rate Limit 재시도 로직이 포함된 내부 메서드
        /// </summary>
        private IEnumerator GenerateContentWithRetry(string prompt, System.Action<string> onSuccess, System.Action<string> onError, int attemptCount)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                onError?.Invoke("API key not initialized");
                yield break;
            }

            var requestBody = new GeminiRequest
            {
                contents = new GeminiContent[]
                {
                    new GeminiContent
                    {
                        parts = new GeminiPart[]
                        {
                            new GeminiPart { text = prompt }
                        }
                    }
                },
                generationConfig = new GenerationConfig
                {
                    temperature = 0.9f,
                    topK = 40,
                    topP = 0.95f,
                    maxOutputTokens = 8192
                }
            };

            string json = JsonUtility.ToJson(requestBody);
            string url = $"{API_URL}?key={apiKey}";

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);

                    if (response.candidates != null && response.candidates.Length > 0)
                    {
                        string generatedText = response.candidates[0].content.parts[0].text;
                        onSuccess?.Invoke(generatedText);
                    }
                    else
                    {
                        onError?.Invoke("No candidates in response");
                    }
                }
                catch (System.Exception e)
                {
                    onError?.Invoke($"JSON parsing error: {e.Message}");
                }
            }
            else
            {
                // Rate Limit 에러 감지
                bool isRateLimitError = false;
                string errorResponse = request.downloadHandler.text;

                // HTTP 429 또는 에러 메시지에 "rate limit" 포함 시
                if (request.responseCode == 429 ||
                    (!string.IsNullOrEmpty(errorResponse) &&
                     (errorResponse.Contains("rate limit") ||
                      errorResponse.Contains("RESOURCE_EXHAUSTED") ||
                      errorResponse.Contains("quota"))))
                {
                    isRateLimitError = true;
                }

                // Rate Limit 에러이고 재시도 가능한 경우
                if (isRateLimitError && attemptCount < maxRetryAttempts)
                {
                    Debug.LogWarning($"[GeminiClient] Rate limit reached. Retry {attemptCount + 1}/{maxRetryAttempts} after {retryDelaySeconds}s...");
                    yield return new WaitForSeconds(retryDelaySeconds);

                    // 재시도
                    yield return GenerateContentWithRetry(prompt, onSuccess, onError, attemptCount + 1);
                }
                else
                {
                    // 재시도 불가능하거나 다른 에러
                    string errorMsg = isRateLimitError
                        ? $"Rate limit exceeded after {maxRetryAttempts} attempts"
                        : $"API Error: {request.error}\n{errorResponse}";

                    onError?.Invoke(errorMsg);
                }
            }
        }

        // ===== JSON 스키마 =====

        [System.Serializable]
        private class GeminiRequest
        {
            public GeminiContent[] contents;
            public GenerationConfig generationConfig;
        }

        [System.Serializable]
        private class GeminiContent
        {
            public GeminiPart[] parts;
        }

        [System.Serializable]
        private class GeminiPart
        {
            public string text;
        }

        [System.Serializable]
        private class GenerationConfig
        {
            public float temperature;
            public int topK;
            public float topP;
            public int maxOutputTokens;
        }

        [System.Serializable]
        private class GeminiResponse
        {
            public Candidate[] candidates;
        }

        [System.Serializable]
        private class Candidate
        {
            public GeminiContent content;
        }
    }
}
