using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.IO;

namespace blockchain_parser.WebSocket
{
    public class WebSocketConnection
    {
        private object consoleLock = new object();
        private readonly uint receiveChunkSize = AppConfig.ReceiveChunkSize;
        private readonly ulong totalMessageLimit = AppConfig.WebSocketMessageLimit;

        
        private readonly bool verbose = AppConfig.WebSocketLogVerbose;
        private ClientWebSocket webSocket = null;

        private static UTF8Encoding encoder = new UTF8Encoding();

        public Action onOpen {
            private get;
            set;
        }

        public Action onClose {
            private get;
            set;
        }

         public Action<string> onMessage {
            private get;
            set;
        } 

        public Action<Exception> onError {
            private get;
            set;
        }

        public WebSocketConnection() {
            onOpen = () =>  {

            };
            onClose = () =>  {

            };
            onMessage = (string message) => {

            };
            onError = (Exception message) => {

            };
        
        }

        public void Connect(string uri) {
            ConnectServer(uri).ConfigureAwait(false);
        }

        public void Close() {
             if (webSocket != null){
                lock (consoleLock)
                {
                    webSocket.Dispose();         
                    if(verbose){
                        Console.WriteLine();
                        Logger.LogStatus(ConsoleColor.Red, "WebSocket closed.");
                    }
                    webSocket = null;
                }
                onClose();
             }
            
        }

        public void SendMessage(string message) {
             Task.Factory.StartNew(() =>
                {
                    try {
                        Task.WhenAll(Send(message));
                    }
                    catch(Exception ex) {
                        if(verbose)
                            Logger.LogStatus(ConsoleColor.Red, "Exception: " + ex);
                        onError(ex);
                        Close();
                    }
                });
        }

        private async Task ConnectServer(string uri)
        {
            try
            {
                Close();
                if(verbose)
                    Logger.LogStatus(ConsoleColor.Green, "Connecting to " + uri + " ...");
                lock (consoleLock)
                {
                    webSocket = new ClientWebSocket();
                }
                await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
                if(verbose)
                    Logger.LogStatus(ConsoleColor.Green, uri + " connected!");
                var t = new Task (async () => await Task.WhenAll(Receive()));
                t.Start();
                onOpen();
            }
            catch (Exception ex)
            {
                if(verbose)
                   Logger.LogStatus(ConsoleColor.Red, "Exception: " + ex);
                onError(ex);
                Close();
            }
        }

        private async Task Send(string message)
        {

            byte[] buffer = encoder.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            if (webSocket.State == WebSocketState.Open && verbose)
                Logger.LogStatus(ConsoleColor.Blue, "REQUEST: " + encoder.GetString(buffer));
        }

        private async Task Receive()
        {
            
            while (webSocket.State == WebSocketState.Open)
            {
                if(verbose)
                    Logger.LogStatus(ConsoleColor.DarkBlue, "Awaiting message from websocket server...");

                string response = null;
                using (var ms = new MemoryStream()){
                    ulong total_message_size = 0;
                    WebSocketReceiveResult result;
                    do {
                        var buffer = new ArraySegment<byte>(new byte[receiveChunkSize]);
                        result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                            onClose();
                            return;
                        }
                        total_message_size += (ulong)result.Count;
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                        if(total_message_size > totalMessageLimit)
                            break;
                    } while(!result.EndOfMessage);
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                        response = reader.ReadToEnd();
                }

                if(verbose) 
                    Logger.LogStatus(ConsoleColor.Green, "RESPONSE: " + response);
                onMessage(response);
            }
        }

    }
}