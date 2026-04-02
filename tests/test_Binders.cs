    public static class PSEnumerableBinderTests
        public static void TestIsStaticTypePossiblyEnumerable()
            // It just needs an arbitrary type
            Assert.False(PSEnumerableBinder.IsStaticTypePossiblyEnumerable(42.GetType()));
