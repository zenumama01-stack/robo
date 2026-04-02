    /// The valid values for the -Authentication parameter for Invoke-RestMethod and Invoke-WebRequest.
    public enum WebAuthenticationType
        /// No authentication. Default.
        /// RFC-7617 Basic Authentication. Requires -Credential.
        Basic,
        /// RFC-6750 OAuth 2.0 Bearer Authentication. Requires -Token.
        Bearer,
        OAuth,
    // WebSslProtocol is used because not all SslProtocols are supported by HttpClientHandler.
    // Also SslProtocols.Default is not the "default" for HttpClientHandler as SslProtocols.Ssl3 is not supported.
    /// The valid values for the -SslProtocol parameter for Invoke-RestMethod and Invoke-WebRequest.
    public enum WebSslProtocol
        /// No SSL protocol will be set and the system defaults will be used.
        Default = SslProtocols.None,
        /// Specifies the TLS 1.0 is obsolete. Using this value now defaults to TLS 1.2.
        Tls = SslProtocols.Tls12,
        /// Specifies the TLS 1.1 is obsolete. Using this value now defaults to TLS 1.2.
        Tls11 = SslProtocols.Tls12,
        /// Specifies the TLS 1.2 security protocol. The TLS protocol is defined in IETF RFC 5246.
        Tls12 = SslProtocols.Tls12,
        /// Specifies the TLS 1.3 security protocol. The TLS protocol is defined in IETF RFC 8446.
        Tls13 = SslProtocols.Tls13
    /// Base class for Invoke-RestMethod and Invoke-WebRequest commands.
    public abstract class WebRequestPSCmdlet : PSCmdlet, IDisposable
        /// Used to prefix the headers in debug and verbose messaging.
        internal const string DebugHeaderPrefix = "--- ";
        /// Cancellation token source.
        internal CancellationTokenSource _cancelToken = null;
        /// Automatically follow Rel Links.
        internal bool _followRelLink = false;
        /// Maximum number of Rel Links to follow.
        internal int _maximumFollowRelLink = int.MaxValue;
        /// Maximum number of Redirects to follow.
        internal int _maximumRedirection;
        /// Parse Rel Links.
        internal bool _parseRelLink = false;
        internal Dictionary<string, string> _relationLink = null;
        /// The current size of the local file being resumed.
        private long _resumeFileSize = 0;
        /// The remote endpoint returned a 206 status code indicating successful resume.
        private bool _resumeSuccess = false;
        /// True if the Dispose() method has already been called to cleanup Disposable fields.
        #region Virtual Properties
        #region URI
        /// Deprecated. Gets or sets UseBasicParsing. This has no affect on the operation of the Cmdlet.
        public virtual SwitchParameter UseBasicParsing { get; set; } = true;
        /// Gets or sets the Uri property.
        public virtual Uri Uri { get; set; }
        #endregion URI
        #region HTTP Version
        /// Gets or sets the HTTP Version property.
        [ArgumentToVersionTransformation]
        [HttpVersionCompletions]
        public virtual Version HttpVersion { get; set; }
        #endregion HTTP Version
        #region Session
        /// Gets or sets the Session property.
        public virtual WebRequestSession WebSession { get; set; }
        /// Gets or sets the SessionVariable property.
        [Alias("SV")]
        public virtual string SessionVariable { get; set; }
        #endregion Session
        #region Authorization and Credentials
        /// Gets or sets the AllowUnencryptedAuthentication property.
        public virtual SwitchParameter AllowUnencryptedAuthentication { get; set; }
        /// Gets or sets the Authentication property used to determine the Authentication method for the web session.
        /// Authentication does not work with UseDefaultCredentials.
        /// Authentication over unencrypted sessions requires AllowUnencryptedAuthentication.
        /// Basic: Requires Credential.
        /// OAuth/Bearer: Requires Token.
        public virtual WebAuthenticationType Authentication { get; set; } = WebAuthenticationType.None;
        /// Gets or sets the Credential property.
        public virtual PSCredential Credential { get; set; }
        public virtual SwitchParameter UseDefaultCredentials { get; set; }
        /// Gets or sets the CertificateThumbprint property.
        public virtual string CertificateThumbprint { get; set; }
        /// Gets or sets the Certificate property.
        public virtual X509Certificate Certificate { get; set; }
        /// Gets or sets the SkipCertificateCheck property.
        public virtual SwitchParameter SkipCertificateCheck { get; set; }
        /// Gets or sets the TLS/SSL protocol used by the Web Cmdlet.
        public virtual WebSslProtocol SslProtocol { get; set; } = WebSslProtocol.Default;
        /// Gets or sets the Token property. Token is required by Authentication OAuth and Bearer.
        public virtual SecureString Token { get; set; }
        #endregion Authorization and Credentials
        #region Headers
        public virtual string UserAgent { get; set; }
        /// Gets or sets the DisableKeepAlive property.
        public virtual SwitchParameter DisableKeepAlive { get; set; }
        /// Gets or sets the ConnectionTimeoutSeconds property.
        /// This property applies to sending the request and receiving the response headers only.
        public virtual int ConnectionTimeoutSeconds { get; set; }
        /// Gets or sets the OperationTimeoutSeconds property.
        /// This property applies to each read operation when receiving the response body.
        public virtual int OperationTimeoutSeconds { get; set; }
        /// Gets or sets the Headers property.
        public virtual IDictionary Headers { get; set; }
        /// Gets or sets the SkipHeaderValidation property.
        /// This property adds headers to the request's header collection without validation.
        public virtual SwitchParameter SkipHeaderValidation { get; set; }
        #endregion Headers
        #region Redirect
        /// Gets or sets the AllowInsecureRedirect property used to follow HTTP redirects from HTTPS.
        public virtual SwitchParameter AllowInsecureRedirect { get; set; }
        /// Gets or sets the RedirectMax property.
        public virtual int MaximumRedirection { get; set; } = -1;
        /// Gets or sets the MaximumRetryCount property, which determines the number of retries of a failed web request.
        public virtual int MaximumRetryCount { get; set; }
        /// Gets or sets the PreserveAuthorizationOnRedirect property.
        /// This property overrides compatibility with web requests on Windows.
        /// On FullCLR (WebRequest), authorization headers are stripped during redirect.
        /// CoreCLR (HTTPClient) does not have this behavior so web requests that work on
        /// PowerShell/FullCLR can fail with PowerShell/CoreCLR. To provide compatibility,
        /// we'll detect requests with an Authorization header and automatically strip
        /// the header when the first redirect occurs. This switch turns off this logic for
        /// edge cases where the authorization header needs to be preserved across redirects.
        public virtual SwitchParameter PreserveAuthorizationOnRedirect { get; set; }
        /// Gets or sets the RetryIntervalSec property, which determines the number seconds between retries.
        public virtual int RetryIntervalSec { get; set; } = 5;
        #endregion Redirect
        #region Method
        /// Gets or sets the Method property.
        [Parameter(ParameterSetName = "StandardMethod")]
        [Parameter(ParameterSetName = "StandardMethodNoProxy")]
        public virtual WebRequestMethod Method { get; set; } = WebRequestMethod.Default;
        /// Gets or sets the CustomMethod property.
        [Parameter(Mandatory = true, ParameterSetName = "CustomMethod")]
        [Parameter(Mandatory = true, ParameterSetName = "CustomMethodNoProxy")]
        [Alias("CM")]
        public virtual string CustomMethod { get => _customMethod; set => _customMethod = value.ToUpperInvariant(); }
        private string _customMethod;
        /// Gets or sets the PreserveHttpMethodOnRedirect property.
        public virtual SwitchParameter PreserveHttpMethodOnRedirect { get; set; }
        /// Gets or sets the UnixSocket property.
        public virtual UnixDomainSocketEndPoint UnixSocket { get; set; }
        #endregion Method
        #region NoProxy
        /// Gets or sets the NoProxy property.
        [Parameter(Mandatory = true, ParameterSetName = "StandardMethodNoProxy")]
        public virtual SwitchParameter NoProxy { get; set; }
        #endregion NoProxy
        #region Proxy
        [Parameter(ParameterSetName = "CustomMethod")]
        public virtual Uri Proxy { get; set; }
        /// Gets or sets the ProxyCredential property.
        public virtual PSCredential ProxyCredential { get; set; }
        /// Gets or sets the ProxyUseDefaultCredentials property.
        public virtual SwitchParameter ProxyUseDefaultCredentials { get; set; }
        #endregion Proxy
        #region Input
        /// Gets or sets the Body property.
        public virtual object Body { get; set; }
        /// Dictionary for use with RFC-7578 multipart/form-data submissions.
        /// Keys are form fields and their respective values are form values.
        /// A value may be a collection of form values or single form value.
        public virtual IDictionary Form { get; set; }
        /// Gets or sets the ContentType property.
        public virtual string ContentType { get; set; }
        /// Gets or sets the TransferEncoding property.
        [ValidateSet("chunked", "compress", "deflate", "gzip", "identity", IgnoreCase = true)]
        public virtual string TransferEncoding { get; set; }
        /// Gets or sets the InFile property.
        public virtual string InFile { get; set; }
        /// Keep the original file path after the resolved provider path is assigned to InFile.
        private string _originalFilePath;
        #endregion Input
        /// Gets or sets the OutFile property.
        public virtual string OutFile { get; set; }
        /// Gets or sets the PassThrough property.
        public virtual SwitchParameter PassThru { get; set; }
        /// Resumes downloading a partial or incomplete file. OutFile is required.
        public virtual SwitchParameter Resume { get; set; }
        /// Gets or sets whether to skip checking HTTP status for error codes.
        public virtual SwitchParameter SkipHttpErrorCheck { get; set; }
        #endregion Output
        #endregion Virtual Properties
        #region Helper Properties
        internal string QualifiedOutFile => QualifyFilePath(OutFile);
        internal string _qualifiedOutFile;
        internal bool ShouldCheckHttpStatus => !SkipHttpErrorCheck;
        /// Determines whether writing to a file should Resume and append rather than overwrite.
        internal bool ShouldResume => Resume.IsPresent && _resumeSuccess;
        internal bool ShouldSaveToOutFile => !string.IsNullOrEmpty(OutFile);
        internal bool ShouldWriteToPipeline => !ShouldSaveToOutFile || PassThru;
        #endregion Helper Properties
        #region Abstract Methods
        /// Read the supplied WebResponse object and push the resulting output into the pipeline.
        /// <param name="response">Instance of a WebResponse object to be processed.</param>
        internal abstract void ProcessResponse(HttpResponseMessage response);
        #endregion Abstract Methods
        /// The main execution method for cmdlets derived from WebRequestPSCmdlet.
                // Set cmdlet context for write progress
                ValidateParameters();
                PrepareSession();
                // If the request contains an authorization header and PreserveAuthorizationOnRedirect is not set,
                // it needs to be stripped on the first redirect.
                bool keepAuthorizationOnRedirect = PreserveAuthorizationOnRedirect.IsPresent
                                                   && WebSession.Headers.ContainsKey(HttpKnownHeaderNames.Authorization);
                bool handleRedirect = keepAuthorizationOnRedirect || AllowInsecureRedirect || PreserveHttpMethodOnRedirect;
                HttpClient client = GetHttpClient(handleRedirect);
                int followedRelLink = 0;
                Uri uri = Uri;
                    if (followedRelLink > 0)
                        string linkVerboseMsg = string.Format(
                            WebCmdletStrings.FollowingRelLinkVerboseMsg,
                            uri.AbsoluteUri);
                        WriteVerbose(linkVerboseMsg);
                    using (HttpRequestMessage request = GetRequest(uri))
                        FillRequestStream(request);
                            _maximumRedirection = WebSession.MaximumRedirection;
                            using HttpResponseMessage response = GetResponse(client, request, handleRedirect);
                            bool _isSuccess = response.IsSuccessStatusCode;
                            // Check if the Resume range was not satisfiable because the file already completed downloading.
                            // This happens when the local file is the same size as the remote file.
                            if (Resume.IsPresent
                                && response.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable
                                && response.Content.Headers.ContentRange.HasLength
                                && response.Content.Headers.ContentRange.Length == _resumeFileSize)
                                _isSuccess = true;
                                WriteVerbose(string.Format(
                                    WebCmdletStrings.OutFileWritingSkipped,
                                    OutFile));
                                // Disable writing to the OutFile.
                                OutFile = null;
                            // Detect insecure redirection.
                            if (!AllowInsecureRedirect)
                                // We will skip detection if either of the URIs is relative, because the 'Scheme' property is not supported on a relative URI.
                                // If we have to skip the check, an error may be thrown later if it's actually an insecure https-to-http redirect.
                                bool originIsHttps = response.RequestMessage.RequestUri.IsAbsoluteUri && response.RequestMessage.RequestUri.Scheme == "https";
                                bool destinationIsHttp = response.Headers.Location is not null && response.Headers.Location.IsAbsoluteUri && response.Headers.Location.Scheme == "http";
                                if (originIsHttps && destinationIsHttp)
                                    ErrorRecord er = new(new InvalidOperationException(), "InsecureRedirection", ErrorCategory.InvalidOperation, request);
                                    er.ErrorDetails = new ErrorDetails(WebCmdletStrings.InsecureRedirection);
                            if (ShouldCheckHttpStatus && !_isSuccess)
                                    WebCmdletStrings.ResponseStatusCodeFailure,
                                    (int)response.StatusCode,
                                    response.ReasonPhrase);
                                HttpResponseException httpEx = new(message, response);
                                ErrorRecord er = new(httpEx, "WebCmdletWebResponseException", ErrorCategory.InvalidOperation, request);
                                string detailMsg = string.Empty;
                                    string contentType = ContentHelper.GetContentType(response);
                                    long? contentLength = response.Content.Headers.ContentLength;
                                    // We can't use ReadAsStringAsync because it doesn't have per read timeouts
                                    string characterSet = WebResponseHelper.GetCharacterSet(response);
                                    var responseStream = StreamHelper.GetResponseStream(response, _cancelToken.Token);
                                    int initialCapacity = (int)Math.Min(contentLength ?? StreamHelper.DefaultReadBuffer, StreamHelper.DefaultReadBuffer);
                                    var bufferedStream = new WebResponseContentMemoryStream(responseStream, initialCapacity, this, contentLength, perReadTimeout, _cancelToken.Token);
                                    string error = StreamHelper.DecodeStream(bufferedStream, characterSet, out Encoding encoding, perReadTimeout, _cancelToken.Token);
                                    detailMsg = FormatErrorMessage(error, contentType);
                                    // Catch all
                                    er.ErrorDetails = new ErrorDetails(ex.ToString());
                                if (!string.IsNullOrEmpty(detailMsg))
                                    er.ErrorDetails = new ErrorDetails(detailMsg);
                            if (_parseRelLink || _followRelLink)
                                ParseLinkHeader(response);
                            ProcessResponse(response);
                            UpdateSession(response);
                            // If we hit our maximum redirection count, generate an error.
                            // Errors with redirection counts of greater than 0 are handled automatically by .NET, but are
                            // impossible to detect programmatically when we hit this limit. By handling this ourselves
                            // (and still writing out the result), users can debug actual HTTP redirect problems.
                            if (_maximumRedirection == 0 && IsRedirectCode(response.StatusCode))
                                ErrorRecord er = new(new InvalidOperationException(), "MaximumRedirectExceeded", ErrorCategory.InvalidOperation, request);
                                er.ErrorDetails = new ErrorDetails(WebCmdletStrings.MaximumRedirectionCountExceeded);
                        catch (TimeoutException ex)
                            ErrorRecord er = new(ex, "OperationTimeoutReached", ErrorCategory.OperationTimeout, null);
                        catch (HttpRequestException ex)
                            ErrorRecord er = new(ex, "WebCmdletWebResponseException", ErrorCategory.InvalidOperation, request);
                            if (ex.InnerException is not null)
                            _cancelToken?.Dispose();
                            _cancelToken = null;
                        if (_followRelLink)
                            if (!_relationLink.ContainsKey("next"))
                            uri = new Uri(_relationLink["next"]);
                            followedRelLink++;
                while (_followRelLink && (followedRelLink < _maximumFollowRelLink));
            catch (CryptographicException ex)
                ErrorRecord er = new(ex, "WebCmdletCertificateException", ErrorCategory.SecurityError, null);
                ErrorRecord er = new(ex, "WebCmdletIEDomNotSupportedException", ErrorCategory.NotImplemented, null);
        protected override void StopProcessing() => _cancelToken?.Cancel();
        /// Disposes the associated WebSession if it is not being used as part of a persistent session.
                if (disposing && !IsPersistentSession())
                    WebSession?.Dispose();
                    WebSession = null;
        #region Virtual Methods
        internal virtual void ValidateParameters()
            // Sessions
            if (WebSession is not null && SessionVariable is not null)
                ErrorRecord error = GetValidationError(WebCmdletStrings.SessionConflict, "WebCmdletSessionConflictException");
            // Authentication
            if (UseDefaultCredentials && Authentication != WebAuthenticationType.None)
                ErrorRecord error = GetValidationError(WebCmdletStrings.AuthenticationConflict, "WebCmdletAuthenticationConflictException");
            if (Authentication != WebAuthenticationType.None && Token is not null && Credential is not null)
                ErrorRecord error = GetValidationError(WebCmdletStrings.AuthenticationTokenConflict, "WebCmdletAuthenticationTokenConflictException");
            if (Authentication == WebAuthenticationType.Basic && Credential is null)
                ErrorRecord error = GetValidationError(WebCmdletStrings.AuthenticationCredentialNotSupplied, "WebCmdletAuthenticationCredentialNotSuppliedException");
            if ((Authentication == WebAuthenticationType.OAuth || Authentication == WebAuthenticationType.Bearer) && Token is null)
                ErrorRecord error = GetValidationError(WebCmdletStrings.AuthenticationTokenNotSupplied, "WebCmdletAuthenticationTokenNotSuppliedException");
            if (!AllowUnencryptedAuthentication && (Authentication != WebAuthenticationType.None || Credential is not null || UseDefaultCredentials) && Uri.Scheme != "https")
                ErrorRecord error = GetValidationError(WebCmdletStrings.AllowUnencryptedAuthenticationRequired, "WebCmdletAllowUnencryptedAuthenticationRequiredException");
            // Credentials
            if (UseDefaultCredentials && Credential is not null)
                ErrorRecord error = GetValidationError(WebCmdletStrings.CredentialConflict, "WebCmdletCredentialConflictException");
            // Proxy server
            if (ProxyUseDefaultCredentials && ProxyCredential is not null)
                ErrorRecord error = GetValidationError(WebCmdletStrings.ProxyCredentialConflict, "WebCmdletProxyCredentialConflictException");
            else if (Proxy is null && (ProxyCredential is not null || ProxyUseDefaultCredentials))
                ErrorRecord error = GetValidationError(WebCmdletStrings.ProxyUriNotSupplied, "WebCmdletProxyUriNotSuppliedException");
            // Request body content
            if (Body is not null && InFile is not null)
                ErrorRecord error = GetValidationError(WebCmdletStrings.BodyConflict, "WebCmdletBodyConflictException");
            if (Body is not null && Form is not null)
                ErrorRecord error = GetValidationError(WebCmdletStrings.BodyFormConflict, "WebCmdletBodyFormConflictException");
            if (InFile is not null && Form is not null)
                ErrorRecord error = GetValidationError(WebCmdletStrings.FormInFileConflict, "WebCmdletFormInFileConflictException");
            // Validate InFile path
            if (InFile is not null)
                    Collection<string> providerPaths = GetResolvedProviderPathFromPSPath(InFile, out ProviderInfo provider);
                    if (!provider.Name.Equals(FileSystemProvider.ProviderName, StringComparison.OrdinalIgnoreCase))
                        errorRecord = GetValidationError(WebCmdletStrings.NotFilesystemPath, "WebCmdletInFileNotFilesystemPathException", InFile);
                        if (providerPaths.Count > 1)
                            errorRecord = GetValidationError(WebCmdletStrings.MultiplePathsResolved, "WebCmdletInFileMultiplePathsResolvedException", InFile);
                        else if (providerPaths.Count == 0)
                            errorRecord = GetValidationError(WebCmdletStrings.NoPathResolved, "WebCmdletInFileNoPathResolvedException", InFile);
                            if (Directory.Exists(providerPaths[0]))
                                errorRecord = GetValidationError(WebCmdletStrings.DirectoryPathSpecified, "WebCmdletInFileNotFilePathException", InFile);
                            _originalFilePath = InFile;
                            InFile = providerPaths[0];
                    errorRecord = new ErrorRecord(pathNotFound.ErrorRecord, pathNotFound);
                    errorRecord = new ErrorRecord(providerNotFound.ErrorRecord, providerNotFound);
                    errorRecord = new ErrorRecord(driveNotFound.ErrorRecord, driveNotFound);
                if (errorRecord is not null)
            // Output ??
            if (PassThru.IsPresent && OutFile is null)
                ErrorRecord error = GetValidationError(WebCmdletStrings.OutFileMissing, "WebCmdletOutFileMissingException", nameof(PassThru));
            // Resume requires OutFile.
            if (Resume.IsPresent && OutFile is null)
                ErrorRecord error = GetValidationError(WebCmdletStrings.OutFileMissing, "WebCmdletOutFileMissingException", nameof(Resume));
            _qualifiedOutFile = ShouldSaveToOutFile ? QualifiedOutFile : null;
            // OutFile must not be a directory to use Resume.
            if (Resume.IsPresent && Directory.Exists(_qualifiedOutFile))
                ErrorRecord error = GetValidationError(WebCmdletStrings.ResumeNotFilePath, "WebCmdletResumeNotFilePathException", _qualifiedOutFile);
        internal virtual void PrepareSession()
            // Make sure we have a valid WebRequestSession object to work with
            WebSession ??= new WebRequestSession();
            if (SessionVariable is not null)
                // Save the session back to the PS environment if requested
                vi.Set(SessionVariable, WebSession);
            // Handle credentials
            if (Credential is not null && Authentication == WebAuthenticationType.None)
                // Get the relevant NetworkCredential
                NetworkCredential netCred = Credential.GetNetworkCredential();
                WebSession.Credentials = netCred;
                // Supplying a credential overrides the UseDefaultCredentials setting
                WebSession.UseDefaultCredentials = false;
            else if ((Credential is not null || Token is not null) && Authentication != WebAuthenticationType.None)
                ProcessAuthentication();
                WebSession.UseDefaultCredentials = true;
            if (CertificateThumbprint is not null)
                using X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection tbCollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByThumbprint, CertificateThumbprint, false);
                if (tbCollection.Count == 0)
                    throw new CryptographicException(WebCmdletStrings.ThumbprintNotFound);
                foreach (X509Certificate2 tbCert in tbCollection)
                    X509Certificate certificate = (X509Certificate)tbCert;
                    WebSession.AddCertificate(certificate);
            if (Certificate is not null)
                WebSession.AddCertificate(Certificate);
            // Handle the user agent
            if (UserAgent is not null)
                // Store the UserAgent string
                WebSession.UserAgent = UserAgent;
            // Proxy and NoProxy parameters are mutually exclusive.
            // If NoProxy is provided, WebSession will turn off the proxy
            // and if Proxy is provided NoProxy will be turned off.
            if (NoProxy.IsPresent)
                WebSession.NoProxy = true;
                if (Proxy is not null)
                    WebProxy webProxy = new(Proxy);
                    webProxy.BypassProxyOnLocal = false;
                    if (ProxyCredential is not null)
                        webProxy.Credentials = ProxyCredential.GetNetworkCredential();
                        webProxy.UseDefaultCredentials = ProxyUseDefaultCredentials;
                    // We don't want to update the WebSession unless the proxies are different
                    // as that will require us to create a new HttpClientHandler and lose connection
                    // persistence.
                    if (!webProxy.Equals(WebSession.Proxy))
                        WebSession.Proxy = webProxy;
            if (MyInvocation.BoundParameters.ContainsKey(nameof(SslProtocol)))
                WebSession.SslProtocol = SslProtocol;
            if (MaximumRedirection > -1)
                WebSession.MaximumRedirection = MaximumRedirection;
            WebSession.UnixSocket = UnixSocket;
            WebSession.SkipCertificateCheck = SkipCertificateCheck.IsPresent;
            // Store the other supplied headers
            if (Headers is not null)
                foreach (string key in Headers.Keys)
                    object value = Headers[key];
                    // null is not valid value for header.
                    // We silently ignore header if value is null.
                    if (value is not null)
                        // Add the header value (or overwrite it if already present).
                        WebSession.Headers[key] = value.ToString();
            if (MaximumRetryCount > 0)
                WebSession.MaximumRetryCount = MaximumRetryCount;
                // Only set retry interval if retry count is set.
                WebSession.RetryIntervalInSeconds = RetryIntervalSec;
            WebSession.ConnectionTimeout = ConvertTimeoutSecondsToTimeSpan(ConnectionTimeoutSeconds);
        internal virtual HttpClient GetHttpClient(bool handleRedirect)
            HttpClient client = WebSession.GetHttpClient(handleRedirect, out bool clientWasReset);
            if (clientWasReset)
                WriteVerbose(WebCmdletStrings.WebSessionConnectionRecreated);
            return client;
        internal virtual HttpRequestMessage GetRequest(Uri uri)
            Uri requestUri = PrepareUri(uri);
            HttpMethod httpMethod = string.IsNullOrEmpty(CustomMethod) ? GetHttpMethod(Method) : new HttpMethod(CustomMethod);
            // Create the base WebRequest object
            HttpRequestMessage request = new(httpMethod, requestUri);
            if (HttpVersion is not null)
                request.Version = HttpVersion;
            // Pull in session data
            if (WebSession.Headers.Count > 0)
                WebSession.ContentHeaders.Clear();
                foreach (var entry in WebSession.Headers)
                    if (HttpKnownHeaderNames.ContentHeaders.Contains(entry.Key))
                        WebSession.ContentHeaders.Add(entry.Key, entry.Value);
                        if (SkipHeaderValidation)
                            request.Headers.TryAddWithoutValidation(entry.Key, entry.Value);
                            request.Headers.Add(entry.Key, entry.Value);
            // Set 'Transfer-Encoding: chunked' if 'Transfer-Encoding' is specified
            if (WebSession.Headers.ContainsKey(HttpKnownHeaderNames.TransferEncoding))
                request.Headers.TransferEncodingChunked = true;
            // Set 'User-Agent' if WebSession.Headers doesn't already contain it
            if (WebSession.Headers.TryGetValue(HttpKnownHeaderNames.UserAgent, out string userAgent))
                WebSession.UserAgent = userAgent;
                    request.Headers.TryAddWithoutValidation(HttpKnownHeaderNames.UserAgent, WebSession.UserAgent);
                    request.Headers.Add(HttpKnownHeaderNames.UserAgent, WebSession.UserAgent);
            // Set 'Keep-Alive' to false. This means set the Connection to 'Close'.
            if (DisableKeepAlive)
                request.Headers.Add(HttpKnownHeaderNames.Connection, "Close");
            // Set 'Transfer-Encoding'
            if (TransferEncoding is not null)
                TransferCodingHeaderValue headerValue = new(TransferEncoding);
                if (!request.Headers.TransferEncoding.Contains(headerValue))
                    request.Headers.TransferEncoding.Add(headerValue);
            // If the file to resume downloading exists, create the Range request header using the file size.
            // If not, create a Range to request the entire file.
            if (Resume.IsPresent)
                FileInfo fileInfo = new(QualifiedOutFile);
                if (fileInfo.Exists)
                    request.Headers.Range = new RangeHeaderValue(fileInfo.Length, null);
                    _resumeFileSize = fileInfo.Length;
                    request.Headers.Range = new RangeHeaderValue(0, null);
            return request;
        internal virtual void FillRequestStream(HttpRequestMessage request)
            ArgumentNullException.ThrowIfNull(request);
            // Set the request content type
            if (ContentType is not null)
                WebSession.ContentHeaders[HttpKnownHeaderNames.ContentType] = ContentType;
            else if (request.Method == HttpMethod.Post)
                // Win8:545310 Invoke-WebRequest does not properly set MIME type for POST
                WebSession.ContentHeaders.TryGetValue(HttpKnownHeaderNames.ContentType, out string contentType);
                    WebSession.ContentHeaders[HttpKnownHeaderNames.ContentType] = "application/x-www-form-urlencoded";
            if (Form is not null)
                MultipartFormDataContent formData = new();
                foreach (DictionaryEntry formEntry in Form)
                    // AddMultipartContent will handle PSObject unwrapping, Object type determination and enumerateing top level IEnumerables.
                    AddMultipartContent(fieldName: formEntry.Key, fieldValue: formEntry.Value, formData: formData, enumerate: true);
                SetRequestContent(request, formData);
            else if (Body is not null)
                // Coerce body into a usable form
                // Make sure we're using the base object of the body, not the PSObject wrapper
                object content = Body is PSObject psBody ? psBody.BaseObject : Body;
                switch (content)
                    case FormObject form:
                        SetRequestContent(request, form.Fields);
                    case IDictionary dictionary when request.Method != HttpMethod.Get:
                        SetRequestContent(request, dictionary);
                    case XmlNode xmlNode:
                        SetRequestContent(request, xmlNode);
                    case Stream stream:
                        SetRequestContent(request, stream);
                    case byte[] bytes:
                        SetRequestContent(request, bytes);
                    case MultipartFormDataContent multipartFormDataContent:
                        SetRequestContent(request, multipartFormDataContent);
                        SetRequestContent(request, (string)LanguagePrimitives.ConvertTo(content, typeof(string), CultureInfo.InvariantCulture));
            else if (InFile is not null)
                // Copy InFile data
                    // Open the input file
                    SetRequestContent(request, new FileStream(InFile, FileMode.Open, FileAccess.Read, FileShare.Read));
                catch (UnauthorizedAccessException)
                    string msg = string.Format(CultureInfo.InvariantCulture, WebCmdletStrings.AccessDenied, _originalFilePath);
                    throw new UnauthorizedAccessException(msg);
            // For other methods like Put where empty content has meaning, we need to fill in the content
            if (request.Content is null)
                // If this is a Get request and there is no content, then don't fill in the content as empty content gets rejected by some web services per RFC7230
                if (request.Method == HttpMethod.Get && ContentType is null)
                request.Content = new StringContent(string.Empty);
                request.Content.Headers.Clear();
            foreach (KeyValuePair<string, string> entry in WebSession.ContentHeaders)
                if (!string.IsNullOrWhiteSpace(entry.Value))
                        request.Content.Headers.TryAddWithoutValidation(entry.Key, entry.Value);
                            request.Content.Headers.Add(entry.Key, entry.Value);
                        catch (FormatException ex)
                            ValidationMetadataException outerEx = new(WebCmdletStrings.ContentTypeException, ex);
                            ErrorRecord er = new(outerEx, "WebCmdletContentTypeException", ErrorCategory.InvalidArgument, ContentType);
        internal virtual HttpResponseMessage GetResponse(HttpClient client, HttpRequestMessage request, bool handleRedirect)
            ArgumentNullException.ThrowIfNull(client);
            // Add 1 to account for the first request.
            int totalRequests = WebSession.MaximumRetryCount + 1;
            HttpRequestMessage currentRequest = request;
            HttpResponseMessage response = null;
                // Track the current URI being used by various requests and re-requests.
                Uri currentUri = currentRequest.RequestUri;
                _cancelToken = new CancellationTokenSource();
                    if (IsWriteVerboseEnabled())
                        WriteWebRequestVerboseInfo(currentRequest);
                    if (IsWriteDebugEnabled())
                        WriteWebRequestDebugInfo(currentRequest);
                    // codeql[cs/ssrf] - This is expected Poweshell behavior where user inputted Uri is supported for the context of this method. The user assumes trust for the Uri and invocation is done on the user's machine, not a web application. If there is concern for remoting, they should use restricted remoting.
                    response = client.SendAsync(currentRequest, HttpCompletionOption.ResponseHeadersRead, _cancelToken.Token).GetAwaiter().GetResult();
                        WriteWebResponseVerboseInfo(response);
                        WriteWebResponseDebugInfo(response);
                    if (ex.InnerException is TimeoutException)
                        // HTTP Request timed out
                        ErrorRecord er = new(ex, "ConnectionTimeoutReached", ErrorCategory.OperationTimeout, null);
                if (handleRedirect
                    && _maximumRedirection is not 0
                    && IsRedirectCode(response.StatusCode)
                    && response.Headers.Location is not null)
                    _cancelToken.Cancel();
                    // If explicit count was provided, reduce it for this redirection.
                    if (_maximumRedirection > 0)
                        _maximumRedirection--;
                    // For selected redirects, GET must be used with the redirected Location.
                    if (RequestRequiresForceGet(response.StatusCode, currentRequest.Method) && !PreserveHttpMethodOnRedirect)
                        Method = WebRequestMethod.Get;
                        CustomMethod = string.Empty;
                    currentUri = new Uri(request.RequestUri, response.Headers.Location);
                    // Continue to handle redirection
                    using HttpRequestMessage redirectRequest = GetRequest(currentUri);
                    response.Dispose();
                    response = GetResponse(client, redirectRequest, handleRedirect);
                // Request again without the Range header because the server indicated the range was not satisfiable.
                // This happens when the local file is larger than the remote file.
                // If the size of the remote file is the same as the local file, there is nothing to resume.
                    && (response.Content.Headers.ContentRange.HasLength
                    && response.Content.Headers.ContentRange.Length != _resumeFileSize))
                    WriteVerbose(WebCmdletStrings.WebMethodResumeFailedVerboseMsg);
                    // Disable the Resume switch so the subsequent calls to GetResponse() and FillRequestStream()
                    // are treated as a standard -OutFile request. This also disables appending local file.
                    Resume = new SwitchParameter(false);
                    using (HttpRequestMessage requestWithoutRange = GetRequest(currentUri))
                        FillRequestStream(requestWithoutRange);
                        response = GetResponse(client, requestWithoutRange, handleRedirect);
                _resumeSuccess = response.StatusCode == HttpStatusCode.PartialContent;
                // When MaximumRetryCount is not specified, the totalRequests is 1.
                if (totalRequests > 1 && ShouldRetry(response.StatusCode))
                    int retryIntervalInSeconds = WebSession.RetryIntervalInSeconds;
                    // If the status code is 429 get the retry interval from the Headers.
                    // Ignore broken header and its value.
                    if (response.StatusCode is HttpStatusCode.TooManyRequests && response.Headers.TryGetValues(HttpKnownHeaderNames.RetryAfter, out IEnumerable<string> retryAfter))
                            IEnumerator<string> enumerator = retryAfter.GetEnumerator();
                                retryIntervalInSeconds = Convert.ToInt32(enumerator.Current);
                            // Ignore broken header.
                    string retryMessage = string.Format(
                        WebCmdletStrings.RetryVerboseMsg,
                        retryIntervalInSeconds,
                        response.StatusCode);
                    WriteVerbose(retryMessage);
                    Task.Delay(retryIntervalInSeconds * 1000, _cancelToken.Token).GetAwaiter().GetResult();
                    currentRequest.Dispose();
                    currentRequest = GetRequest(currentUri);
                    FillRequestStream(currentRequest);
                totalRequests--;
            while (totalRequests > 0 && !response.IsSuccessStatusCode);
            return response;
        internal virtual void UpdateSession(HttpResponseMessage response)
        #endregion Virtual Methods
        internal static TimeSpan ConvertTimeoutSecondsToTimeSpan(int timeout) => timeout > 0 ? TimeSpan.FromSeconds(timeout) : Timeout.InfiniteTimeSpan;
        private void WriteWebRequestVerboseInfo(HttpRequestMessage request)
                // Typical Basic Example: 'WebRequest: v1.1 POST https://httpstat.us/200 with query length 6'
                StringBuilder verboseBuilder = new(128);
                // "Redact" the query string from verbose output, the details will be visible in Debug output
                string uriWithoutQuery = request.RequestUri?.GetLeftPart(UriPartial.Path) ?? string.Empty;
                verboseBuilder.Append($"WebRequest: v{request.Version} {request.Method} {uriWithoutQuery}");
                if (request.RequestUri?.Query is not null && request.RequestUri.Query.Length > 1)
                    verboseBuilder.Append($" with query length {request.RequestUri.Query.Length - 1}");
                string? requestContentType = ContentHelper.GetContentType(request);
                if (requestContentType is not null)
                    verboseBuilder.Append($" with {requestContentType} payload");
                long? requestContentLength = request.Content?.Headers?.ContentLength;
                if (requestContentLength is not null)
                    verboseBuilder.Append($" with body size {ContentHelper.GetFriendlyContentLength(requestContentLength)}");
                if (OutFile is not null)
                    verboseBuilder.Append($" output to {QualifyFilePath(OutFile)}");
                WriteVerbose(verboseBuilder.ToString().Trim());
                // Just in case there are any edge cases we missed, we don't break workflows with an exception
                WriteVerbose($"Failed to Write WebRequest Verbose Info: {ex} {ex.StackTrace}");
        private void WriteWebRequestDebugInfo(HttpRequestMessage request)
                // Typical basic example:
                // WebRequest Detail
                // ---QUERY
                // test = 5
                // --- HEADERS
                // User - Agent: Mozilla / 5.0, (Linux;Ubuntu 24.04.2 LTS;en - US), PowerShell / 7.6.0
                StringBuilder debugBuilder = new("WebRequest Detail" + Environment.NewLine, 512);
                if (!string.IsNullOrEmpty(request.RequestUri?.Query))
                    debugBuilder.Append(DebugHeaderPrefix).AppendLine("QUERY");
                    string[] queryParams = request.RequestUri.Query.TrimStart('?').Split('&');
                    debugBuilder
                        .AppendJoin(Environment.NewLine, queryParams)
                        .AppendLine()
                        .AppendLine();
                debugBuilder.Append(DebugHeaderPrefix).AppendLine("HEADERS");
                foreach (var headerSet in new HttpHeaders?[] { request.Headers, request.Content?.Headers })
                    if (headerSet is null)
                    debugBuilder.AppendLine(headerSet.ToString());
                if (request.Content is not null)
                    .Append(DebugHeaderPrefix).AppendLine("BODY")
                    .AppendLine(request.Content switch
                        StringContent stringContent => stringContent
                            .ReadAsStringAsync(_cancelToken.Token)
                            .GetAwaiter().GetResult(),
                        MultipartFormDataContent multipartContent => "=> Multipart Form Content"
                            + Environment.NewLine
                            + multipartContent.ReadAsStringAsync(_cancelToken.Token)
                        ByteArrayContent byteContent => InFile is not null
                            ? "[Binary content: "
                                + ContentHelper.GetFriendlyContentLength(byteContent.Headers.ContentLength)
                                + "]"
                            : byteContent.ReadAsStringAsync(_cancelToken.Token).GetAwaiter().GetResult(),
                        StreamContent streamContent =>
                            "[Stream content: " + ContentHelper.GetFriendlyContentLength(streamContent.Headers.ContentLength) + "]",
                        _ => "[Unknown content type]",
                    })
                WriteDebug(debugBuilder.ToString().Trim());
                WriteVerbose($"Failed to Write WebRequest Debug Info: {ex} {ex.StackTrace}");
        private void WriteWebResponseVerboseInfo(HttpResponseMessage response)
                // Typical basic example: WebResponse: 200 OK with text/plain payload body size 6 B (6 bytes)
                verboseBuilder.Append($"WebResponse: {(int)response.StatusCode} {response.ReasonPhrase ?? response.StatusCode.ToString()}");
                string? responseContentType = ContentHelper.GetContentType(response);
                if (responseContentType is not null)
                    verboseBuilder.Append($" with {responseContentType} payload");
                long? responseContentLength = response.Content?.Headers?.ContentLength;
                if (responseContentLength is not null)
                    verboseBuilder.Append($" with body size {ContentHelper.GetFriendlyContentLength(responseContentLength)}");
                WriteVerbose($"Failed to Write WebResponse Verbose Info: {ex} {ex.StackTrace}");
        private void WriteWebResponseDebugInfo(HttpResponseMessage response)
                // Typical basic example
                // WebResponse Detail
                // Date: Fri, 09 May 2025 18:06:44 GMT
                // Server: Kestrel
                // Set-Cookie: ARRAffinity=ee0b467f95b53d8dcfe48aeeb4173f93cf819be6e4721f434341647f4695039d;Path=/;HttpOnly;Secure;Domain=httpstat.us, ARRAffinitySameSite=ee0b467f95b53d8dcfe48aeeb4173f93cf819be6e4721f434341647f4695039d;Path=/;HttpOnly;SameSite=None;Secure;Domain=httpstat.us
                // Strict-Transport-Security: max-age=2592000
                // Request-Context: appId=cid-v1:3548b0f5-7f75-492f-82bb-b6eb0e864e53
                // Content-Length: 6
                // Content-Type: text/plain
                // --- BODY
                // 200 OK
                StringBuilder debugBuilder = new("WebResponse Detail" + Environment.NewLine, 512);
                foreach (var headerSet in new HttpHeaders?[] { response.Headers, response.Content?.Headers })
                if (response.Content is not null)
                    debugBuilder.Append(DebugHeaderPrefix).AppendLine("BODY");
                    if (ContentHelper.IsTextBasedContentType(ContentHelper.GetContentType(response)))
                        debugBuilder.AppendLine(
                            response.Content.ReadAsStringAsync(_cancelToken.Token)
                            .GetAwaiter().GetResult());
                        string friendlyContentLength = ContentHelper.GetFriendlyContentLength(
                            response.Content?.Headers?.ContentLength);
                        debugBuilder.AppendLine($"[Binary content: {friendlyContentLength}]");
                WriteVerbose($"Failed to Write WebResponse Debug Info: {ex} {ex.StackTrace}");
        private Uri PrepareUri(Uri uri)
            uri = CheckProtocol(uri);
            // Before creating the web request,
            // preprocess Body if content is a dictionary and method is GET (set as query)
            LanguagePrimitives.TryConvertTo<IDictionary>(Body, out IDictionary bodyAsDictionary);
            if (bodyAsDictionary is not null && (Method == WebRequestMethod.Default || Method == WebRequestMethod.Get || CustomMethod == "GET"))
                UriBuilder uriBuilder = new(uri);
                if (uriBuilder.Query is not null && uriBuilder.Query.Length > 1)
                    uriBuilder.Query = string.Concat(uriBuilder.Query.AsSpan(1), "&", FormatDictionary(bodyAsDictionary));
                    uriBuilder.Query = FormatDictionary(bodyAsDictionary);
                uri = uriBuilder.Uri;
                // Set body to null to prevent later FillRequestStream
                Body = null;
            return uri;
        private static Uri CheckProtocol(Uri uri)
            ArgumentNullException.ThrowIfNull(uri);
            return uri.IsAbsoluteUri ? uri : new Uri("http://" + uri.OriginalString);
