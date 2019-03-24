using System;
using System.Threading.Tasks;
using Grpc.Core;
using SimpleRequestResponse;

namespace Client
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            var channel = new Channel("localhost:5000", ChannelCredentials.Insecure);
            var client = new GreeterService.GreeterServiceClient(channel);

            var reply = await client.SayHelloAsync(new HelloRequest { Name = "Client" });

            Console.WriteLine("Reply: " + reply.Reply);

            await channel.ShutdownAsync();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
