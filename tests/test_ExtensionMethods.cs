    public static class PSTypeExtensionsTests
        public static void TestIsNumeric()
            Assert.True(PSTypeExtensions.IsNumeric(42.GetType()));
