using UnityEngine;
using UnityEditor;
using IyagiAI.SetupWizard;

/// <summary>
/// SetupWizardAutoFill 컴포넌트를 자동으로 추가하고 설정하는 헬퍼
/// </summary>
public class SetupWizardAutoFillHelper : EditorWindow
{
    [MenuItem("Iyagi/Setup AutoFill Component")]
    public static void SetupAutoFill()
    {
        // SetupWizardManager 찾기
        SetupWizardManager wizardManager = FindObjectOfType<SetupWizardManager>();

        if (wizardManager == null)
        {
            Debug.LogError("SetupWizardManager not found! Please open SetupWizardScene first.");
            return;
        }

        // 기존 SetupWizardAutoFill 컴포넌트 확인
        SetupWizardAutoFill autoFill = wizardManager.GetComponent<SetupWizardAutoFill>();

        if (autoFill == null)
        {
            // 컴포넌트 추가
            autoFill = wizardManager.gameObject.AddComponent<SetupWizardAutoFill>();
            Debug.Log("✅ SetupWizardAutoFill component added to SetupWizardManager");
        }
        else
        {
            Debug.Log("SetupWizardAutoFill component already exists");
        }

        // wizardManager 레퍼런스 연결
        SerializedObject serializedAutoFill = new SerializedObject(autoFill);
        SerializedProperty wizardManagerProp = serializedAutoFill.FindProperty("wizardManager");
        wizardManagerProp.objectReferenceValue = wizardManager;
        serializedAutoFill.ApplyModifiedProperties();

        EditorUtility.SetDirty(autoFill);
        EditorUtility.SetDirty(wizardManager);

        Debug.Log("✅ SetupWizardAutoFill is ready!");
        Debug.Log("Press F5 in Play Mode to auto-fill the current step.");
    }
}
