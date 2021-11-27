# DNS

A DNS library written in C# targeting .NET Standard 2.0. Versions prior to version two (2.0.0) were written for .NET 4 using blocking network operations. Version two and above use asynchronous operations.

Available through NuGet.

	Install-Package DNS

[![Test](https://github.com/kapetan/dns/actions/workflows/test.yml/badge.svg)](https://github.com/kapetan/dns/actions/workflows/test.yml)

# Usage

The library exposes a `Request` and `Response` classes for parsing and creating DNS messages. These can be serialized to byte arrays.

```C#
Request request = new Request();

request.RecursionDesired = true;
request.Id = 123;

UdpClient udp = new UdpClient();
IPEndPoint google = new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53);

// Send to google's DNS server
await udp.SendAsync(request.ToArray(), request.Size, google);

UdpReceiveResult result = await udp.ReceiveAsync();
byte[] buffer = result.Buffer;
Response response = Response.FromArray(buffer);

// Outputs a human readable representation
Console.WriteLine(response);
```

### Client

The libray also includes a small client and a proxy server. Using the `ClientRequest` or the `DnsClient` class it is possible to send a request to a Domain Name Server. The request is first sent using UDP, if that fails (response is truncated), the request is sent again using TCP. This behaviour can be changed by supplying an `IRequestResolver` to the client constructor.

```C#
ClientRequest request = new ClientRequest("8.8.8.8");

// Request an IPv6 record for the foo.com domain
request.Questions.Add(new Question(Domain.FromString("foo.com"), RecordType.AAAA));
request.RecursionDesired = true;

ClientResponse response = await request.Resolve();

// Get all the IPs for the foo.com domain
IList<IPAddress> ips = response.AnswerRecords
	.Where(r => r.Type == RecordType.AAAA)
	.Cast<IPAddressResourceRecord>()
	.Select(r => r.IPAddress)
	.ToList();
```

The `DnsClient` class contains some conveniance methods for creating instances of `ClientRequest` and resolving domains.

```C#
// Bind to a Domain Name Server
DnsClient client = new DnsClient("8.8.8.8");

// Create request bound to 8.8.8.8
ClientRequest request = client.Create();

// Returns a list of IPs
IList<IPAddress> ips = await client.Lookup("foo.com");

// Get the domain name belonging to the IP (google.com)
string domain = await client.Reverse("173.194.69.100");
```

### Server

The `DnsServer` class exposes a proxy Domain Name Server (UDP only). You can intercept domain name resolution requests and route them to specified IPs. The server is asynchronous. It also emits an event on every request and every successful resolution.

```C#
// Proxy to google's DNS
MasterFile masterFile = new MasterFile();
DnsServer server = new DnsServer(masterFile, "8.8.8.8");

// Resolve these domain to localhost
masterFile.AddIPAddressResourceRecord("google.com", "127.0.0.1");
masterFile.AddIPAddressResourceRecord("github.com", "127.0.0.1");

// Log every request
server.Requested += (sender, e) => Console.WriteLine(e.Request);
// On every successful request log the request and the response
server.Responded += (sender, e) => Console.WriteLine("{0} => {1}", e.Request, e.Response);
// Log errors
server.Errored += (sender, e) => Console.WriteLine(e.Exception.Message);

// Start the server (by default it listens on port 53)
await server.Listen();
```

Depending on the application setup the events might be executed on a different thread than the calling thread.

It's also possible to modify the `request` instance in the `server.Requested` callback.

### Request Resolver

The `DnsServer`, `DnsClient` and `ClientRequest` classes also accept an instance implementing the `IRequestResolver` interface, which they internally use to resolve DNS requests. Some of the default implementations are `UdpRequestResolver`, `TcpRequestResolver` and `MasterFile` classes. But it's also possible to provide a custom request resolver.

```C#
// A request resolver that resolves all dns queries to localhost
public class LocalRequestResolver : IRequestResolver {
	public Task<IResponse> Resolve(IRequest request) {
		IResponse response = Response.FromRequest(request);

		foreach (Question question in response.Questions) {
			if (question.Type == RecordType.A) {
				IResourceRecord record = new IPAddressResourceRecord(
					question.Name, IPAddress.Parse("127.0.0.1"));
				response.AnswerRecords.Add(record);
			}
		}

		return Task.FromResult(response);
	}
}

// All dns requests received will be handled by the localhost request resolver
DnsServer server = new DnsServer(new LocalRequestResolver());

await server.Listen();
```
