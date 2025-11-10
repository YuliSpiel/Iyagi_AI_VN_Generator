using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace IyagiAI.AISystem
{
    /// <summary>
    /// ElevenLabs API 클라이언트
    /// BGM, SFX 오디오 생성 담당
    /// Rate Limit 자동 재시도 기능 포함
    /// </summary>
    public class ElevenLabsClient : MonoBehaviour
    {
        private string apiKey;
        private const string API_URL_SOUND_GENERATION = "https://api.elevenlabs.io/v1/sound-generation";

        [Header("Rate Limit Settings")]
        [SerializeField] private int maxRetryAttempts = 3;
        [SerializeField] private float retryDelaySeconds = 60f;

        [Header("Audio Settings")]
        [SerializeField] private float defaultDurationSeconds = 60f;
        [SerializeField] private float defaultPromptInfluence = 0.3f;

        /// <summary>
        /// API 키 초기화
        /// </summary>
        public void Initialize(string key)
        {
            this.apiKey = key;
        }

        /// <summary>
        /// BGM 생성 (Rate Limit 자동 재시도 포함)
        /// </summary>
        /// <param name="description">음악 설명 (예: "epic battle music with orchestral drums")</param>
        /// <param name="durationSeconds">음악 길이 (초)</param>
        /// <param name="onSuccess">성공 콜백 (AudioClip)</param>
        /// <param name="onError">실패 콜백 (에러 메시지)</param>
        public IEnumerator GenerateBGM(
            string description,
            float durationSeconds,
            System.Action<AudioClip> onSuccess,
            System.Action<string> onError)
        {
            yield return GenerateSound(description, durationSeconds, onSuccess, onError, 0);
        }

        /// <summary>
        /// SFX 생성 (Rate Limit 자동 재시도 포함)
        /// </summary>
        /// <param name="description">효과음 설명 (예: "sword clashing sound")</param>
        /// <param name="durationSeconds">효과음 길이 (초, 기본 5초)</param>
        /// <param name="onSuccess">성공 콜백 (AudioClip)</param>
        /// <param name="onError">실패 콜백 (에러 메시지)</param>
        public IEnumerator GenerateSFX(
            string description,
            float durationSeconds = 5f,
            System.Action<AudioClip> onSuccess = null,
            System.Action<string> onError = null)
        {
            yield return GenerateSound(description, durationSeconds, onSuccess, onError, 0);
        }

        /// <summary>
        /// Rate Limit 재시도 로직이 포함된 내부 메서드
        /// </summary>
        private IEnumerator GenerateSound(
            string description,
            float durationSeconds,
            System.Action<AudioClip> onSuccess,
            System.Action<string> onError,
            int attemptCount)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                onError?.Invoke("API key not initialized");
                yield break;
            }

            // ElevenLabs API 요청 형식
            var requestBody = new SoundGenerationRequest
            {
                text = description,
                duration_seconds = durationSeconds > 0 ? durationSeconds : defaultDurationSeconds,
                prompt_influence = defaultPromptInfluence
            };

            string json = JsonUtility.ToJson(requestBody);

            UnityWebRequest request = new UnityWebRequest(API_URL_SOUND_GENERATION, "POST");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // MP3 바이너리 데이터를 AudioClip으로 변환
                    byte[] audioData = request.downloadHandler.data;

                    // Unity는 MP3를 직접 로드할 수 없으므로, 파일로 저장 후 로드
                    string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, $"temp_audio_{System.Guid.NewGuid()}.mp3");
                    System.IO.File.WriteAllBytes(tempPath, audioData);

                    // UnityWebRequest를 사용하여 MP3 로드
                    UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.MPEG);
                    yield return audioRequest.SendWebRequest();

                    if (audioRequest.result == UnityWebRequest.Result.Success)
                    {
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(audioRequest);
                        clip.name = description; // 설명을 클립 이름으로 설정

                        onSuccess?.Invoke(clip);

                        // 임시 파일 삭제
                        if (System.IO.File.Exists(tempPath))
                        {
                            System.IO.File.Delete(tempPath);
                        }
                    }
                    else
                    {
                        onError?.Invoke($"Audio loading error: {audioRequest.error}");
                    }
                }
                catch (System.Exception e)
                {
                    onError?.Invoke($"Audio conversion error: {e.Message}");
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
                      errorResponse.Contains("quota") ||
                      errorResponse.Contains("too many requests"))))
                {
                    isRateLimitError = true;
                }

                // Rate Limit 에러이고 재시도 가능한 경우
                if (isRateLimitError && attemptCount < maxRetryAttempts)
                {
                    Debug.LogWarning($"[ElevenLabsClient] Rate limit reached. Retry {attemptCount + 1}/{maxRetryAttempts} after {retryDelaySeconds}s...");
                    yield return new WaitForSeconds(retryDelaySeconds);

                    // 재시도
                    yield return GenerateSound(description, durationSeconds, onSuccess, onError, attemptCount + 1);
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
        private class SoundGenerationRequest
        {
            public string text;
            public float duration_seconds;
            public float prompt_influence;
        }
    }
}
