using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IyagiAI.Runtime;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// Setup Wizard - Step 6: 최종 확인 및 프로젝트 생성
    /// 모든 설정 요약 표시 + 프로젝트 저장
    /// </summary>
    public class Step6_Finalize : MonoBehaviour
    {
        [Header("References")]
        public SetupWizardManager wizardManager;

        [Header("Summary UI")]
        public TMP_Text summaryText;
        public ScrollRect summaryScrollRect;

        [Header("Buttons")]
        public Button backButton;
        public Button createProjectButton;

        void Start()
        {
            // 요약 정보 표시
            DisplayProjectSummary();

            // 버튼 이벤트
            backButton.onClick.AddListener(OnBackClicked);
            createProjectButton.onClick.AddListener(OnCreateProjectClicked);
        }

        void DisplayProjectSummary()
        {
            var data = wizardManager.projectData;

            string summary = $@"<b><size=24>프로젝트 요약</size></b>

<b>게임 정보</b>
제목: {data.gameTitle}
장르: {data.genre}
분위기: {data.tone}
예상 플레이 타임: {data.playtime}
총 챕터 수: {data.totalChapters}

<b>게임 개요</b>
{data.gamePremise}

<b>핵심 가치</b>
{string.Join(", ", data.coreValues)}

<b>엔딩 조건</b>";

            foreach (var ending in data.endings)
            {
                summary += $"\n• {ending.endingName}: {ending.endingDescription}";
            }

            summary += $@"

<b>플레이어 캐릭터</b>
이름: {data.playerCharacter.characterName}
나이: {data.playerCharacter.age}
성별: {data.playerCharacter.gender}
시점: {data.playerCharacter.pov}
아키타입: {data.playerCharacter.archetype}
외모: {data.playerCharacter.appearanceDescription}
성격: {data.playerCharacter.personality}

<b>NPC 캐릭터</b>";

            if (data.npcs.Count == 0)
            {
                summary += "\n(NPC 없음)";
            }
            else
            {
                foreach (var npc in data.npcs)
                {
                    summary += $@"
• {npc.characterName} ({npc.age}세, {npc.gender})
  역할: {npc.role}
  아키타입: {npc.archetype}
  연애 가능: {(npc.isRomanceable ? "예" : "아니오")}
  외모: {npc.appearanceDescription}
  성격: {npc.personality}";
                }
            }

            summary += "\n\n<b>준비 완료!</b>\n프로젝트를 생성하면 AI가 스토리를 생성할 준비가 완료됩니다.";

            summaryText.text = summary;

            // 스크롤을 맨 위로
            if (summaryScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                summaryScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void OnBackClicked()
        {
            // Step 5로 돌아가기 (NPC 추가 수정 가능)
            wizardManager.PreviousStep();
        }

        public void OnCreateProjectClicked()
        {
            createProjectButton.interactable = false;
            backButton.interactable = false;

#if UNITY_EDITOR
            // 에디터에서는 마지막 변경사항 저장
            wizardManager.SaveProjectAsset();
#else
            // 빌드에서는 JSON으로 저장
            SaveProjectAsJson();
#endif

            Debug.Log($"Project finalized: {wizardManager.projectData.gameTitle}");
            Debug.Log($"Project GUID: {wizardManager.projectData.projectGuid}");

            // 위저드 완료 (캐릭터 서브 에셋 추가 및 세이브 파일 생성)
            wizardManager.OnWizardComplete();
        }

        private void SaveProjectAsJson()
        {
            // Runtime에서는 Application.persistentDataPath에 JSON으로 저장
            string dir = $"{Application.persistentDataPath}/Projects";
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            string safeName = GetSafeFileName(wizardManager.projectData.gameTitle);
            string path = $"{dir}/{safeName}.json";

            // JSON 직렬화 (Unity JsonUtility는 Dictionary를 지원하지 않으므로 주의)
            // 실제 구현에서는 더 견고한 JSON 라이브러리(Newtonsoft.Json 등) 사용 권장
            string json = JsonUtility.ToJson(wizardManager.projectData, true);
            System.IO.File.WriteAllText(path, json);

            Debug.Log($"Project saved as JSON: {path}");
        }

        private string GetSafeFileName(string fileName)
        {
            // 파일명에 사용할 수 없는 문자 제거
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            string safe = fileName;
            foreach (char c in invalidChars)
            {
                safe = safe.Replace(c, '_');
            }
            return safe;
        }
    }
}
