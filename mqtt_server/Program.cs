using System;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace mqtt_server
{
    class Program
    {
        public static IMqttServer mqttServer;
        static void Main(string[] args)
        {
           StartMqttServer();
        }

        private static async void StartMqttServer()
        {
            try
            {
                var options = new MqttServerOptions
                {
                    ConnectionValidator = new MqttServerConnectionValidatorDelegate(p =>
                    {
                        if (p.ClientId == "SpecialClient")
                        {
                            if (p.Username != "USER" || p.Password != "PASS")
                            {
                                p.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                            }
                        }
                    })
                };

                options.DefaultEndpointOptions.Port = 8031;
                mqttServer = new MqttFactory().CreateMqttServer();

                mqttServer.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(m =>
                {
                    var ClientId = m.ClientId;
                    var Topic = m.TopicFilter.Topic;
                    System.Console.WriteLine($"客户端[{ClientId}]已订阅主题:{Topic}");
                });

                mqttServer.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(x =>
                {
                    var ClientId = x.ClientId;
                    var Topic = x.TopicFilter;
                    System.Console.WriteLine($"客户端[{ClientId}]已取消订阅主题:{Topic}");
                });

                mqttServer.UseClientConnectedHandler(c =>
                {
                    var ClientId = c.ClientId;
                    System.Console.WriteLine($"客户端[{ClientId}]已连接");
                });

                mqttServer.UseClientDisconnectedHandler(c =>
                {
                    var ClientId = c.ClientId;
                    System.Console.WriteLine($"客户端[{ClientId}]已断开");
                });

                await mqttServer.StartAsync(options);

                System.Console.WriteLine("服务器启动成功!");
                System.Console.ReadLine();

                await mqttServer.StopAsync();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"服务端启动失败!{ex.Message}");
            }
        }
    }
}
