# DNS

A DNS library written in C#. It needs .NET 4 to run.

# Usage

The library exposes a `Request` and `Response` classes for parsing and creating DNS messages. These can be serialzed to byte arrays.

```C#
Request request = new Request();

request.RecursionDesired = true;
request.Id = 123;

UdpClient udp = new UdpClient();
IPEndPoint google = new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53);

// Connect to google's DNS server
udp.Connect(google);
udp.Send(request.ToArray(), request.Size);

byte[] buffer = udp.Receive(ref google);
Response response = Response.FromArray(buffer);

// Outputs an human readable representation
Console.WriteLine(response);
```

The libray also includes a small client and a proxy server. Using the `ClientRequest` or the `Client` class it is possible to send a request to a Domain Name Server.

```C#
ClientRequest request = new ClientRequest("8.8.8.8");

// Request an IPv6 record for the foo.com domain
request.AddQuestion(new Question("foo.com", RecordType.AAAA));
request.RecursionDesired = true;

ClientResponse response = request.Resolve();

// Get all the IPs for the foo.com domain
IList<IPAddress> ips = response.AnswerRecords
	.Where(r => r.Type == RecordType.AAAA)
	.Cast<IPAddressResourceRecord>()
	.Select(r => r.IPAddress)
	.ToList();
```

The `Client` class contains some conveniance methods for creating instances of `ClientRequest` and resolving domains.

```C#
// Bind to a Domain Name Server
Client client = new Client("8.8.8.8");

// Create request bound to 8.8.8.8
ClientRequest request = client.Create();

// Returns a list of IPs
client.Resolve("foo.com");

// Get the domain name belonging to the IP (google.com)
client.Reverse("173.194.69.100");
```