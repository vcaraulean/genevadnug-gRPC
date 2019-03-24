using System;
using System.Threading.Tasks;
using Grpc.Core;
using StreamingChatApp;

namespace Server
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var server = new Grpc.Core.Server
            {
                Services = { ChatService.BindService(new ChatServiceImpl()) },
                Ports = { new ServerPort("localhost", 5001, ServerCredentials.Insecure) }
            };

            // Start server
            server.Start();

            Console.WriteLine("ChatServer listening on port " + 5001);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            await server.ShutdownAsync();
        }
    }
}
