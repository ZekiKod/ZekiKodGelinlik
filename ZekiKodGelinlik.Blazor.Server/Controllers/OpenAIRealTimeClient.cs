using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class OpenAIRealTimeClient : ViewController
    {
        private readonly ClientWebSocket _webSocket;
        private const string Url = "wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01";
        private readonly IJSRuntime _jsRuntime;
        public OpenAIRealTimeClient()
        {
            _webSocket = new ClientWebSocket();
            _webSocket.Options.SetRequestHeader("Authorization", "Bearer sk-i_xeISjve8wP0IvRWnPfzVUBIM3FwLxfq42kuThiWcT3BlbkFJTXt1pq1qk5cwsA603PQTyniw2CKDRlhuKvVJc5EW4A");
            _webSocket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");
        }

        public async Task ConnectAsync()
        {
            await _webSocket.ConnectAsync(new Uri(Url), CancellationToken.None);
            Console.WriteLine("Connected to OpenAI Realtime API.");
        }

        public async Task SendCommandAsync(string commandText)
        {
            var message = "{\"type\":\"response.create\",\"response\":{\"modalities\":[\"text\"],\"instructions\":\"" + commandText + "\"}}";

            var encodedMessage = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(encodedMessage);

            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024 * 4];
            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received Message: {message}");
                // Gelen mesajı ViewController'a ilet
                await _jsRuntime.InvokeVoidAsync("DotNet.invokeMethodAsync", "ZekiKodGelinlik.Blazor.Server", "ProcessCommand", message);

            }
        }

        public async Task CloseConnectionAsync()
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            Console.WriteLine("Connection closed.");
        }
    }
}
