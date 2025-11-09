// SimpleLanguageDropdownTMP.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;

public class SimpleLanguageDropdownTMP : MonoBehaviour
{
    TMP_Dropdown dropdown;
    List<Locale> locales;

    IEnumerator Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        yield return LocalizationSettings.InitializationOperation; // 로컬라이제이션 로드 대기

        locales = LocalizationSettings.AvailableLocales.Locales;

        // 옵션 채우기 (표시는 네이티브 이름 or 코드)
        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var loc in locales)
        {
            var name = loc.Identifier.CultureInfo != null
                ? loc.Identifier.CultureInfo.NativeName
                : loc.Identifier.Code;
            options.Add(new TMP_Dropdown.OptionData(name));
        }
        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        // 현재 선택 반영
        int idx = locales.FindIndex(l => l == LocalizationSettings.SelectedLocale);
        dropdown.SetValueWithoutNotify(Mathf.Max(0, idx));

        // 변경 시 SelectedLocale만 교체
        dropdown.onValueChanged.AddListener(i =>
        {
            if (i >= 0 && i < locales.Count)
            {
                var selectedLocale = locales[i];
                LocalizationSettings.SelectedLocale = selectedLocale;

                // 문자열로 저장 ("ko-KR", "en", "ja" 등)
                string code = selectedLocale.Identifier.Code;
                PlayerPrefs.SetString("LocaleCode", selectedLocale.Identifier.Code);
                PlayerPrefs.Save();
                if (GameManager.Instance != null)
                {
                    switch (code)
                    {
                        case "ko":
                        case "ko-KR":
                            GameManager.Instance.CurrentLanguage = GameLanguage.Korean;
                            break;
                        case "en":
                        case "en-US":
                            GameManager.Instance.CurrentLanguage = GameLanguage.English;
                            break;
                        case "ja":
                        case "ja-JP":
                            GameManager.Instance.CurrentLanguage = GameLanguage.Japanese;
                            break;
                        default:
                            GameManager.Instance.CurrentLanguage = GameLanguage.English;
                            break;
                    }
                }
            }
        });
    }
}