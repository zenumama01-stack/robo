    /// WebResponseObject.
    public class WebResponseObject
        /// Gets or sets the BaseResponse property.
        public HttpResponseMessage BaseResponse { get; set; }
        /// Gets or protected sets the response body content.
        public byte[]? Content { get; protected set; }
        /// Gets the Headers property.
        public Dictionary<string, IEnumerable<string>> Headers => _headers ??= WebResponseHelper.GetHeadersDictionary(BaseResponse);
        private Dictionary<string, IEnumerable<string>>? _headers;
        /// Gets or protected sets the full response content.
        /// Full response content, including the HTTP status line, headers, and body.
        public string? RawContent { get; protected set; }
        /// Gets the length (in bytes) of <see cref="RawContentStream"/>.
        public long RawContentLength => RawContentStream is null ? -1 : RawContentStream.Length;
        /// Gets or protected sets the response body content as a <see cref="MemoryStream"/>.
        public MemoryStream RawContentStream { get; protected set; }
        /// Gets the RelationLink property.
        public Dictionary<string, string>? RelationLink { get; internal set; }
        /// Gets the response status code.
        public int StatusCode => WebResponseHelper.GetStatusCode(BaseResponse);
        /// Gets the response status description.
        public string StatusDescription => WebResponseHelper.GetStatusDescription(BaseResponse);
        /// Gets or sets the output file path.
        public string? OutFile { get; internal set; }
        #region Protected Fields
        /// Time permitted between reads or Timeout.InfiniteTimeSpan for no timeout.
        protected TimeSpan perReadTimeout;
        #endregion Protected Fields
        /// Initializes a new instance of the <see cref="WebResponseObject"/> class.
        /// <param name="response">The Http response.</param>
        public WebResponseObject(HttpResponseMessage response, TimeSpan perReadTimeout, CancellationToken cancellationToken) : this(response, null, perReadTimeout, cancellationToken) { }
        /// Initializes a new instance of the <see cref="WebResponseObject"/> class
        /// <param name="response">Http response.</param>
        /// <param name="contentStream">The http content stream.</param>
        public WebResponseObject(HttpResponseMessage response, Stream? contentStream, TimeSpan perReadTimeout, CancellationToken cancellationToken)
            this.perReadTimeout = perReadTimeout;
            SetResponse(response, contentStream, cancellationToken);
            InitializeContent();
        private void InitializeContent()
            Content = RawContentStream.ToArray();
            // Use ASCII encoding for the RawContent visual view of the content.
            if (Content?.Length > 0)
                raw.Append(ToString());
        private static bool IsPrintable(char c) => char.IsLetterOrDigit(c)
                                                || char.IsPunctuation(c)
                                                || char.IsSeparator(c)
                                                || char.IsSymbol(c)
                                                || char.IsWhiteSpace(c);
        [MemberNotNull(nameof(RawContentStream))]
        [MemberNotNull(nameof(BaseResponse))]
        private void SetResponse(HttpResponseMessage response, Stream? contentStream, CancellationToken cancellationToken)
            BaseResponse = response;
            if (contentStream is MemoryStream ms)
                RawContentStream = ms;
                Stream st = contentStream ?? StreamHelper.GetResponseStream(response, cancellationToken);
                long contentLength = response.Content.Headers.ContentLength.GetValueOrDefault();
                if (contentLength <= 0)
                    contentLength = StreamHelper.DefaultReadBuffer;
                int initialCapacity = (int)Math.Min(contentLength, StreamHelper.DefaultReadBuffer);
                RawContentStream = new WebResponseContentMemoryStream(st, initialCapacity, cmdlet: null, response.Content.Headers.ContentLength.GetValueOrDefault(), perReadTimeout, cancellationToken);
            // Set the position of the content stream to the beginning
            RawContentStream.Position = 0;
        /// Returns the string representation of this web response.
        /// <returns>The string representation of this web response.</returns>
        public sealed override string ToString()
            if (Content is null)
            char[] stringContent = Encoding.ASCII.GetChars(Content);
            for (int counter = 0; counter < stringContent.Length; counter++)
                if (!IsPrintable(stringContent[counter]))
                    stringContent[counter] = '.';
            return new string(stringContent);
