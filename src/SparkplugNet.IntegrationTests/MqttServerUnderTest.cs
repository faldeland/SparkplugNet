// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MQTTServerUnderTest.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   A class to define MQTT Server for reuse throughout integration tests.
//   Starts an in-process MQTTnet broker automatically via [AssemblyInitialize].
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SparkplugNet.IntegrationTests
{
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MQTTnet;
    using MQTTnet.Server;

    /// <summary>
    /// Manages an in-process MQTT broker for the integration test assembly.
    /// The broker is started once before any test runs and stopped after all tests complete.
    /// </summary>
    [TestClass]
    public class MqttServerUnderTest
    {
        private static IMqttServer? mqttServer;

        /// <summary>
        /// The loopback address used by the in-process broker.
        /// </summary>
        public const string ServerAddress = "localhost";

        /// <summary>
        /// The dynamically assigned port for the in-process broker.
        /// Populated by <see cref="StartBrokerAsync"/> before any test runs.
        /// </summary>
        public static int ServerPort { get; private set; }

        /// <summary>
        /// Starts the in-process MQTT broker before any test in the assembly runs.
        /// </summary>
        [AssemblyInitialize]
        public static async Task StartBrokerAsync(TestContext context)
        {
            ServerPort = GetFreeTcpPort();

            var options = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(ServerPort)
                .WithConnectionValidator(ctx =>
                {
                    // Accept all connections regardless of credentials.
                    ctx.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                })
                .Build();

            mqttServer = new MqttFactory().CreateMqttServer();
            await mqttServer.StartAsync(options);
        }

        /// <summary>
        /// Stops the in-process MQTT broker after all tests in the assembly have run.
        /// </summary>
        [AssemblyCleanup]
        public static async Task StopBrokerAsync()
        {
            if (mqttServer != null)
            {
                await mqttServer.StopAsync();
                mqttServer.Dispose();
                mqttServer = null;
            }
        }

        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}