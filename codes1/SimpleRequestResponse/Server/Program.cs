using System;
using System.Threading.Tasks;
using Grpc.Core;
using SimpleRequestResponse;

namespace Server
{
    internal static class Program
    {
        private const int Port = 5000;

        static async Task Main(string[] args)
        {
            var server = new Grpc.Core.Server
            {
                Services = { GreeterService.BindService(new GreeterServiceImplementation()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            await server.ShutdownAsync();
        }
    }

    public class GreeterServiceImplementation : GreeterService.GreeterServiceBase
    {
        public override Task<HelloResponse> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloResponse
            {
                Reply = "Hello " + request.Name
            });
        }
    }
}
