# YYAssistant client

一个使用 Unity 和 yyassistant server 进行交互的项目，交互基于 websocket 的网络通信。

# 基础架构

## Config

使用保存在 json 中 pipeline_config 对远程 server 进行初始化之后，就正式进入与 assistant 交互的流程。config 相关可以参考 Setting 文件夹

## State

总体上 assistant 由一个状态机控制，目前分为三个状态，idle，recording，answering，具体可以参考 States 文件夹。分别如下：

1. idle 状态下 assistant 可以随时进入 recording 状态，当按下录制键或者 VAD 检测到正在说话，就会进入 recording 状态并开始录音。
2. recording 状态下，系统会等待结束录音（松开录制键或者 VAD 检测到说话结束），之后就进入 answering 状态。
3. answering 状态下，系统会收集刚才得到的录音，并发送给服务器，等待服务器回复直至结束。

当系统改变状态时，会广播 state_change 的信号，emotion 组件（控制 motion 和 expression），talking 组件，content 组件（显示图片和文字）分别会进行相应处理。

## Recorder

控制录音的组件为 MicrophoneManager，它会持续在后台录制音频，当 ServiceRecorder 开始录音时会记录开始时间，等结束录音时会用开始与结束时间向 MicrophoneManager 请求这期间的音频。这样可以做到多个不同的 ServiceRecorder 互不干扰地进行录制。也可以使得 VAD 可以实时检测当前音量。具体可以参考 Recorder 文件夹。

## WebSocket

系统传给服务器的音频由 WebSocketClinet 组件控制，通过 websocket 协议将音频传输给服务器，同时开启一个服务来接收回应。每当接收到回应会存入一个队列，并交给 DataProcessor 处理。

DataProcessor 会根据每个回答的内容分别路由到对应组件的接口上。每个回答都带有时间戳，可以在取消请求被触发时丢弃所有之前的回应。

ContentLoader 复杂把图片和文字显示在屏幕上，而 TalkingManager 会在队列非空的情况下持续播放接收到的音频，并在每段播放开始的时候通知 ContentLoader 显示此时的文字。

## Signal

除了服务器接收到的音频文件这种比较大的消息会交给 DataProcessor 处理，其他消息都由 SignalManager 处理（参考 Utils/SignalManager）。发送方会直接使用 SignalManager.SendSignal(type, message)。接收方则通过 SignalManager.Add(type)来监听来自指定 type 的 message。

在 SignalManager 中，可以编辑消息的路由方式，默认情况下，来在 typeA 的消息会转发到所有 type
A 的监听者，也会转发给所有列表中转发该 type 的所有 type，比如列表中为：

1. (typeA, typeB, messageA, messageB): 那么 typeA.messageA 的信号会触发 typeB.messageB
2. (typeA, typeB, all, all): 那么 typeA 的所有信号会转发给 typeB
3. (typeA, typeB, messageA, all): 那么 typeA.messageA 的信号会触发 typeB.messageA
4. (typeA, typeB, all, messageB): 那么 typeA 的所有信号会触发 typeB.messageB

通过这种方式，我们可以很方便地定义 assistant 各种组件地信号路由
