using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CountDownController : MonoBehaviour
{
    private int _countDownTime;
    public TextMeshProUGUI countDownText;
    public EventSystem eventSystem;
    private void Awake()
    {
        ShowCountDown(3);
    }

    public void ShowCountDown(int countDownTime)
    {
        _countDownTime = countDownTime;
        Time.timeScale = 1;
        StartCoroutine(CountdownToStart());
    }

    IEnumerator CountdownToStart()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        eventSystem.sendNavigationEvents = false;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1;
        
        while (_countDownTime > 0)
        {
            countDownText.text = _countDownTime.ToString();
            _countDownTime--;
            yield return new WaitForSeconds(1f);
        }

        countDownText.text = "Go!";
        
        yield return new WaitForSeconds(1f);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
        eventSystem.sendNavigationEvents = true;
    }
}
