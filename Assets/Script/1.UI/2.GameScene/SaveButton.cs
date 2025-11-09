using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 버튼 클릭 시 현재 슬롯에 진행 상황을 저장합니다.
/// - GameManager.Instance.SaveCurrentProgress() 호출
/// - Button 컴포넌트에 붙여도 되고, 빈 오브젝트에 붙여서 인스펙터로 버튼을 연결해도 됩니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class SaveButton : MonoBehaviour
{
    [Tooltip("눌렀을 때 저장을 수행할 버튼. 비워두면 자기 자신에서 자동으로 찾습니다.")]
    public Button targetButton;

    private void Awake()
    {
        if (targetButton == null)
            targetButton = GetComponent<Button>();

        if (targetButton != null)
            targetButton.onClick.AddListener(OnClickSave);
        else
            Debug.LogError("[SaveButton] Button reference missing.");
    }

    private void OnDestroy()
    {
        if (targetButton != null)
            targetButton.onClick.RemoveListener(OnClickSave);
    }

    private void OnClickSave()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[SaveButton] GameManager.Instance not found.");
            return;
        }

        GameManager.Instance.SaveCurrentProgress();
        Debug.Log("[SaveButton] Saved current slot.");
    }
}