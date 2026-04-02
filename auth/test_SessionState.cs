    public class SessionStateTests
        public void TestDrives()
            Collection<PSDriveInfo> drives = sessionState.Drives(null);
            Assert.NotNull(drives);