#nullable restore
        private string QualifyFilePath(string path) => PathUtils.ResolveFilePath(filePath: path, command: this, isLiteralPath: true);
        private static string FormatDictionary(IDictionary content)
            ArgumentNullException.ThrowIfNull(content);
            StringBuilder bodyBuilder = new();
            foreach (string key in content.Keys)
                if (bodyBuilder.Length > 0)
                    bodyBuilder.Append('&');
                object value = content[key];
                // URLEncode the key and value
                string encodedKey = WebUtility.UrlEncode(key);
                string encodedValue = value is null ? string.Empty : WebUtility.UrlEncode(value.ToString());
                bodyBuilder.Append($"{encodedKey}={encodedValue}");
            return bodyBuilder.ToString();
        private ErrorRecord GetValidationError(string msg, string errorId)
            ValidationMetadataException ex = new(msg);
            return new ErrorRecord(ex, errorId, ErrorCategory.InvalidArgument, this);
        private ErrorRecord GetValidationError(string msg, string errorId, params object[] args)
            msg = string.Format(CultureInfo.InvariantCulture, msg, args);
        private string GetBasicAuthorizationHeader()
            string password = new NetworkCredential(string.Empty, Credential.Password).Password;
            string unencoded = string.Create(CultureInfo.InvariantCulture, $"{Credential.UserName}:{password}");
            byte[] bytes = Encoding.UTF8.GetBytes(unencoded);
            return string.Create(CultureInfo.InvariantCulture, $"Basic {Convert.ToBase64String(bytes)}");
        private string GetBearerAuthorizationHeader()
            return string.Create(CultureInfo.InvariantCulture, $"Bearer {new NetworkCredential(string.Empty, Token).Password}");
        private void ProcessAuthentication()
            if (Authentication == WebAuthenticationType.Basic)
                WebSession.Headers["Authorization"] = GetBasicAuthorizationHeader();
            else if (Authentication == WebAuthenticationType.Bearer || Authentication == WebAuthenticationType.OAuth)
                WebSession.Headers["Authorization"] = GetBearerAuthorizationHeader();
                Diagnostics.Assert(false, string.Create(CultureInfo.InvariantCulture, $"Unrecognized Authentication value: {Authentication}"));
        private bool IsPersistentSession() => MyInvocation.BoundParameters.ContainsKey(nameof(WebSession)) || MyInvocation.BoundParameters.ContainsKey(nameof(SessionVariable));
        /// Sets the ContentLength property of the request and writes the specified content to the request's RequestStream.
        /// <param name="request">The WebRequest who's content is to be set.</param>
        /// <param name="content">A byte array containing the content data.</param>
        /// Because this function sets the request's ContentLength property and writes content data into the request's stream,
        /// it should be called one time maximum on a given request.
        internal void SetRequestContent(HttpRequestMessage request, byte[] content)
            request.Content = new ByteArrayContent(content);
        /// <param name="content">A String object containing the content data.</param>
        internal void SetRequestContent(HttpRequestMessage request, string content)
            Encoding encoding = null;
            if (WebSession.ContentHeaders.TryGetValue(HttpKnownHeaderNames.ContentType, out string contentType) && contentType is not null)
                // If Content-Type contains the encoding format (as CharSet), use this encoding format
                // to encode the Body of the WebRequest sent to the server. Default Encoding format
                // would be used if Charset is not supplied in the Content-Type property.
                    MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);
                    if (!string.IsNullOrEmpty(mediaTypeHeaderValue.CharSet))
                        encoding = Encoding.GetEncoding(mediaTypeHeaderValue.CharSet);
                catch (Exception ex) when (ex is FormatException || ex is ArgumentException)
                    if (!SkipHeaderValidation)
                        ErrorRecord er = new(outerEx, "WebCmdletContentTypeException", ErrorCategory.InvalidArgument, contentType);
            byte[] bytes = StreamHelper.EncodeToBytes(content, encoding);
            request.Content = new ByteArrayContent(bytes);
        internal void SetRequestContent(HttpRequestMessage request, XmlNode xmlNode)
            ArgumentNullException.ThrowIfNull(xmlNode);
            XmlDocument doc = xmlNode as XmlDocument;
            if (doc?.FirstChild is XmlDeclaration decl && !string.IsNullOrEmpty(decl.Encoding))
                Encoding encoding = Encoding.GetEncoding(decl.Encoding);
                bytes = StreamHelper.EncodeToBytes(doc.OuterXml, encoding);
                bytes = StreamHelper.EncodeToBytes(xmlNode.OuterXml, encoding: null);
        /// <param name="contentStream">A Stream object containing the content data.</param>
        internal void SetRequestContent(HttpRequestMessage request, Stream contentStream)
            ArgumentNullException.ThrowIfNull(contentStream);
            request.Content = new StreamContent(contentStream);
        /// Sets the ContentLength property of the request and writes the ContentLength property of the request and writes the specified content to the request's RequestStream.
        /// <param name="multipartContent">A MultipartFormDataContent object containing multipart/form-data content.</param>
        internal void SetRequestContent(HttpRequestMessage request, MultipartFormDataContent multipartContent)
            ArgumentNullException.ThrowIfNull(multipartContent);
            // Content headers will be set by MultipartFormDataContent which will throw unless we clear them first
            request.Content = multipartContent;
        internal void SetRequestContent(HttpRequestMessage request, IDictionary content)
            string body = FormatDictionary(content);
            SetRequestContent(request, body);
        internal void ParseLinkHeader(HttpResponseMessage response)
            Uri requestUri = response.RequestMessage.RequestUri;
            if (_relationLink is null)
                // Must ignore the case of relation links. See RFC 8288 (https://tools.ietf.org/html/rfc8288)
                _relationLink = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _relationLink.Clear();
            // We only support the URL in angle brackets and `rel`, other attributes are ignored
            // user can still parse it themselves via the Headers property
            const string Pattern = "<(?<url>.*?)>;\\s*rel=(?<quoted>\")?(?<rel>(?(quoted).*?|[^,;]*))(?(quoted)\")";
            if (response.Headers.TryGetValues("Link", out IEnumerable<string> links))
                foreach (string linkHeader in links)
                    MatchCollection matchCollection = Regex.Matches(linkHeader, Pattern);
                    foreach (Match match in matchCollection)
                            string url = match.Groups["url"].Value;
                            string rel = match.Groups["rel"].Value;
                            if (url != string.Empty && rel != string.Empty && !_relationLink.ContainsKey(rel))
                                Uri absoluteUri = new(requestUri, url);
                                _relationLink.Add(rel, absoluteUri.AbsoluteUri);
        /// Adds content to a <see cref="MultipartFormDataContent"/>. Object type detection is used to determine if the value is string, File, or Collection.
        /// <param name="fieldName">The Field Name to use.</param>
        /// <param name="fieldValue">The Field Value to use.</param>
        /// <param name="formData">The <see cref="MultipartFormDataContent"/> to update.</param>
        /// <param name="enumerate">If true, collection types in <paramref name="fieldValue"/> will be enumerated. If false, collections will be treated as single value.</param>
        private static void AddMultipartContent(object fieldName, object fieldValue, MultipartFormDataContent formData, bool enumerate)
            ArgumentNullException.ThrowIfNull(formData);
            // It is possible that the dictionary keys or values are PSObject wrapped depending on how the dictionary is defined and assigned.
            // Before processing the field name and value we need to ensure we are working with the base objects and not the PSObject wrappers.
            // Unwrap fieldName PSObjects
            if (fieldName is PSObject namePSObject)
                fieldName = namePSObject.BaseObject;
            // Unwrap fieldValue PSObjects
            if (fieldValue is PSObject valuePSObject)
                fieldValue = valuePSObject.BaseObject;
            // Treat a single FileInfo as a FileContent
            if (fieldValue is FileInfo file)
                formData.Add(GetMultipartFileContent(fieldName: fieldName, file: file));
            // Treat Strings and other single values as a StringContent.
            // If enumeration is false, also treat IEnumerables as StringContents.
            // String implements IEnumerable so the explicit check is required.
            if (!enumerate || fieldValue is string || fieldValue is not IEnumerable)
                formData.Add(GetMultipartStringContent(fieldName: fieldName, fieldValue: fieldValue));
            // Treat the value as a collection and enumerate it if enumeration is true
            if (enumerate && fieldValue is IEnumerable items)
                    // Recurse, but do not enumerate the next level. IEnumerables will be treated as single values.
                    AddMultipartContent(fieldName: fieldName, fieldValue: item, formData: formData, enumerate: false);
        /// Gets a <see cref="StringContent"/> from the supplied field name and field value. Uses <see cref="LanguagePrimitives.ConvertTo{T}(object)"/> to convert the objects to strings.
        /// <param name="fieldName">The Field Name to use for the <see cref="StringContent"/></param>
        /// <param name="fieldValue">The Field Value to use for the <see cref="StringContent"/></param>
        private static StringContent GetMultipartStringContent(object fieldName, object fieldValue)
            ContentDispositionHeaderValue contentDisposition = new("form-data");
            contentDisposition.Name = LanguagePrimitives.ConvertTo<string>(fieldName);
            // codeql[cs/information-exposure-through-exception] - PowerShell is an on-premise product, meaning local users would already have access to the binaries and stack traces. Therefore, the information would not be exposed in the same way it would be for an ASP .NET service.
            StringContent result = new(LanguagePrimitives.ConvertTo<string>(fieldValue));
            result.Headers.ContentDisposition = contentDisposition;
        /// Gets a <see cref="StreamContent"/> from the supplied field name and <see cref="Stream"/>. Uses <see cref="LanguagePrimitives.ConvertTo{T}(object)"/> to convert the fieldname to a string.
        /// <param name="fieldName">The Field Name to use for the <see cref="StreamContent"/></param>
        /// <param name="stream">The <see cref="Stream"/> to use for the <see cref="StreamContent"/></param>
        private static StreamContent GetMultipartStreamContent(object fieldName, Stream stream)
            StreamContent result = new(stream);
            result.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        /// Gets a <see cref="StreamContent"/> from the supplied field name and file. Calls <see cref="GetMultipartStreamContent(object, Stream)"/> to create the <see cref="StreamContent"/> and then sets the file name.
        /// <param name="file">The file to use for the <see cref="StreamContent"/></param>
        private static StreamContent GetMultipartFileContent(object fieldName, FileInfo file)
            StreamContent result = GetMultipartStreamContent(fieldName: fieldName, stream: new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read));
            result.Headers.ContentDisposition.FileName = file.Name;
            result.Headers.ContentDisposition.FileNameStar = file.Name;
        private static string FormatErrorMessage(string error, string contentType)
            string formattedError = null;
                if (ContentHelper.IsXml(contentType))
                    XmlDocument doc = new();
                    doc.LoadXml(error);
                    XmlWriterSettings settings = new XmlWriterSettings
                        Indent = true,
                        NewLineOnAttributes = true,
                        OmitXmlDeclaration = true
                    if (doc.FirstChild is XmlDeclaration decl)
                        settings.Encoding = Encoding.GetEncoding(decl.Encoding);
                    StringBuilder stringBuilder = new();
                    using XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings);
                    doc.Save(xmlWriter);
                    string xmlString = stringBuilder.ToString();
                    formattedError = Environment.NewLine + xmlString;
                else if (ContentHelper.IsJson(contentType))
                    JsonNode jsonNode = JsonNode.Parse(error);
                    JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                    string jsonString = jsonNode.ToJsonString(options);
                    formattedError = Environment.NewLine + jsonString;
                // Ignore errors
            if (string.IsNullOrEmpty(formattedError))
                // Remove HTML tags making it easier to read
                formattedError = Regex.Replace(error, "<[^>]*>", string.Empty);
            return formattedError;
        // Returns true if the status code is one of the supported redirection codes.
        private static bool IsRedirectCode(HttpStatusCode statusCode) => statusCode switch
            HttpStatusCode.Found
            or HttpStatusCode.Moved
            or HttpStatusCode.MultipleChoices
            or HttpStatusCode.PermanentRedirect
            or HttpStatusCode.SeeOther
            or HttpStatusCode.TemporaryRedirect => true,
            _ => false
        // Returns true if the status code is a redirection code and the action requires switching to GET on redirection.
        // See https://learn.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode
        private static bool RequestRequiresForceGet(HttpStatusCode statusCode, HttpMethod requestMethod) => statusCode switch
            or HttpStatusCode.MultipleChoices => requestMethod == HttpMethod.Post,
            HttpStatusCode.SeeOther => requestMethod != HttpMethod.Get && requestMethod != HttpMethod.Head,
        // Returns true if the status code shows a server or client error and MaximumRetryCount > 0
        private static bool ShouldRetry(HttpStatusCode statusCode) => (int)statusCode switch
            304 or (>= 400 and <= 599) => true,
        private static HttpMethod GetHttpMethod(WebRequestMethod method) => method switch
            WebRequestMethod.Default or WebRequestMethod.Get => HttpMethod.Get,
            WebRequestMethod.Delete => HttpMethod.Delete,
            WebRequestMethod.Head => HttpMethod.Head,
            WebRequestMethod.Patch => HttpMethod.Patch,
            WebRequestMethod.Post => HttpMethod.Post,
            WebRequestMethod.Put => HttpMethod.Put,
            WebRequestMethod.Options => HttpMethod.Options,
            WebRequestMethod.Trace => HttpMethod.Trace,
            _ => new HttpMethod(method.ToString().ToUpperInvariant())
    /// Exception class for webcmdlets to enable returning HTTP error response.
    public sealed class HttpResponseException : HttpRequestException
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class.
        /// <param name="message">Message for the exception.</param>
        /// <param name="response">Response from the HTTP server.</param>
        public HttpResponseException(string message, HttpResponseMessage response) : base(message, inner: null, response.StatusCode)
            Response = response;
        /// HTTP error response.
        public HttpResponseMessage Response { get; }
