    public static class PSVersionInfoTests
        public static void TestVersions()
            // test that a non-null version table is returned, and
            // that it does not throw
            Assert.NotNull(PSVersionInfo.GetPSVersionTable());
