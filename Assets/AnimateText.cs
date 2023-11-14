using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnimateText : MonoBehaviour
{
    private float _maxFountSize = 60;
    private float _animateTimeInSeconds;
    private int _counts = 0;
    private TextMeshPro _textMeshPro;
    private float _originalFontSize;
    
    // Start is called before the first frame update
    void Start()
    {
        _textMeshPro = this.GetComponent<TextMeshPro>();
        _originalFontSize = _textMeshPro.fontSize;
        _textMeshPro.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _counts++;
    }

    public void ShowText(String message,float waitSeconds)
    {
        _textMeshPro.SetText(message);
        _textMeshPro.gameObject.SetActive(true);
        _animateTimeInSeconds = waitSeconds;
        StartCoroutine(StartAnimateText());
    }
    
    IEnumerator StartAnimateText()
    {
        while (_counts <= _animateTimeInSeconds * 60)
        {
            _textMeshPro.fontSize = Math.Min(_originalFontSize + _counts / 5f, _maxFountSize); 
            yield return new WaitForSeconds(1/60f);
        }

        _counts = 0;
        _textMeshPro.gameObject.SetActive(false);
        _textMeshPro.fontSize = _originalFontSize;
        _textMeshPro.SetText("");
    }
}
