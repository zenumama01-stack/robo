    /// Implements the stop-transcript cmdlet.
    [Cmdlet(VerbsLifecycle.Stop, "Transcript", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.None, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096798")]
    public sealed class StopTranscriptCommand : PSCmdlet
        /// Stops the transcription.
            if (!ShouldProcess(string.Empty))
                string outFilename = Host.UI.StopTranscribing();
                if (outFilename != null)
                        StringUtil.Format(TranscriptStrings.TranscriptionStopped, outFilename));
                    outputObject.Properties.Add(new PSNoteProperty("Path", outFilename));
                        e, TranscriptStrings.ErrorStoppingTranscript, e.Message);
