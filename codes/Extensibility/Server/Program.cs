using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using Grpc.Reflection;
using Grpc.Reflection.V1Alpha;
using SimpleRequestResponse;

namespace Server
{
    internal static class Program
    {
        private const int Port = 3005;

        static async Task Main(string[] args)
        {
            var server = new Grpc.Core.Server
            {
                Services =
                {
                    ServerReflection.BindService(new ReflectionServiceImpl(GreeterServiceReflection.Descriptor.Services)),
                    Health.BindService(new HealthServiceImpl()),

                    GreeterService
                        .BindService(new GreeterServiceImplementation())
                        .Intercept(new SimpleInterceptor())
                },
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
            var clientName = context.RequestHeaders.FirstOrDefault(h => h.Key == "client-name")?.Value ?? "unknown";

            Console.WriteLine($"Client name received in metadata: {clientName}");

            return Task.FromResult(new HelloResponse
            {
                Reply = "Hello " + request.Name
            });
        }
    }
}
