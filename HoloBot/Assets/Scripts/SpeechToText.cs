using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using System.IO;
using System;
using BotClient;
using UnityEngine.UI;
#if WINDOWS_UWP
using System.Threading.Tasks;
#endif

public class SpeechToText : Singleton<SpeechToText>
{
    public int messageLength = 3;      //录音时间，单位：秒

    private bool recording = false;
    private static string deviceName = string.Empty;  //microphone设备名称
    private int samplingRate;          //采样率
    private AudioClip audioClip;

    private static string fetchUri = "https://api.cognitive.microsoft.com/sts/v1.0";
    private static string subscriptionKey = "745d51f70a804bae90da163207801d3e"; //Bing Speech 订阅key
    private string token = null;
    private const int refreshTokenDuration = 9 * 60;  //Access token每10分钟后过期，9分钟后重新获取token
    private float startTime;
    BotService botService;

    private AudioSource audioSource;

    void Start () {
        int unUsed;
        Microphone.GetDeviceCaps(deviceName, out unUsed, out samplingRate);
        StartCoroutine(FetchToken(true));
        startTime = Time.time;
        botService = new BotService();
        audioSource = gameObject.GetComponent<AudioSource>();
#if WINDOWS_UWP
        botService.StartConversation();
#endif
    }

    void Update () {
        if (recording && !Microphone.IsRecording(deviceName))
        {
            RecordStop();
        }

        //token即将过期时，重新获取token
        if (Time.time - startTime >= refreshTokenDuration)
        {
            Debug.Log("ReFetchToken");
            StartCoroutine(FetchToken(false));
            startTime = Time.time;
        }
	}

    /// <summary>
    /// 获取Bing Speech Token值
    /// </summary>
    /// <returns></returns>
    public string GetToken()
    {
        return token;
    }

    public bool IsRecording()
    {
        return recording;
    }

    /// <summary>
    /// 获取token
    /// </summary>
    /// <returns></returns>
    private IEnumerator<object> FetchToken(bool showTipFlag)
    {
        var headers = new Dictionary<string, string>() {
            { "Ocp-Apim-Subscription-Key", subscriptionKey }
        };
        UriBuilder uriBuilder = new UriBuilder(fetchUri);
        uriBuilder.Path += "/issueToken";
        byte[] postdata = new byte[1];
        WWW www = new WWW(uriBuilder.ToString(), postdata, headers);
        yield return www;
        token = www.text;
        Debug.Log(token);
        if (showTipFlag)
        {
            ModelManager.Instance.SetTipText("点我进行提问");
        }
    }

    /// <summary>
    /// 使用Bing Speech API，将语音文件转成text
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    private IEnumerator<object> AudioToText(string filepath)
    {
        string requestUri = "https://speech.platform.bing.com/recognize";
        requestUri += @"?scenarios=smd";
        requestUri += @"&appid=D4D52672-91D7-4C74-8AD8-42B1D98141A5";     // You must use this ID.
        requestUri += @"&locale=zh-CN";
        requestUri += @"&device.os=win10";
        requestUri += @"&version=3.0";
        requestUri += @"&format=json";
        requestUri += @"&instanceid=565D69FF-E928-4B7E-87DA-9A750B96D9E3";
        requestUri += @"&requestid=" + Guid.NewGuid().ToString();

        FileStream fs = null;
        using (fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
        {
            byte[] buffer = null;
            buffer = new Byte[(int)fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            var headers = new Dictionary<string, string>() {
                { "Authorization", "Bearer " + token },
                { "Content-Type", @"audio/wav; codec=""audio/pcm""; samplerate=16000" }
            };
            WWW www = new WWW(requestUri, buffer, headers);

            yield return www;
            try
            {
                string result = www.text;
                JSONObject jsonObj = new JSONObject(result);
                string resultStr = jsonObj.GetField("header").GetField("name").str;
                resultStr = TrimResultStr(resultStr);
                ModelManager.Instance.SetResponseText("问：" + resultStr);
#if WINDOWS_UWP
                SendMessage(resultStr);
#endif
            }
            catch
            {
                TextToSpeech.Instance.SpeakText("对不起，无法听清您说什么");
            }
        }
    }

#if WINDOWS_UWP
    private async void SendMessage(string message)
    {
        string result = "对不起，无法回答您的问题";
        if (await botService.SendMessage(message))
        {
            ActivitySet messages = await botService.GetMessages();
            if (messages != null)
            {
                for (int i = 1; i < messages.activities.Length; i++)
                {
                    result = messages.activities[i].text;
                }
            }
        }
        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            TextToSpeech.Instance.SpeakText(result);
        }, false); 
    } 
#endif

    /// <summary>
    /// 对Speech API返回的结果进行处理，去除最后的句号，防止影响结果
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private string TrimResultStr(string result)
    {
        string resultStr = result;
        if (resultStr != null)
        {
            int index = resultStr.LastIndexOf("。");
            if (index > 0)
            {
                resultStr = resultStr.Remove(index, 1);
            }
        }
        return resultStr;
    }

    /// <summary>
    /// 开始录音
    /// </summary>
    public void Record()
    {
        recording = true;
        audioSource.Stop();

        ModelManager.Instance.SetMicrophoneIcon(true);
        ModelManager.Instance.SetTipText("正在聆听中");
        ModelManager.Instance.SetResponseText("");

        if (Microphone.IsRecording(deviceName))
        {
            return;
        }
        audioClip = StartRecording();
    }

    /// <summary>
    /// 停止录音,将语音保存成文件
    /// </summary>
    public void RecordStop()
    {
        recording = false;

        ModelManager.Instance.SetMicrophoneIcon(false);
        ModelManager.Instance.SetTipText("思考中，请稍候");

        StopRecording();
        string filename = "myfile.wav";
        var filepath = Path.Combine(Application.persistentDataPath, filename);
        SavWav.Save(filename, audioClip);
        StartCoroutine(AudioToText(filepath));
    }

    /// <summary>
    /// 开始录音
    /// </summary>
    /// <returns></returns>
    private AudioClip StartRecording()
    {
        return Microphone.Start(deviceName, false, messageLength, 16000);
    }

    /// <summary>
    /// 停止录音
    /// </summary>
    private void StopRecording()
    {
        if (Microphone.IsRecording(deviceName))
        {
            Microphone.End(deviceName);
        }
    }
}
