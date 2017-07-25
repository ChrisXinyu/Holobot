using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;
using HoloToolkit.Unity;
using UnityEngine.UI;

public class ModelManager : Singleton<ModelManager>, IInputClickHandler {

    public GameObject microphoneIcon = null;
    public Text responseText = null;
    public Text tipText = null;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetMicrophoneIcon(bool showFlag)
    {
        if (microphoneIcon != null)
        {
            microphoneIcon.SetActive(showFlag);
        }
    }

    /// <summary>
    /// 设置bot 提示内容
    /// </summary>
    /// <param name="text"></param>
    public void SetTipText(string text)
    {
        if (tipText != null)
        {
            tipText.text = text;
        }
    }

    /// <summary>
    /// 显示响应消息
    /// </summary>
    /// <param name="text"></param>
    public void SetResponseText(string text)
    {
        if (responseText != null)
        {
            responseText.text = text;
        }
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (SpeechToText.Instance.IsRecording())
        {
            SpeechToText.Instance.RecordStop();
        }
        else
        {
            SpeechToText.Instance.Record();
        }
    }
}
