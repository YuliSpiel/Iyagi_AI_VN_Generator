using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IyagiAI.AISystem;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// 캐릭터 얼굴 프리뷰 생성 및 히스토리 탐색
    /// 여러 시드를 시도하며 마음에 드는 얼굴을 선택
    /// </summary>
    public class CharacterFaceGenerator : MonoBehaviour
    {
        [Header("Preview")]
        public Image previewImage; // 얼굴 프리뷰를 표시할 Image

        [Header("History")]
        public List<Sprite> previewHistory = new List<Sprite>(); // 생성된 얼굴 히스토리
        public List<int> seedHistory = new List<int>(); // 대응되는 시드 히스토리
        public int currentIndex = -1; // 현재 보고 있는 인덱스

        private bool isGenerating = false;

        /// <summary>
        /// 새로운 얼굴 생성
        /// </summary>
        public IEnumerator GenerateNewFace(
            string appearanceDescription,
            NanoBananaClient client,
            System.Action onComplete)
        {
            if (isGenerating)
            {
                Debug.LogWarning("Already generating...");
                yield break;
            }

            isGenerating = true;

            // 얼굴 프리뷰 프롬프트 (작은 크기, 빠른 생성)
            string prompt = $@"A portrait of a character for a visual novel.
High-quality anime illustration style with clean outlines and soft gradient shading.
Large expressive eyes, natural lighting, smooth skin tone.
Close-up face portrait, shoulders visible.
{appearanceDescription}
Background: soft gradient or solid color (no complex scenery).
Resolution: 512×512.";

            bool completed = false;
            Texture2D result = null;
            int usedSeed = 0;

            yield return client.GenerateImage(
                prompt,
                null, // 랜덤 시드
                (texture, seed) => {
                    result = texture;
                    usedSeed = seed;
                    completed = true;
                },
                (error) => {
                    Debug.LogError($"Face generation failed: {error}");
                    completed = true;
                }
            );

            yield return new WaitUntil(() => completed);

            if (result != null)
            {
                // Sprite 생성
                Sprite sprite = Sprite.Create(
                    result,
                    new Rect(0, 0, result.width, result.height),
                    new Vector2(0.5f, 0.5f)
                );

                // 히스토리에 추가
                previewHistory.Add(sprite);
                seedHistory.Add(usedSeed);
                currentIndex = previewHistory.Count - 1;

                // 프리뷰 표시
                previewImage.sprite = sprite;

                Debug.Log($"Face generated with seed: {usedSeed}");
            }

            isGenerating = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// 이전 얼굴로 이동
        /// </summary>
        public void ShowPrevious()
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                previewImage.sprite = previewHistory[currentIndex];
            }
        }

        /// <summary>
        /// 다음 얼굴로 이동
        /// </summary>
        public void ShowNext()
        {
            if (currentIndex < previewHistory.Count - 1)
            {
                currentIndex++;
                previewImage.sprite = previewHistory[currentIndex];
            }
        }

        /// <summary>
        /// 현재 프리뷰 가져오기
        /// </summary>
        public Sprite GetCurrentPreview()
        {
            return currentIndex >= 0 && currentIndex < previewHistory.Count
                ? previewHistory[currentIndex]
                : null;
        }

        /// <summary>
        /// 현재 시드 가져오기
        /// </summary>
        public int GetCurrentSeed()
        {
            return currentIndex >= 0 && currentIndex < seedHistory.Count
                ? seedHistory[currentIndex]
                : 0;
        }

        /// <summary>
        /// 히스토리 초기화
        /// </summary>
        public void ClearHistory()
        {
            previewHistory.Clear();
            seedHistory.Clear();
            currentIndex = -1;
            previewImage.sprite = null;
        }
    }
}
