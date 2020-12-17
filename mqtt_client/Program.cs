using System.Threading.Tasks;
using System;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace mqtt_client
{
    class Program
    {
        public static IMqttClient mqttClient;
        static void Main(string[] args)
        {
            ConnectMqttServerAsync();
            ImportData();
        }

        private static async void ConnectMqttServerAsync()
        {
            try
            {
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer("127.0.0.1", 8031)
                    .WithCredentials("USER", "PASS")
                    .WithClientId(Guid.NewGuid().ToString().Substring(0, 5))
                    .Build();

                //消息
                mqttClient.UseApplicationMessageReceivedHandler(x =>
                {
                    System.Console.WriteLine("收到的消息");
                    System.Console.WriteLine($"Topic={x.ApplicationMessage.Topic}");
                    System.Console.WriteLine($"PayLoad= {x.ApplicationMessage.Payload}");
                    System.Console.WriteLine($"Qos={x.ApplicationMessage.QualityOfServiceLevel}");
                    System.Console.WriteLine($"Retain={x.ApplicationMessage.Retain}");
                    System.Console.ReadLine();
                });

                //重连机制
                mqttClient.UseDisconnectedHandler(async e =>
                {
                    System.Console.WriteLine("与服务器断开");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    try
                    {

                        await mqttClient.ConnectAsync(options);
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"重新连接服务器失败 Msg:{ex}");
                    }
                });

                await mqttClient.ConnectAsync(options);
                System.Console.WriteLine($"连接服务器成功!输入任意内容并回车进入菜单页面");

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private static void ImportData()
        {
            System.Console.ReadLine();
            bool isExit = false;
            while (!isExit)
            {
                System.Console.WriteLine(@"请输入 1.订阅主题  2.取消订阅 3.发出消息 4.退出");
                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        System.Console.WriteLine(@"请输入主题名称:");
                        var topicName = Console.ReadLine();
                        Subscribe(topicName);
                        break;
                    case "2":
                        System.Console.WriteLine(@"请输入要取消订阅的主题名称:");
                        topicName = Console.ReadLine();
                        UnSubscribe(topicName);
                        break;
                    case "3":
                        System.Console.WriteLine(@"请输入要发送的主题名称:");
                        topicName = Console.ReadLine();
                        System.Console.WriteLine(@"请输入要发送的消息");
                        var message = Console.ReadLine();

                        break;
                    case "4": isExit = true; break;
                    default: System.Console.WriteLine("请输入正确指定"); break;
                }
            }
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topicName">主题名称</param>
        /// <returns></returns>
        private static async void Subscribe(string topicName)
        {
            var topic = topicName.Trim();
            if (string.IsNullOrEmpty(topic))
            {
                System.Console.WriteLine("订阅主题不能为空!");
                return;
            }
            if (!mqttClient.IsConnected)
            {
                System.Console.WriteLine("MQTT客户端尚未连接!");
                return;
            }
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="topicName"></param>
        /// <returns></returns>
        private static async void UnSubscribe(string topicName)
        {
            var topic = topicName.Trim();
            if (string.IsNullOrEmpty(topic))
            {
                System.Console.WriteLine("订阅主题不能为空!");
                return;
            }
            if (!mqttClient.IsConnected)
            {
                System.Console.WriteLine("MQTT客户端尚未连接!");
                return;
            }
            await mqttClient.UnsubscribeAsync(topic);
        }

        private static async void Publish(string topicName,string message){
            string topic = topicName.Trim();
            string msg = message.Trim();
            if(string.IsNullOrEmpty(topic)){
                System.Console.WriteLine("主题不能为空");
            }
            if (!mqttClient.IsConnected)
            {
                System.Console.WriteLine("MQTT客户端尚未连接!");
                return;
            }
            var MessageBuilder = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(msg)
            .WithExactlyOnceQoS()
            .WithRetainFlag()
            .Build();

            await mqttClient.PublishAsync(MessageBuilder);
        }
    }
}
