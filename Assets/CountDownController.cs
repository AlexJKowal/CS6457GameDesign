using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountDownController : MonoBehaviour
{
    public int countDownTime;
    public TextMeshProUGUI countDownText;

    private void Awake()
    {
        StartCoroutine(CountdownToStart());
    }

    IEnumerator CountdownToStart()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1;
        
        while (countDownTime > 0)
        {
            countDownText.text = countDownTime.ToString();
            countDownTime--;
            yield return new WaitForSeconds(1f);
        }

        countDownText.text = "Go!";
        
        yield return new WaitForSeconds(1f);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
    }
}
