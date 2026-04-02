    internal sealed class GzipFilter : ResultFilterAttribute
                using (var compressedStream = new GZipStream(responseStream, CompressionLevel.Fastest))
                    httpContext.Response.Headers.Append("Content-Encoding", new[] { "gzip" });
