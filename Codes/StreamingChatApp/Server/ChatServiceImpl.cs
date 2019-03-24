using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using StreamingChatApp;

namespace Server
{
    public class ChatServiceImpl : ChatService.ChatServiceBase
    {
        private static HashSet<IServerStreamWriter<ChatNotificationResponse>> _connectedClients;

        public ChatServiceImpl()
        {
            _connectedClients = new HashSet<IServerStreamWriter<ChatNotificationResponse>>();
        }

        public override async Task Chat(
            IAsyncStreamReader<ChatRequest> requestStream, 
            IServerStreamWriter<ChatNotificationResponse> responseStream, 
            ServerCallContext context)
        {
            var userHeader = context.RequestHeaders.FirstOrDefault(x => x.Key == "username")?.Value ?? "unknown";
            Console.WriteLine($"Chat started with {context.Host}, user {userHeader}");

            _connectedClients.Add(responseStream);

            while (await requestStream.MoveNext(CancellationToken.None))
            {
                var receivedMessage = requestStream.Current;

                var response = new ChatNotificationResponse
                {
                    Response = new ChatMessage
                    {
                        From = receivedMessage.Request.From,
                        Message = receivedMessage.Request.Message
                    }
                };

                foreach (var listener in _connectedClients)
                {
                    await listener.WriteAsync(response);
                }
            }

            Console.WriteLine($"Client {context.Host} dropped off");
            _connectedClients.Remove(responseStream);
        }
    }
}