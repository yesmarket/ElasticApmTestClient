using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Resource.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace ElasticApmTestClient;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        //args[0] should be the elastic APM instance id which is the lowest level sub-domain in the request URI.
        //args[1] should be the elastic APM bearer token.

        var tracesData = GetTracesData();

        var httpClient = new HttpClient();

        using var stream = new MemoryStream();
        tracesData.WriteTo(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");
        
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"https://{args[0]}.apm.ap-southeast-2.aws.cloud.es.io:443/v1/traces"),
            Headers = {
                { "Authorization", $"Bearer {args[1]}" },
            },
            Content = streamContent,
        };

        var response = await httpClient.SendAsync(httpRequestMessage);

        var responseStream = await response.Content.ReadAsStringAsync();

        Console.WriteLine(responseStream);

        return 0;
    }

    private static TracesData GetTracesData()
    {
        var dateTime = DateTime.UtcNow;
        var traceId = ByteString.CopyFromUtf8(GetHexIdentifier("trace-id", 16));
        var parentSpanId = ByteString.CopyFromUtf8(GetHexIdentifier("parent-span-id", 8));

        return new TracesData
        {
            ResourceSpans =
            {
                new[]
                {
                    new ResourceSpans
                    {
                        Resource = new Resource
                        {
                            Attributes =
                            {
                                new[]
                                {
                                    new KeyValue
                                    {
                                        Key = "service.name",
                                        Value = new AnyValue
                                        {
                                            StringValue = "manual-test"
                                        }
                                    },
                                    new KeyValue
                                    {
                                        Key = "service.version",
                                        Value = new AnyValue
                                        {
                                            StringValue = "1.0.0"
                                        }
                                    },
                                    new KeyValue
                                    {
                                        Key = "service.instance.id",
                                        Value = new AnyValue
                                        {
                                            StringValue = "9C15200B-57CC-4E70-B090-3A88F425E705"
                                        }
                                    },
                                    new KeyValue
                                    {
                                        Key = "deployment.environment",
                                        Value = new AnyValue
                                        {
                                            StringValue = "production"
                                        }
                                    }
                                }
                            }
                        },
                        ScopeSpans =
                        {
                            new[]
                            {
                                new ScopeSpans
                                {
                                    Scope = new InstrumentationScope(),
                                    Spans =
                                    {
                                        new[]
                                        {
                                            new Span
                                            {
                                                TraceId = traceId,
                                                SpanId = ByteString.CopyFromUtf8(GetHexIdentifier("nested-span-id-2", 8)),
                                                ParentSpanId = parentSpanId,
                                                Name = "Operation-3",
                                                Kind = Span.Types.SpanKind.Server,
                                                StartTimeUnixNano = GetUnixTimeNano(dateTime, 6),
                                                EndTimeUnixNano = GetUnixTimeNano(dateTime, 9),
                                                Status = new Status {Code = Status.Types.StatusCode.Ok}
                                            },
                                            new Span
                                            {
                                                TraceId = traceId,
                                                SpanId = ByteString.CopyFromUtf8(GetHexIdentifier("nested-span-id-1", 8)),
                                                ParentSpanId = parentSpanId,
                                                Name = "Operation-2",
                                                Kind = Span.Types.SpanKind.Server,
                                                StartTimeUnixNano = GetUnixTimeNano(dateTime, 2),
                                                EndTimeUnixNano = GetUnixTimeNano(dateTime, 5),
                                                Status = new Status {Code = Status.Types.StatusCode.Ok}
                                            },
                                            new Span
                                            {
                                                TraceId = traceId,
                                                SpanId = parentSpanId,
                                                Name = "Operation-1",
                                                Kind = Span.Types.SpanKind.Server,
                                                StartTimeUnixNano = GetUnixTimeNano(dateTime, 1),
                                                EndTimeUnixNano = GetUnixTimeNano(dateTime, 10),
                                                Status = new Status {Code = Status.Types.StatusCode.Ok},
                                                Events =
                                                {
                                                    new[]
                                                    {
                                                        new Span.Types.Event
                                                        {
                                                            Name = "Event-1",
                                                            TimeUnixNano = GetUnixTimeNano(dateTime, 3)
                                                        }
                                                    }
                                                }
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
    }

    private static ulong GetUnixTimeNano(DateTime dateTime, int seconds)
    {
        var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (ulong)((dateTime.AddSeconds(seconds) - epochStart).Ticks * 100);
    }

    private static string GetHexIdentifier(string input, int length)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var inputHash = SHA256.HashData(inputBytes);
        var hexString = Convert.ToHexString(inputHash).Substring(0, length);
        return hexString;
    }
}

