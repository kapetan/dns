# DNS

A DNS library written in C#. It needs .NET 4 to run.

# Usage

The library exposes a Request and Response object for parsing and creating DNS messages. These can be serialzed to byte arrays.

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
