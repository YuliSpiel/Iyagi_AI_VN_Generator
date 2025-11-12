using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace IyagiAI.AISystem
{
    /// <summary>
    /// Gemini 2.5 Flash Image API 클라이언트
    /// 캐릭터 스탠딩, 배경, CG 생성 담당
    /// Rate Limit 자동 재시도 기능 포함
    /// </summary>
    public class NanoBananaClient : MonoBehaviour
    {
        private string apiKey;
        private const string API_URL_GENERATE = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-image:generateContent";

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
        /// 기본 이미지 생성 (스탠딩, 배경 등) - Rate Limit 자동 재시도 포함
        /// </summary>
        /// <param name="prompt">이미지 생성 프롬프트</param>
        /// <param name="seed">시드 값 (null이면 랜덤)</param>
        /// <param name="onSuccess">성공 콜백 (Texture2D, 사용된 시드)</param>
        /// <param name="onError">실패 콜백 (에러 메시지)</param>
        public IEnumerator GenerateImage(
            string prompt,
            int? seed,
            System.Action<Texture2D, int> onSuccess,
            System.Action<string> onError)
        {
            int usedSeed = seed ?? Random.Range(1000, 99999);
            yield return GenerateImageWithRetry(prompt, usedSeed, onSuccess, onError, 0);
        }

        /// <summary>
        /// Rate Limit 재시도 로직이 포함된 내부 메서드
        /// </summary>
        private IEnumerator GenerateImageWithRetry(
            string prompt,
            int usedSeed,
            System.Action<Texture2D, int> onSuccess,
            System.Action<string> onError,
            int attemptCount)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                onError?.Invoke("API key not initialized");
                yield break;
            }

            // Gemini 2.5 Flash Image API 요청 형식
            var requestBody = new GeminiImageRequest
            {
                contents = new GeminiImageContent[]
                {
                    new GeminiImageContent
                    {
                        parts = new GeminiImageRequestPart[]
                        {
                            new GeminiImageRequestPart { text = prompt }
                        }
                    }
                }
                // generationConfig는 이미지 생성에서 지원하지 않음
            };

            string json = JsonUtility.ToJson(requestBody);
            string url = $"{API_URL_GENERATE}?key={apiKey}";

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 응답 디버깅
                Debug.Log("=== Gemini Image API Response ===");
                Debug.Log($"Response length: {request.downloadHandler.text.Length}");
                Debug.Log($"First 500 chars: {request.downloadHandler.text.Substring(0, Mathf.Min(500, request.downloadHandler.text.Length))}");

                // JSON 파싱
                GeminiImageResponse response = null;
                try
                {
                    response = JsonUtility.FromJson<GeminiImageResponse>(request.downloadHandler.text);
                }
                catch (System.Exception e)
                {
                    onError?.Invoke($"JSON parsing error: {e.Message}\nResponse: {request.downloadHandler.text}");
                    yield break;
                }

                if (response == null || response.candidates == null || response.candidates.Length == 0)
                {
                    onError?.Invoke($"Invalid response: no candidates\nFull response: {request.downloadHandler.text}");
                    yield break;
                }

                Debug.Log($"Candidates count: {response.candidates.Length}");

                // 첫 번째 candidate의 parts에서 inline_data 추출
                var candidate = response.candidates[0];
                if (candidate.content == null || candidate.content.parts == null || candidate.content.parts.Length == 0)
                {
                    onError?.Invoke($"Invalid response: no content parts\nFull response: {request.downloadHandler.text}");
                    yield break;
                }

                Debug.Log($"Parts count: {candidate.content.parts.Length}");

                // 모든 parts를 순회하여 inlineData가 있는 part 찾기
                GeminiImageResponsePart imagePart = null;
                foreach (var p in candidate.content.parts)
                {
                    if (p.inlineData != null && !string.IsNullOrEmpty(p.inlineData.data))
                    {
                        imagePart = p;
                        Debug.Log($"Found image in part with mimeType: {p.inlineData.mimeType}");
                        Debug.Log($"Image data length: {p.inlineData.data.Length}");
                        break;
                    }
                    else if (!string.IsNullOrEmpty(p.text))
                    {
                        Debug.Log($"Part has text: {p.text.Substring(0, Mathf.Min(100, p.text.Length))}...");
                    }
                }

                if (imagePart == null || imagePart.inlineData == null || string.IsNullOrEmpty(imagePart.inlineData.data))
                {
                    onError?.Invoke($"Invalid response: no inlineData found in any part\nFull response: {request.downloadHandler.text}");
                    yield break;
                }

                // Base64 이미지 디코딩
                try
                {
                    byte[] imageBytes = System.Convert.FromBase64String(imagePart.inlineData.data);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(imageBytes);
                    onSuccess?.Invoke(texture, usedSeed);
                }
                catch (System.Exception e)
                {
                    onError?.Invoke($"Image decoding error: {e.Message}");
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
                    Debug.LogWarning($"[NanoBananaClient] Rate limit reached. Retry {attemptCount + 1}/{maxRetryAttempts} after {retryDelaySeconds}s...");
                    yield return new WaitForSeconds(retryDelaySeconds);

                    // 재시도
                    yield return GenerateImageWithRetry(prompt, usedSeed, onSuccess, onError, attemptCount + 1);
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

        /// <summary>
        /// 레퍼런스 이미지를 사용한 이미지 생성 (CG 전용)
        /// NOTE: Imagen 3는 레퍼런스 이미지를 직접 지원하지 않으므로
        /// 프롬프트에 캐릭터 설명을 추가하는 방식으로 대체
        /// </summary>
        /// <param name="prompt">이미지 생성 프롬프트</param>
        /// <param name="referenceImages">레퍼런스 이미지 목록 (사용하지 않음)</param>
        /// <param name="width">생성 이미지 너비 (사용하지 않음)</param>
        /// <param name="height">생성 이미지 높이 (사용하지 않음)</param>
        /// <param name="onSuccess">성공 콜백 (Texture2D)</param>
        /// <param name="onError">실패 콜백 (에러 메시지)</param>
        public IEnumerator GenerateImageWithReferences(
            string prompt,
            List<Texture2D> referenceImages,
            int width,
            int height,
            System.Action<Texture2D> onSuccess,
            System.Action<string> onError)
        {
            // Imagen 3는 레퍼런스 이미지를 직접 지원하지 않으므로
            // 프롬프트 기반 생성만 사용
            // 캐릭터 일관성은 prompt에 상세한 설명을 포함하여 유지

            yield return GenerateImage(
                prompt,
                null, // 랜덤 시드
                (texture, seed) => onSuccess?.Invoke(texture),
                onError
            );
        }

        /// <summary>
        /// Sprite를 Texture2D로 변환 (레퍼런스 이미지 전송용)
        /// </summary>
        public static Texture2D SpriteToTexture2D(Sprite sprite)
        {
            if (sprite.rect.width != sprite.texture.width)
            {
                // Sprite가 아틀라스의 일부인 경우
                Texture2D newTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] pixels = sprite.texture.GetPixels(
                    (int)sprite.rect.x,
                    (int)sprite.rect.y,
                    (int)sprite.rect.width,
                    (int)sprite.rect.height
                );
                newTexture.SetPixels(pixels);
                newTexture.Apply();
                return newTexture;
            }
            else
            {
                // 단독 텍스처인 경우
                return sprite.texture;
            }
        }

        // ===== JSON 스키마 (Gemini 2.5 Flash Image API) =====

        [System.Serializable]
        private class GeminiImageRequest
        {
            public GeminiImageContent[] contents;
        }

        [System.Serializable]
        private class GeminiImageContent
        {
            public GeminiImageRequestPart[] parts;
        }

        // 요청용 Part (text만 포함)
        [System.Serializable]
        private class GeminiImageRequestPart
        {
            public string text;
        }

        // 응답용 Part (inlineData 포함 가능)
        [System.Serializable]
        private class GeminiImageResponsePart
        {
            public string text;
            public InlineData inlineData;
        }

        [System.Serializable]
        private class InlineData
        {
            public string mimeType;
            public string data; // Base64 encoded
        }

        [System.Serializable]
        private class GeminiImageResponse
        {
            public GeminiImageCandidate[] candidates;
        }

        [System.Serializable]
        private class GeminiImageCandidate
        {
            public GeminiImageResponseContent content;
        }

        [System.Serializable]
        private class GeminiImageResponseContent
        {
            public GeminiImageResponsePart[] parts;
        }
    }
}
