
class: center, middle

# gRPC

### Remote Procedure Calls for Modern Services

Valeriu Caraulean 

@vcaraulean valeriu@caraulean.com

---

# Agenda

Talk, Code, Demos

---

## Remote Procedure Calls

It's a *very* old concept. 

In distributed computing, a remote procedure call (RPC) is when a computer program causes a procedure (subroutine) to execute in a different address space (commonly on another computer on a shared network), which is coded as if it were a normal (local) procedure call, without the programmer explicitly coding the details for the remote interaction. 

--

Some of most known & used implementations:

 - CORBA
 - DCOM
 - RMI
 - Remoting 

Mostly TCP based, often platform specific

--

**Honorable mention**

> SOAP, WSDL

???

### SOAP

[SOAP](https://en.wikipedia.org/wiki/SOAP) (abbreviation for Simple Object Access Protocol) is a messaging protocol specification for exchanging structured information in the implementation of web services in computer networks. Its purpose is to provide extensibility, neutrality and independence. It uses XML Information Set for its message format, and relies on application layer protocols, most often Hypertext Transfer Protocol (HTTP) or Simple Mail Transfer Protocol (SMTP), for message negotiation and transmission.

SOAP has three major characteristics:

 - extensibility (security and WS-Addressing are among the extensions under development)
 - neutrality (SOAP can operate over any protocol such as HTTP, SMTP, TCP, UDP, or JMS)
 - independence (SOAP allows for any programming model)

SOAP evolved as a successor of XML-RPC

[The Web Services Description Language](https://en.wikipedia.org/wiki/Web_Services_Description_Language) (WSDL /ˈwɪz dəl/) is an XML-based interface description language that is used for describing the functionality offered by a web service. The acronym is also used for any specific WSDL description of a web service (also referred to as a WSDL file), which provides a machine-readable description of how the service can be called, what parameters it expects, and what data structures it returns. Therefore, its purpose is roughly similar to that of a type signature in a programming language.

---

## Remote Procedure Calls - new challenges

... then **cloud** happened, with a new set of requirements:
 - Interoperability
 - Distributed
 - From micro to monoliths
 - Significant loads
 - Web, mobile & IoT

???

New breed of RPC frameworks 
 - JSON-RPC
 - Apache Thrift
 - Apache Avro
	
*While mostly using HTTP, most of them lack streaming support*

---

## HTTP & REST APIs

Now it's a default choice
 - Web is built around HTTP & REST
 - HTTP verbs (GET, PUT, DELETE) are reach enough and Map well to CRUD
 - Well understood
 - Interoperable
 - Loose coupling between client & server
 - Readable

--

Not great
 - Loose coupling
 - Textual representation
 - Modelling operations & RESTful constraints
 - Versioning, documentation
 - Streaming (bidirectional even not possible)

???

Swagger should be mentioned as a way to document & publish API.

---

### gRPC: Motivation and Design Principles

General-purpose, uniform and cross-platform RPC infrastructure

 - Free & Open
 - Services not Objects, Messages not References
 - Coverage & Simplicity
 - Performance, Interoperability & Reach
 - Payload Agnostic
 - First class streaming support
 - Support blocking & non-blocking clients
 - Plugable architecture for security, health checks, metrics, etc

And some more at https://grpc.io/blog/principles

---

### gRPC: How Does It Work?

A high level **service definition** to describe the API using Protocol Buffers

--

Client and Server code generated from the definition in **10+ languages** and multiple platforms

--

Strongly typed message definitions and operations

--

Efficiency in serialization with `protobuf` and connection with **HTTP/2**

--

**Connection options**: Unary, server-side streaming, client-side streaming, bi-directional streaming

--

**Authentication options**: SSL/TLS, token based authentication

---

### Protocol Buffers, gRPC's IDL

Language-neutral, platform-neutral, extensible mechanism for describing service interactions and serializing structured data

--

```
syntax = "proto3";

message Person {
  string name = 1;
  int32 id = 2;         // Unique ID number for this person.
  string email = 3;

  enum PhoneType {
    MOBILE = 0;
    HOME = 1;
    WORK = 2;
  }

  message PhoneNumber {
    string number = 1;
    PhoneType type = 2;
  }

  repeated PhoneNumber phones = 4;
}
```

---

### gRPC Development Flow

1. Define services using IDL

- Server side
    - generate service interfaces
    - create service implementation
    - wire up
    - run

- Client side
    - generate stubs
    - create channel
    - call server

---

### Let's create a service! (1)

Service definition:

```
syntax = "proto3";

package SimpleRequestResponse;

message HelloRequest {
  string Name = 1;
}
  
message HelloResponse {
  string Reply = 1;
}

service GreeterService {
  rpc SayHello (HelloRequest) returns (HelloResponse);
}
```

---

### Let's create a service! (2)

Generating server side code

```xml
  <ItemGroup>
    <Protobuf Include="..\*.proto" GrpcServices="Server" />
    <Content Include="@(Protobuf)" LinkBase="..\" />
  </ItemGroup>
```
--

```xml
  <ItemGroup>
    <PackageReference Include="Google.Protobuf" Version="3.7.0" />
    <PackageReference Include="Grpc.Core" Version="1.19.0" />
    <PackageReference Include="Grpc.Tools" Version="1.19.0" PrivateAssets="All" />
  </ItemGroup>
```

---

### Let's create a service! (3) <small>Server implementation</small>

```csharp
public class GreeterServiceImplementation : GreeterService.GreeterServiceBase
{
    public override Task<HelloResponse> SayHello(
        HelloRequest request, 
        ServerCallContext context)
    {
        return Task.FromResult(new HelloResponse
        {
            Reply = "Hello " + request.Name
        });
    }
}
```
--

```csharp
var server = new Grpc.Core.Server
{
    Services = { GreeterService.BindService(new GreeterServiceImplementation()) },
    Ports = { new ServerPort("localhost", 5000, ServerCredentials.Insecure) }
};
server.Start();
Console.ReadKey();
await server.ShutdownAsync();
```

---

### Let's create a service! (3) <small>Generating client code</small>

```xml
  <ItemGroup>
    <Protobuf Include="..\*.proto" GrpcServices="Client" />
    <Content Include="@(Protobuf)" LinkBase="..\" />
  </ItemGroup>
```

```xml
  <ItemGroup>
    <PackageReference Include="Google.Protobuf" Version="3.7.0" />
    <PackageReference Include="Grpc.Core" Version="1.19.0" />
    <PackageReference Include="Grpc.Tools" Version="1.19.0" PrivateAssets="All" />
  </ItemGroup>
```

---

### Let's create a service! (3) <small>Calling the server</small>

```csharp
var channel = new Channel("localhost:5000", ChannelCredentials.Insecure);
var client = new GreeterService.GreeterServiceClient(channel);

var reply = await client.SayHelloAsync(new HelloRequest { Name = "Client" });
Console.WriteLine("Reply: " + reply.Reply);

await channel.ShutdownAsync();
```

---

class: center, middle

## DEMO 1

### ...to prove that it actually works

???

Show in Visual Studio:
 - generated code
 - sync/async operations
 - additional request parameters

---

### TODO: Languages & Platforms

---

### TODO: HTTP/2

---

### gRPC Communication Styles

- Unary request/response
- Server-side streaming
- Client-side streaming
- Bi-directional streaming


```
service Examples {

  // Unary request/response
  rpc GetData(Request) returns (Response);
  
  // Server-side streaming
  rpc GetWeatherForecast(PositionRequest) returns (stream WeatherResponse);
  
  // Client-side streaming
  rpc FillForm(stream UpdateRequest) returns (CompletedResponse);
  
  // Bi-directional streaming
  rpc Chat (stream ChatRequest) returns (stream ChatNotificationResponse);
}
```

???
https://grpc.io/docs/guides/concepts.html#rpc-life-cycle

---

### gRPC Status codes and Error Handling

- HTTP status code `200` (unless cannot reach the server)
- gRPC header `grpc-status` 
- additional `grpc-messsage` to provide error description
- standardized gRPC startus codes for all platforms


Some of the codes:

    OK, CANCELLED, UNKNOWN, INVALID_ARGUMENT, DEADLINE_EXCEEDED,
    NOT_FOUND, PERMISSION_DENIED, UNAUTHENTICATED, UNIMPLEMENTED

Some of them are generated by the library itself, other might be specified by applications

???

Errors are surfaced in .NET as `RpcException`

> Why am I receiving a GOAWAY with error code ENHANCE_YOUR_CALM?

---

class: center, middle

## DEMO 2

### Bi-directional streaming

---

### gRPC & ASP .NET Core

**It's a work in progress**

The plan: fully-managed version of gRPC for .NET that will be built on top of ASP.NET Core HTTP/2 server

Advantages:
 - Using same application level facilities (configuration, dependency injection)
 - Same extensibility models (pipelines)
 - Inherits all the performance work in ASP .NET Core & Kestrel

Managed server is coming in ASP .NET Core 3.0 timeframe, client _might_ come at same time

Follow the development: https://github.com/grpc/grpc-dotnet

---

### gRPC on the battlefield

Extensibility mechanisms enable integration with applications & system used to support real life use

- Load balancing & proxying: Envoy, NGINX, grpclb
- Monitoring and tracing: Prometheus, OpenTracing
- Health checking: Consul, Kubernetes

---

### Real World Use: Customizations (1)

A layer on top of gRPC, called `Esperanto`

Service and DTO definitions in XML:
  * More validation rules
  * Semantic versioning
  * Transport agnostic

--

The flow:
 - Esperanto's XML
 - Generating
  1. `.proto` file(s)
  2. code using `gRPC tools`
  3. Application level interfaces (client & server)
  4. Client and service implementations 

---

### Real World Use: Customizations (2)

Custom code-gen pipeline & service hosting facilities
  - Multiple clients (.NET, TypeScript) and platform specific packages (nuget, npm)
  - RX (Reactive eXtensions) API for client implementations.
  - Authentication & Authorization mechanisms
  - Enforcing semantic versioning  of the contracts
  - Structured logging (connection and call level) using `Serilog`
  - Additional context (correlation ids, instance identifiers, locale, time zones)
  - Data type conventions (immutability, nullability, formatting/precision)
  - DTOs & DTO Builders
  - Specific data constructs (Replicated Sets)
  - Service Interfaces to abstract from RPC

Still compatible with vanilla gRPC: `.proto` files are generated and used by third-party clients.

---
