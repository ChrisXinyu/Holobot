# HoloBotDemo #
本项目使用 [Bing Speech API](https://azure.microsoft.com/en-us/services/cognitive-services/speech/), [Bot Framework](https://dev.botframework.com/), [LUIS](https://www.luis.ai/home/index) 实现在Hololens端应用内的中文语音问答流程

本项目中使用到的LUIS及Bot Framework相关后端代码请参考 [BotDemo](https://github.com/leonlj/BotDemo) 中的 [VendingMachineBotDemo](https://github.com/leonlj/BotDemo/tree/master/VendingMachineBotDemo)

### 文件说明 ###
*HoloBot*      文件夹中包含HoloLens客户端Unity工程

### 准备工作 ###
本工程可以直接使用Unity打开，编译并部署到HoloLens设备中运行
如需使用自定义Bot和LUIS后端，请下载 [VendingMachineBotDemo](https://github.com/leonlj/BotDemo/tree/master/VendingMachineBotDemo) 代码，订阅 *Bot Framework, LUIS* 的KEY，并在代码的相应位置进行替换
代码中提供的 *Bing Speech* 的KEY为免费订阅，可以自行申请并在代码的相应位置进行替换
  
#### *Bing Speech API* ####
1、进入 [申请地址](https://azure.microsoft.com/zh-cn/try/cognitive-services/?api=speech-api) ，点击创建，登陆并获取Subscription Key
![](/Screenshots/1.jpg)

2、修改 *HoloBot\Assets\Scripts\SpeechToText.cs*，添加Subscription Key
![](/Screenshots/2.jpg)

#### *Bot Framework* ####
1、进入 [Bot管理后台](https://dev.botframework.com/bots) 登陆并创建Bot，获取BotId, AppId, AppPassword
![](/Screenshots/3.jpg)  

2、修改下载的BotDemo项目中的 *[BotDemo/VendingMachineBotDemo/Bot Application2/Web.config](https://github.com/leonlj/BotDemo/blob/master/VendingMachineBotDemo/Bot%20Application2/Web.config)*，添加BotId,AppId,AppPassword
![](/Screenshots/4.jpg) 

3、添加Direct Line Channel
![](/Screenshots/5.jpg)

4、获取Direct Line Key
![](/Screenshots/6.jpg)

5、修改 *HoloBot\Assets\Scripts\BotService.cs* ，添加Direct Line Key
![](/Screenshots/7.jpg)

#### *LUIS* ####
1、进入 [LUIS管理后台](https://www.luis.ai/applications) ，新增LUIS App　　  
![](/Screenshots/8.jpg)

2、获取 LUIS App Id 和 LUIS Endpoint Key
![](/Screenshots/9.jpg)

![](/Screenshots/10.jpg)

3、修改下载的BotDemo项目中的 *[BotDemo/VendingMachineBotDemo/Bot Application2/LUIS.cs](https://github.com/leonlj/BotDemo/blob/master/VendingMachineBotDemo/Bot%20Application2/LUIS.cs)*，添加Luis AppId和Key  
![](/Screenshots/11.jpg)

### 参考资料 ###
[Bing Speech API文档](https://docs.microsoft.com/zh-cn/azure/cognitive-services/speech/getstarted/getstartedrest)  
[Bot Framework API文档](https://docs.microsoft.com/en-us/bot-framework/rest-api/bot-framework-rest-direct-line-3-0-concepts)
