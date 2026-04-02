    public static class PlatformTests
        public static void TestIsCoreCLR()
            Assert.True(Platform.IsCoreCLR);
#if Unix
        public static void TestGetUserName()
                FileName = @"/usr/bin/env",
                Arguments = "whoami",
                UseShellExecute = false
            using (Process process = Process.Start(startInfo))
                // Get output of call to whoami without trailing newline
                string username = process.StandardOutput.ReadToEnd().Trim();
                // The process should return an exit code of 0 on success
                Assert.Equal(0, process.ExitCode);
                Assert.Equal(username, Environment.UserName);
        public static void TestGetMachineName()
                Arguments = "hostname",
                 // Get output of call to hostname without trailing newline
                string hostname = process.StandardOutput.ReadToEnd().Trim();
                // It should be the same as what our platform code returns
                Assert.Equal(hostname, Environment.MachineName);
        public static void TestGetFQDN()
                Arguments = "hostname --fqdn",
                Assert.Equal(hostname, Platform.NonWindowsGetHostName());
        public static void TestIsExecutable()
            Assert.True(Platform.NonWindowsIsExecutable("/bin/ls"));
        public static void TestIsNotExecutable()
            Assert.False(Platform.NonWindowsIsExecutable("/etc/hosts"));
        public static void TestDirectoryIsNotExecutable()
            Assert.False(Platform.NonWindowsIsExecutable("/etc"));
        public static void TestFileIsNotHardLink()
            string path = @"/tmp/nothardlink";
            File.Create(path);
            FileSystemInfo fd = new FileInfo(path);
            // Since this is the only reference to the file, it is not considered a
            // hardlink by our API (though all files are hardlinks on Linux)
            Assert.False(Platform.NonWindowsIsHardLink(fd));
        public static void TestFileIsHardLink()
            string path = @"/tmp/originallink";
            string link = "/tmp/newlink";
            if (File.Exists(link))
                File.Delete(link);
                Arguments = "ln " + path + " " + link,
            // Since there are now two references to the file, both are considered
            // hardlinks by our API (though all files are hardlinks on Linux)
            Assert.True(Platform.NonWindowsIsHardLink(fd));
            fd = new FileInfo(link);
        public static void TestDirectoryIsNotHardLink()
            string path = @"/tmp";
        public static void TestNonExistentIsHardLink()
            // A file that should *never* exist on a test machine:
            string path = @"/tmp/ThisFileShouldNotExistOnTestMachines";
            // If the file exists, then there's a larger issue that needs to be looked at
            Assert.False(File.Exists(path));
            // Convert `path` string to FileSystemInfo data type. And now, it should return true
        public static void TestFileIsSymLink()
                Arguments = "ln -s " + path + " " + link,
            Assert.False(Platform.NonWindowsIsSymLink(fd));
            Assert.True(Platform.NonWindowsIsSymLink(fd));
