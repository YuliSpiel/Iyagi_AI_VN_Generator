using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StatInfo : MonoBehaviour
{
    public TextMeshProUGUI txt;
    public Button HomeBtn;

    private void Awake()
    {
        txt.text = 
                   $"Obedience : {GameManager.Instance.Stats.Obedience}  " + $"Will : {GameManager.Instance.Stats.Will}";
        
        HomeBtn.onClick.AddListener(OpenTitle);
    }

    public void OpenTitle()
    {
        SceneManager.LoadScene(0);
    }
}
