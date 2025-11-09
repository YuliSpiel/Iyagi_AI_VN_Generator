using UnityEngine;

namespace IyagiAI.AISystem
{
    /// <summary>
    /// API 키를 안전하게 저장하는 설정 파일
    /// Assets/Resources/APIConfig.asset에 저장
    /// .gitignore에 추가하여 Git 커밋 방지 필수!
    /// </summary>
    [CreateAssetMenu(fileName = "APIConfig", menuName = "Iyagi/API Config")]
    public class APIConfigData : ScriptableObject
    {
        [Header("Gemini API")]
        [Tooltip("Google AI Studio에서 발급받은 Gemini API 키")]
        public string geminiApiKey;

        [Header("NanoBanana API")]
        [Tooltip("NanoBanana 또는 대체 이미지 생성 API 키")]
        public string nanoBananaApiKey;

        [Header("ElevenLabs API (Optional)")]
        [Tooltip("ElevenLabs 오디오 생성 API 키 (선택사항)")]
        public string elevenLabsApiKey;

        /// <summary>
        /// 설정이 유효한지 확인
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(geminiApiKey))
            {
                Debug.LogError("Gemini API key is missing!");
                return false;
            }

            if (string.IsNullOrEmpty(nanoBananaApiKey))
            {
                Debug.LogError("NanoBanana API key is missing!");
                return false;
            }

            return true;
        }
    }
}
