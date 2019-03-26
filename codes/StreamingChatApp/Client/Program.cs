using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using StreamingChatApp;

namespace Client
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var userName = args?.FirstOrDefault() ?? "Me";

            var channel = new Channel("localhost:5001", ChannelCredentials.Insecure);

            var client = new ChatService.ChatServiceClient(channel);
            
            using (var call = client.Chat())
            {
                var cancellationSignal = new CancellationTokenSource();

                var cancellationTask = Task.Run(() =>
                {
                    Console.WriteLine("Press any key to stop");
                    Console.ReadKey();

                    cancellationSignal.Cancel();
                });

                await call.RequestStream.WriteAsync(new ChatRequest
                {
                    Request = new ChatMessage
                    {
                        From = userName,
                        Message = $"Hi there, it's {userName}!"
                    }
                });
                Console.WriteLine("Message sent!");

                while (await call.ResponseStream.MoveNext(CancellationToken.None))
                {
                    var message = call.ResponseStream.Current;
                    Console.WriteLine($"Received message from {message.Response.From}: {message.Response.Message}");
                }

                await cancellationTask;
                await call.RequestStream.CompleteAsync();
            }

            await channel.ShutdownAsync();
        }
    }
}
