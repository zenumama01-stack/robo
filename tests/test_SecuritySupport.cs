    public static class SecuritySupportTests
        public static void TestScanContent()
                Assert.Equal(AmsiUtils.AmsiNativeMethods.AMSI_RESULT.AMSI_RESULT_NOT_DETECTED, AmsiUtils.ScanContent(string.Empty, string.Empty));
