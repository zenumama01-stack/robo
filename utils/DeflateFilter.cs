    internal sealed class DeflateFilter : ResultFilterAttribute
                using (var compressedStream = new DeflateStream(responseStream, CompressionLevel.Fastest))
                    httpContext.Response.Headers.Append("Content-Encoding", new[] { "deflate" });
