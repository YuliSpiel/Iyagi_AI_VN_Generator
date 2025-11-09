using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace IyagiAI.AISystem
{
    /// <summary>
    /// NanoBanana (또는 대체) 이미지 생성 API 클라이언트
    /// 캐릭터 스탠딩, 배경, CG 생성 담당
    /// </summary>
    public class NanoBananaClient : MonoBehaviour
    {
        private string apiKey;
        private const string API_URL_GENERATE = "https://api.nanobanana.ai/v1/generate";
        private const string API_URL_WITH_REFERENCE = "https://api.nanobanana.ai/v1/generate_with_reference";

        /// <summary>
        /// API 키 초기화
        /// </summary>
        public void Initialize(string key)
        {
            this.apiKey = key;
        }

        /// <summary>
        /// 기본 이미지 생성 (스탠딩, 배경 등)
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
            if (string.IsNullOrEmpty(apiKey))
            {
                onError?.Invoke("API key not initialized");
                yield break;
            }

            var requestBody = new ImageGenRequest
            {
                prompt = prompt,
                seed = seed ?? Random.Range(1000, 99999)
            };

            string json = JsonUtility.ToJson(requestBody);
            UnityWebRequest request = new UnityWebRequest(API_URL_GENERATE, "POST");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // JSON 파싱 (try-catch는 yield 없이)
                ImageGenResponse response = null;
                try
                {
                    response = JsonUtility.FromJson<ImageGenResponse>(request.downloadHandler.text);
                }
                catch (System.Exception e)
                {
                    onError?.Invoke($"JSON parsing error: {e.Message}");
                    yield break;
                }

                if (response == null || string.IsNullOrEmpty(response.image_url))
                {
                    onError?.Invoke("Invalid response: no image URL");
                    yield break;
                }

                // 이미지 다운로드 (try-catch 밖에서)
                UnityWebRequest imgRequest = UnityWebRequestTexture.GetTexture(response.image_url);
                yield return imgRequest.SendWebRequest();

                if (imgRequest.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(imgRequest);
                    onSuccess?.Invoke(texture, response.seed);
                }
                else
                {
                    onError?.Invoke($"Image download failed: {imgRequest.error}");
                }
            }
            else
            {
                onError?.Invoke($"API Error: {request.error}\n{request.downloadHandler.text}");
            }
        }

        /// <summary>
        /// 레퍼런스 이미지를 사용한 이미지 생성 (CG 전용)
        /// </summary>
        /// <param name="prompt">이미지 생성 프롬프트</param>
        /// <param name="referenceImages">레퍼런스 이미지 목록 (캐릭터 얼굴)</param>
        /// <param name="width">생성 이미지 너비</param>
        /// <param name="height">생성 이미지 높이</param>
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
            if (string.IsNullOrEmpty(apiKey))
            {
                onError?.Invoke("API key not initialized");
                yield break;
            }

            // Multipart form data 생성
            WWWForm form = new WWWForm();
            form.AddField("prompt", prompt);
            form.AddField("width", width.ToString());
            form.AddField("height", height.ToString());

            // 레퍼런스 이미지 추가
            for (int i = 0; i < referenceImages.Count; i++)
            {
                byte[] imageBytes = referenceImages[i].EncodeToPNG();
                form.AddBinaryData($"reference_image_{i}", imageBytes, $"ref_{i}.png", "image/png");
            }

            UnityWebRequest request = UnityWebRequest.Post(API_URL_WITH_REFERENCE, form);
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // JSON 파싱 (try-catch는 yield 없이)
                ImageGenResponse response = null;
                try
                {
                    response = JsonUtility.FromJson<ImageGenResponse>(request.downloadHandler.text);
                }
                catch (System.Exception e)
                {
                    onError?.Invoke($"JSON parsing error: {e.Message}");
                    yield break;
                }

                if (response == null || string.IsNullOrEmpty(response.image_url))
                {
                    onError?.Invoke("Invalid response: no image URL");
                    yield break;
                }

                // 이미지 다운로드 (try-catch 밖에서)
                UnityWebRequest imgRequest = UnityWebRequestTexture.GetTexture(response.image_url);
                yield return imgRequest.SendWebRequest();

                if (imgRequest.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(imgRequest);
                    onSuccess?.Invoke(texture);
                }
                else
                {
                    onError?.Invoke($"Image download failed: {imgRequest.error}");
                }
            }
            else
            {
                onError?.Invoke($"API Error: {request.error}\n{request.downloadHandler.text}");
            }
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

        // ===== JSON 스키마 =====

        [System.Serializable]
        private class ImageGenRequest
        {
            public string prompt;
            public int seed;
        }

        [System.Serializable]
        private class ImageGenResponse
        {
            public string image_url;
            public int seed;
        }
    }
}
