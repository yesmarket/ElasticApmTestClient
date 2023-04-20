using Google.Protobuf;
using OpenTelemetry.Proto.Trace.V1;

namespace ElasticApmTestClient;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var tracesData = new TracesData
        {
            ResourceSpans =
            {
                new ResourceSpans[]
                {
                    new ResourceSpans
                    {
                        ScopeSpans =
                        {
                            new ScopeSpans[]
                            {
                                new ScopeSpans
                                {
                                    Spans =
                                    {
                                        new Span[]
                                        {
                                            new Span
                                            {
                                                TraceId = ByteString.CopyFromUtf8("de9bf12b-e222-4742-97eb-605bde1a12fb"),
                                                SpanId = ByteString.CopyFromUtf8("652a2cfd-c898-4f89-a16b-ff5b13f3c1fa"),
                                                Name = "test",
                                                StartTimeUnixNano = GetUnixTime(),
                                                EndTimeUnixNano = GetUnixTime()
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        

        var httpClient = new HttpClient();

        using var stream = new MemoryStream();
        tracesData.WriteTo(stream);
        var streamContent = new StreamContent(stream);

        // stream.Seek(0, SeekOrigin.Begin);
        //
        // await using (var fs = new FileStream("test.dat", FileMode.OpenOrCreate))
        // {
        //     await stream.CopyToAsync(fs);
        // }

        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(args[0]),
            Headers = {
                { "Authorization", $"ApiKey {args[1]}" },
                { "kbn-xsrf", "true" },
                //{ "Content-Type", "application/protobuf" }
            },
            Content = streamContent
        };

        var response = await httpClient.SendAsync(httpRequestMessage);

        var responseStream = await response.Content.ReadAsStringAsync();

        Console.WriteLine(responseStream);

        return 0;
    }

    private static ulong GetUnixTime()
    {
        var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (ulong)((DateTime.UtcNow - epochStart).Ticks * 100);
    }
}

