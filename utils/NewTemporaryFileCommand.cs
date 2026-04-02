    /// The implementation of the "New-TemporaryFile" cmdlet.
    [Cmdlet(VerbsCommon.New, "TemporaryFile", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2097032")]
    [OutputType(typeof(System.IO.FileInfo))]
    public class NewTemporaryFileCommand : Cmdlet
        /// Returns a TemporaryFile.
            string tempPath = Path.GetTempPath();
            if (ShouldProcess(tempPath))
                    filePath = Path.GetTempFileName();
                            "NewTemporaryFileWriteError",
                            ErrorCategory.WriteError,
                            tempPath));
                if (!string.IsNullOrEmpty(filePath))
                    FileInfo file = new(filePath);
                    WriteObject(file);
