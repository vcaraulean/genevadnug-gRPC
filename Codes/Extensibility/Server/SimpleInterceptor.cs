using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Server
{
    public class SimpleInterceptor : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request, 
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            Console.WriteLine($"Incoming call from {context.Host} for {context.Method} operation");
            
            var stopwatch = Stopwatch.StartNew();

            var response = await base.UnaryServerHandler(request, context, continuation);

            Console.WriteLine($"Call to {context.Method} took {stopwatch.Elapsed}");

            return response;
        }
    }
}