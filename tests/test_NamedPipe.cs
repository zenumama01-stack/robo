    public class NamedPipeTests
        public void TestCustomPipeNameCreation()
            string pipeNameForFirstCall = Path.GetRandomFileName();
            string pipeNameForSecondCall = Path.GetRandomFileName();
            RemoteSessionNamedPipeServer.CreateCustomNamedPipeServer(pipeNameForFirstCall);
            Assert.True(File.Exists(GetPipePath(pipeNameForFirstCall)));
            // The second call to this method would override the first named pipe.
            RemoteSessionNamedPipeServer.CreateCustomNamedPipeServer(pipeNameForSecondCall);
            Assert.True(File.Exists(GetPipePath(pipeNameForSecondCall)));
            // Previous pipe should have been cleaned up.
            Assert.False(File.Exists(GetPipePath(pipeNameForFirstCall)));
        public void TestCustomPipeNameCreationTooLongOnNonWindows()
            const string longPipeName = "DoggoipsumwaggywagssmolborkingdoggowithalongsnootforpatsdoingmeafrightenporgoYapperporgolongwatershoobcloudsbigolpupperlengthboy";
                Assert.Throws<InvalidOperationException>(() =>
                    RemoteSessionNamedPipeServer.CreateCustomNamedPipeServer(longPipeName));
                RemoteSessionNamedPipeServer.CreateCustomNamedPipeServer(longPipeName);
                Assert.True(File.Exists(GetPipePath(longPipeName)));
        private static string GetPipePath(string pipeName)
                return $@"\\.\pipe\{pipeName}";
            return $@"{Path.GetTempPath()}CoreFxPipe_{pipeName}";
