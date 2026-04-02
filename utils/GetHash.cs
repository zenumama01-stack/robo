    /// This class implements Get-FileHash.
    [Cmdlet(VerbsCommon.Get, "FileHash", DefaultParameterSetName = PathParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkId=517145")]
    [OutputType(typeof(FileHashInfo))]
    public class GetFileHashCommand : HashCmdletBase
        /// The paths of the files to calculate hash values.
        /// Resolved wildcards.
        [Parameter(Mandatory = true, ParameterSetName = PathParameterSet, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        /// LiteralPath parameter.
        /// The literal paths of the files to calculate a hashs.
        /// Don't resolved wildcards.
        [Parameter(Mandatory = true, ParameterSetName = LiteralPathParameterSet, Position = 0, ValueFromPipelineByPropertyName = true)]
        /// InputStream parameter.
        /// The stream of the file to calculate a hash.
        [Parameter(Mandatory = true, ParameterSetName = StreamParameterSet, Position = 0)]
        public Stream InputStream { get; set; }
        /// This is for paths collecting from pipe.
            List<string> pathsToProcess = new();
                    // Resolve paths and check existence
                            Collection<string> newPaths = Context.SessionState.Path.GetResolvedProviderPathFromPSPath(path, out provider);
                            if (newPaths != null)
                                pathsToProcess.AddRange(newPaths);
                        catch (ItemNotFoundException e)
                                ErrorRecord errorRecord = new(e,
                        string newPath = Context.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);
                        pathsToProcess.Add(newPath);
            foreach (string path in pathsToProcess)
                if (ComputeFileHash(path, out string hash))
                    WriteHashResult(Algorithm, hash, path);
        private byte[] ComputeHash(Stream stream)
            switch (Algorithm)
                case HashAlgorithmNames.SHA1:
                    return SHA1.HashData(stream);
                case HashAlgorithmNames.SHA256:
                    return SHA256.HashData(stream);
                case HashAlgorithmNames.SHA384:
                    return SHA384.HashData(stream);
                case HashAlgorithmNames.SHA512:
                    return SHA512.HashData(stream);
                case HashAlgorithmNames.MD5:
                    return MD5.HashData(stream);
            Debug.Assert(false, "invalid hash algorithm");
        /// Perform common error checks.
        /// Populate source code.
            if (ParameterSetName == StreamParameterSet)
                byte[] bytehash = ComputeHash(InputStream);
                string hash = Convert.ToHexString(bytehash);
                WriteHashResult(Algorithm, hash, string.Empty);
        /// Read the file and calculate the hash.
        /// <param name="path">Path to file which will be hashed.</param>
        /// <param name="hash">Will contain the hash of the file content.</param>
        /// <returns>Boolean value indicating whether the hash calculation succeeded or failed.</returns>
        private bool ComputeFileHash(string path, out string hash)
            Stream openfilestream = null;
            hash = null;
                openfilestream = File.OpenRead(path);
                byte[] bytehash = ComputeHash(openfilestream);
                hash = Convert.ToHexString(bytehash);
                    ex,
            catch (UnauthorizedAccessException ex)
                    "UnauthorizedAccessError",
            catch (IOException ioException)
                    ioException,
                    "FileReadError",
                openfilestream?.Dispose();
            return hash != null;
        /// Create FileHashInfo object and output it.
        private void WriteHashResult(string Algorithm, string hash, string path)
            FileHashInfo result = new();
            result.Algorithm = Algorithm;
            result.Hash = hash;
            result.Path = path;
        /// Parameter set names.
        private const string StreamParameterSet = "StreamParameterSet";
    /// Base Cmdlet for cmdlets which deal with crypto hashes.
    public class HashCmdletBase : PSCmdlet
        /// Algorithm parameter.
        /// The hash algorithm name: "SHA1", "SHA256", "SHA384", "SHA512", "MD5".
        [ValidateSet(HashAlgorithmNames.SHA1,
                     HashAlgorithmNames.SHA256,
                     HashAlgorithmNames.SHA384,
                     HashAlgorithmNames.SHA512,
                     HashAlgorithmNames.MD5)]
        public string Algorithm
                return _Algorithm;
                // A hash algorithm name is case sensitive
                // and always must be in upper case
                _Algorithm = value.ToUpper();
        private string _Algorithm = HashAlgorithmNames.SHA256;
        /// Hash algorithm names.
        internal static class HashAlgorithmNames
            public const string MD5 = "MD5";
            public const string SHA1 = "SHA1";
            public const string SHA256 = "SHA256";
            public const string SHA384 = "SHA384";
            public const string SHA512 = "SHA512";
    /// FileHashInfo class contains information about a file hash.
    public class FileHashInfo
        /// Hash algorithm name.
        public string Algorithm { get; set; }
        /// Hash value.
        public string Hash { get; set; }
        /// File path.
        public string Path { get; set; }
