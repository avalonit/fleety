using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System.Fabric;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Listener.Models;

namespace Listener.Services
{
    public class TcpCommunicationService : ICommunicationListener, IDisposable
    {
        private readonly CancellationTokenSource processRequestsCancellation = new CancellationTokenSource();

        public int Port { get; set; }

        private TcpListener? server;
        private TcpClient? client;

        /// <summary>
        /// Stops the Server Ungracefully
        /// </summary>
        public void Abort()
        {
            StopWebServer();
        }

        /// <summary>
        /// Stops the Server Gracefully
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Task for Asynchron usage</returns>
        public Task CloseAsync(CancellationToken cancellationToken)
        {
            DisposeClient();
            StopWebServer();

            return Task.FromResult(true);
        }

        /// <summary>
        /// Free Resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes Configuration
        /// </summary>
        /// <param name="context">Code Package Activation Context</param>
        public void Initialize(ICodePackageActivationContext context)
        {
            var serviceEndpoint = context.GetEndpoint("ServiceEndpoint");
            Port = serviceEndpoint.Port;
        }

        /// <summary>
        /// Starts the Server
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Task for Asynchron usage</returns>
        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            try
            {
                server = new TcpListener(IPAddress.Any, Port);
                server.Start();
                while (!processRequestsCancellation.Token.IsCancellationRequested)
                {
                    client = server.AcceptTcpClient();
                    if (MessageHandling != null)
                    {
                        var clientThread = new Thread(new ParameterizedThreadStart(MessageHandling));
                        clientThread.Start(client);
                    }
                }
                server.Stop();
                DisposeClient();
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message("OpenAsync:" + ex.Message);
                if (server != null)
                    server.Stop();
            }

            ThreadPool.QueueUserWorkItem((state) =>
            {
                MessageHandling(processRequestsCancellation.Token);
            });

            return Task.FromResult("tcp://" + FabricRuntime.GetNodeContext().IPAddressOrFQDN + ":" + Port);
        }

        protected async void MessageHandling(object obj)
        {
            try
            {
                if (client != null)
                {
                    var clientStream = client.GetStream();
                    var receivedBytes = new byte[4096];
                    var bytesRead = 0;

                    while (!processRequestsCancellation.Token.IsCancellationRequested)
                    {
                        if (clientStream.DataAvailable)
                        {
                            bytesRead = clientStream.Read(receivedBytes);
                            if (bytesRead != 0)
                            {
                                var encoder = new ASCIIEncoding();

                                var content = encoder.GetString(receivedBytes, 0, bytesRead);

                                try
                                {
                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        var manager = new GPSManager();
                                        var messageData = manager.Validate(content);
                                        var apiService = new ApiService();
                                        if (messageData.MessageStatus == GPSMessageStatus.MessagePing)
                                        {
                                            client.Client.Send(Encoding.ASCII.GetBytes(content));
                                        }
                                        else if (messageData.MessageStatus == GPSMessageStatus.MessagePingPong)
                                        {
                                            client.Client.Send(Encoding.ASCII.GetBytes(content));
                                        }
                                        else if (messageData.MessageStatus == GPSMessageStatus.MessageValid)
                                        {
                                            client.Client.Send(Encoding.ASCII.GetBytes(content));

                                            if (messageData.MessageType == GPSMessageType.GPS_Tracking)
                                            {
                                            }
                                        }
                                        else
                                            client.Client.Send(Encoding.ASCII.GetBytes("ERROR"));

                                        await apiService.Post(messageData);

                                    }

                                }
                                catch (Exception ex)
                                {
                                    var errorAnswer = Encoding.ASCII.GetBytes(ex.Message.ToString());
                                    client.Client.Send(errorAnswer);
                                    ServiceEventSource.Current.Message(DateTime.Now.ToShortDateString() + " " + ex.Message + " " + content);
                                }
                            }
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message("MessageHandling:" + ex.Message);
            }
        }



        /// <summary>
        /// Free Resources and Stop Server
        /// </summary>
        /// <param name="disposing">Disposing .NET Resources?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeClient();
            }
        }

        private void DisposeClient()
        {
            if (server != null)
            {
                try
                {
                    if (client != null)
                    {
                        client.Close();
                        client.Dispose();
                    }
                    server.Stop();
                    server = null;
                    client = null;
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.Message("DisposeClient:" + ex.Message);
                }
            }
        }

        /// <summary>
        /// Stops Server and Free Handles
        /// </summary>
        private void StopWebServer()
        {
            processRequestsCancellation.Cancel();
            Dispose();
        }
    }
}
