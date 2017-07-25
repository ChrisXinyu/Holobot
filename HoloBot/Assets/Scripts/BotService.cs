#if WINDOWS_UWP
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
#endif

namespace BotClient
{
#if WINDOWS_UWP
    class Conversation
    {
        public string conversationId { get; set; }
        public string token { get; set; }
        public int expires_in { get; set; }
        public string streamUrl { get; set; }
        public string referenceGrammarId { get; set; }
        public string eTag { get; set; }
    }

    public class Activity
    {
        public string type { get; set; }
        public string id { get; set;}
        public string timestamp { get; set; }
        public string localTimestamp { get; set; }
        public string serviceUrl { get; set; }
        public string channelId { get; set; }
        public ChannelAccount from { get; set; }
        public ConversationAccount conversation { get; set; }
        public ChannelAccount recipient { get; set; }
        public string textFormat { get; set; }
        public string attachmentLayout { get; set; }
        public ChannelAccount[] membersAdded { get; set; }
        public ChannelAccount[] membersRemoved { get; set; }
        public string topicName { get; set; }
        public bool historyDisclosed { get; set; }
        public string locale;
        public string text { get; set; }
        public string speak { get; set; }
        public string inputHint { get; set; }
        public string summary { get; set; }
        public SuggestedActions suggestedActions { get; set; }
        public Attachment[] attachments { get; set; }
        public Entity[] entities { get; set; }
        public Object channelData { get; set; }
        public string action { get; set; }
        public string replyToId { get; set; }
        public Object value { get; set; }
        public string name { get; set; }
        public ConversationReference relatesTo { get; set; }
        public string code { get; set; }
    }

    public class ChannelAccount
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class ConversationAccount
    {
        public bool isGroup { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public class SuggestedActions
    {
        public string[] to { get; set; }
        public CardAction[] actions { get; set; }
    }

    public class Attachment
    {
        public string contentType { get; set; }
        public string contentUrl { get; set; }
        public Object content { get; set; }
        public string name { get; set; }
        public string thumbnailUrl { get; set; }
    }

    public class Entity
    {
        public string type { get; set; }
    }

    public class ConversationReference
    {
        public string activityId { get; set; }
        public ChannelAccount user { get; set; }
        public ChannelAccount bot { get; set; }
        public ConversationAccount conversation { get; set; }
        public string channelId { get; set; }
        public string serviceUrl { get; set; }
    }

    public class CardAction
    {
        public string type { get; set; }
        public string title { get; set; }
        public string image { get; set; }
        public Object value { get; set; }
    }

    public class ActivitySet
    {
        public Activity[] activities { get; set; }
        public string watermark { get; set; }
    }

    public class BotService
    {
        //Bot Framework Directline通道key
        private string APIKEY = "aDyJxnUSx30.cwA.WOg.4DzXtwItzBC6jyUCxHXG8fLKcgdx2zZYf2BkkfW5Lpc";
        private string BASEHOST = "https://directline.botframework.com/";
        private string activeConversation = null;
        private string activeWatermark;

        /// <summary>
        /// json序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string JsonSerializer<T>(T t)
        {
            try
            {
                string JsonStr = JsonConvert.SerializeObject(t);
                return JsonStr;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="JsonStr"></param>
        /// <returns></returns>
        public static T JsonDeserialize<T>(string JsonStr)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(JsonStr);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 建立会话
        /// </summary>
        /// <returns></returns>
        public async Task<string> StartConversation()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BASEHOST);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //添加header
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + APIKEY);

                HttpResponseMessage response = await client.PostAsync("/v3/directline/conversations", null);
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    Conversation myConversation = JsonDeserialize<Conversation>(result);
                    if (myConversation == null)
                    {
                        return null;
                    }
                    activeConversation = myConversation.conversationId;
                    return myConversation.conversationId;
                }
            }
            return null;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        public async Task<bool> SendMessage(string message)
        {
            using (var client = new HttpClient())
            {
                string conversationId = activeConversation;
                if (conversationId == null)
                {
                    return false;
                }

                client.BaseAddress = new Uri(BASEHOST);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //添加认证header
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + APIKEY);

                var activity = new Activity()
                {
                    type = "message",
                    from = new ChannelAccount { id = "Adjacentech"},
                    text = message
                };

                string postBody = JsonSerializer(activity);
                HttpResponseMessage response = await client.PostAsync("/v3/directline/conversations/" + conversationId + "/activities",
                    new StringContent(postBody, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var re = response.Content.ReadAsStringAsync().Result;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 获取响应消息
        /// </summary>
        /// <returns></returns>
        public async Task<ActivitySet> GetMessages()
        {
            using (var client = new HttpClient())
            {
                string conversationId = activeConversation;
                if (activeConversation == null)
                {
                    return null;
                }
                client.BaseAddress = new Uri(BASEHOST);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //添加认证header
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + APIKEY);

                ActivitySet activitySet = new ActivitySet();
                string messageURL = "/v3/directline/conversations/" + conversationId + "/activities";
                if (activeWatermark != null)
                {
                    messageURL += "/?watermark=" + activeWatermark;
                }
                 
                HttpResponseMessage response = await client.GetAsync(messageURL);
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    activitySet = JsonConvert.DeserializeObject<ActivitySet>(result);
                    activeWatermark = activitySet.watermark;
                    return activitySet;
                }
                return activitySet;
            }
        }
    }
#endif

#if !WINDOWS_UWP
    public class BotService
    {

    }
#endif
} 

