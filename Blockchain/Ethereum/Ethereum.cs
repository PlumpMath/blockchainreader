using System;
using blockchain_parser.WebSocket;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net;

namespace blockchain_parser.Blockchain.Ethereum
{
    public sealed class Ethereum
    {
        private static WebSocketConnection webSocket = null;
        public static void StartListenNewBlocks(Action<dynamic> block_result) {
            webSocket = new WebSocketConnection();
           
            webSocket.onOpen = () => {
                webSocket.SendMessage("{\"id\": 1, \"method\": \"eth_subscribe\", \"params\": [\"newHeads\"], \"jsonrpc\":\"2.0\"}");
            };
            
            webSocket.onMessage = (message) => {
                dynamic response = null;
                try{
                    response = JObject.Parse(message);
                }catch(Exception ex){
                    Logger.LogStatus(ConsoleColor.Red, "ERROR: " + ex);
                    return;
                }
                if(response != null && response.@params != null && response.@params.result != null)
                    Task.Factory.StartNew(() => {block_result(response.@params.result);});
            };

            webSocket.onError = (error) => {
                Logger.LogStatus(ConsoleColor.Red, AppConfig.WebsocketNodeUrl + " " + error.Message);
            };

            webSocket.onClose = () => {
                Logger.LogStatus(ConsoleColor.Red, "Connection with " + AppConfig.WebsocketNodeUrl + " has been closed");
            };

            webSocket.Connect(AppConfig.WebsocketNodeUrl);
        }   

        public static dynamic GetBlockDetails(string block_hash) {
             if(block_hash ==  null){
                Logger.LogStatus(ConsoleColor.Red, "ERROR: Block hash is not provided");
                return null;
             }
             dynamic block_response = null;
             using(WebClient client = new WebClient ()) {
                client.Headers[header: HttpRequestHeader.ContentType] = "application/json";
                try{
                    string response = client.UploadString(AppConfig.RpcNodeUrl, "{\"jsonrpc\":\"2.0\",\"method\":\"eth_getBlockByHash\",\"params\":[\"" + block_hash + "\", true],\"id\":1}");
                    block_response = JObject.Parse(response);
                } catch(Exception ex){
                    Logger.LogStatus(ConsoleColor.Red, "ERROR: " + ex);
                    return null;
                } 
             }
             if(block_response != null && block_response.result != null)
                return block_response.result;
            return null;
        }

        public static dynamic GetBlockDetails(ulong block_number) {
            
             dynamic block_response = null;
             using(WebClient client = new WebClient ()) {
                client.Headers[header: HttpRequestHeader.ContentType] = "application/json";
                try{
                    string response = client.UploadString(AppConfig.RpcNodeUrl, "{\"jsonrpc\":\"2.0\",\"method\":\"eth_getBlockByNumber\",\"params\":[\"" +  string.Format("0x{0:X16}", block_number) + "\", true],\"id\":1}");
                    block_response = JObject.Parse(response);
                } catch(Exception ex){
                    Logger.LogStatus(ConsoleColor.Red, "ERROR: " + ex);
                    return null;
                } 
             }
             if(block_response != null && block_response.result != null)
                return block_response.result;
            return null;
        }

        public static void StopListenNewBlocks() {
            if(webSocket != null)
                webSocket.Close();
            webSocket = null;   
        }

    }
}