using System.Net.Mime;
using RangeItemHeaderValue = System.Net.Http.Headers.RangeItemHeaderValue;
using RangeHeaderValue = System.Net.Http.Headers.RangeHeaderValue;
    public class ResumeController : Controller
        private static readonly byte[] FileBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
        public async void Index()
            SetResumeResponseHeaders();
            string rangeHeader;
            int from = 0;
            int to = FileBytes.Length - 1;
            if (TryGetRangeHeader(out rangeHeader))
                var range = GetRange(rangeHeader);
                if (range.From != null)
                    from = (int)range.From;
                if (range.To != null)
                    to = (int)range.To;
                Response.ContentType = MediaTypeNames.Application.Octet;
                await Response.Body.WriteAsync(FileBytes, 0, FileBytes.Length);
            if (to >= FileBytes.Length || from >= FileBytes.Length)
                Response.StatusCode = StatusCodes.Status416RequestedRangeNotSatisfiable;
                Response.Headers[HeaderNames.ContentRange] = $"bytes */{FileBytes.Length}";
                Response.ContentLength = to - from + 1;
                Response.Headers[HeaderNames.ContentRange] = $"bytes {from}-{to}/{FileBytes.Length}";
                Response.StatusCode = StatusCodes.Status206PartialContent;
                await Response.Body.WriteAsync(FileBytes, from, (int)Response.ContentLength);
        public async void NoResume()
            Response.ContentLength = FileBytes.Length;
        public async void Bytes(int NumberBytes)
            if (NumberBytes > FileBytes.Length || NumberBytes < 0)
                NumberBytes = FileBytes.Length;
            Response.ContentLength = NumberBytes;
            await Response.Body.WriteAsync(FileBytes, 0, NumberBytes);
        private static RangeItemHeaderValue GetRange(string rangeHeader)
            return RangeHeaderValue.Parse(rangeHeader).Ranges.FirstOrDefault();
        private void SetResumeResponseHeaders()
                Response.Headers["X-WebListener-Has-Range"] = "true";
                Response.Headers["X-WebListener-Request-Range"] = rangeHeader;
                Response.Headers["X-WebListener-Has-Range"] = "false";
        private bool TryGetRangeHeader(out string rangeHeader)
            var rangeHeaderSv = new StringValues();
            if (Request.Headers.TryGetValue("Range", out rangeHeaderSv))
                rangeHeader = rangeHeaderSv.FirstOrDefault();
                rangeHeader = string.Empty;
