    /// The Invoke-WebRequest command.
    /// This command makes an HTTP or HTTPS request to a web server and returns the results.
    [Cmdlet(VerbsLifecycle.Invoke, "WebRequest", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097126", DefaultParameterSetName = "StandardMethod")]
    [OutputType(typeof(BasicHtmlWebResponseObject))]
    public class InvokeWebRequestCommand : WebRequestPSCmdlet
        /// Initializes a new instance of the <see cref="InvokeWebRequestCommand"/> class.
        public InvokeWebRequestCommand() : base()
            _parseRelLink = true;
                // Creating a MemoryStream wrapper to response stream here to support IsStopping.
                responseStream = new WebResponseContentMemoryStream(
                    responseStream,
                    StreamHelper.ChunkSize,
                    response.Content.Headers.ContentLength.GetValueOrDefault(),
                    perReadTimeout,
                    _cancelToken.Token);
                WebResponseObject ro = WebResponseHelper.IsText(response) ? new BasicHtmlWebResponseObject(response, responseStream, perReadTimeout, _cancelToken.Token) : new WebResponseObject(response, responseStream, perReadTimeout, _cancelToken.Token);
                ro.RelationLink = _relationLink;
                ro.OutFile = outFilePath;
                WriteObject(ro);
                // Use the rawcontent stream from WebResponseObject for further
                // processing of the stream. This is need because WebResponse's
                // stream can be used only once.
                responseStream = ro.RawContentStream;
                // ContentLength is always the partial length, while ContentRange is the full length
                // Without Request.Range set, ContentRange is null and partial length (ContentLength) equals to full length
                StreamHelper.SaveStreamToFile(responseStream, outFilePath, this, response.Content.Headers.ContentRange?.Length.GetValueOrDefault() ?? response.Content.Headers.ContentLength.GetValueOrDefault(), perReadTimeout, _cancelToken.Token);
