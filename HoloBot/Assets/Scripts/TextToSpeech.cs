using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using System.IO;
using System;

public class TextToSpeech : Singleton<TextToSpeech>
{
    private static string SSML = "<speak version='1.0' xml:lang='zh-CN'><voice xml:lang='zh-CN' xml:gender='Male' name='Microsoft Server Speech Text to Speech Voice (zh-CN, Kangkang, Apollo)'>{0}</voice></speak>";
    AudioSource audioSource;
    // Use this for initialization
    void Start () {
        audioSource = gameObject.GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// 使用bing speech api,将文字转为中文语音
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public IEnumerator<object> TextToAudio(string text)
    {
        string requestUri = "https://speech.platform.bing.com/synthesize";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(string.Format(SSML, text));
        var headers = new Dictionary<string, string>() {
            { "Authorization", "Bearer " + SpeechToText.Instance.GetToken() },
            { "Content-Type", @"audio/wav; samplerate=16000" },
            { "X-Microsoft-OutputFormat", @"riff-16khz-16bit-mono-pcm"},
            { "X-Search-AppId", Guid.NewGuid().ToString().Replace("-", "")},
            { "X-Search-ClientID", Guid.NewGuid().ToString().Replace("-", "")},
            { "User-Agent", "TTSHololens"}
        };
        audioSource.Stop();
        WWW www = new WWW(requestUri, buffer, headers);
        yield return www;
        audioSource.clip = www.GetAudioClip(false, true, AudioType.WAV);
        audioSource.Play();
        ModelManager.Instance.SetTipText("点我进行提问");
        ModelManager.Instance.SetResponseText("答：" + text);
    }

    public void SpeakText(string text)
    {
        StartCoroutine(TextToAudio(text));
    }
}
