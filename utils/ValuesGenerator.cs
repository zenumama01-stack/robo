    public static class ValuesGenerator
        private const int Seed = 12345; // we always use the same seed to have repeatable results!
        public static T GetNonDefaultValue<T>()
            if (typeof(T) == typeof(byte)) // we can't use ArrayOfUniqueValues for byte
                return Array<T>(byte.MaxValue).First(value => !value.Equals(default));
                return ArrayOfUniqueValues<T>(2).First(value => !value.Equals(default));
        /// does not support byte because there are only 256 unique byte values
        public static T[] ArrayOfUniqueValues<T>(int count)
            // allocate the array first to try to take advantage of memory randomization
            // as it's usually the first thing called from GlobalSetup method
            // which with MemoryRandomization enabled is the first method called right after allocation
            // of random-sized memory by BDN engine
            T[] result = new T[count];
            var random = new Random(Seed);
            var uniqueValues = new HashSet<T>();
            while (uniqueValues.Count != count)
                T value = GenerateValue<T>(random);
                if (!uniqueValues.Contains(value))
                    uniqueValues.Add(value);
            uniqueValues.CopyTo(result);
        public static T[] Array<T>(int count)
            var result = new T[count];
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
                random.NextBytes(Unsafe.As<byte[]>(result));
                    result[i] = GenerateValue<T>(random);
        public static Dictionary<TKey, TValue> Dictionary<TKey, TValue>(int count)
            var dictionary = new Dictionary<TKey, TValue>();
            while (dictionary.Count != count)
                TKey key = GenerateValue<TKey>(random);
                if (!dictionary.ContainsKey(key))
                    dictionary.Add(key, GenerateValue<TValue>(random));
        private static T GenerateValue<T>(Random random)
            if (typeof(T) == typeof(char))
                return (T)(object)(char)random.Next(char.MinValue, char.MaxValue);
            if (typeof(T) == typeof(short))
                return (T)(object)(short)random.Next(short.MaxValue);
            if (typeof(T) == typeof(ushort))
                return (T)(object)(ushort)random.Next(short.MaxValue);
            if (typeof(T) == typeof(int))
                return (T)(object)random.Next();
            if (typeof(T) == typeof(uint))
                return (T)(object)(uint)random.Next();
            if (typeof(T) == typeof(long))
                return (T)(object)(long)random.Next();
            if (typeof(T) == typeof(ulong))
                return (T)(object)(ulong)random.Next();
            if (typeof(T) == typeof(float))
                return (T)(object)(float)random.NextDouble();
            if (typeof(T) == typeof(double))
                return (T)(object)random.NextDouble();
                return (T)(object)(random.NextDouble() > 0.5);
            if (typeof(T) == typeof(string))
                return (T)(object)GenerateRandomString(random, 1, 50);
                return (T)(object)GenerateRandomGuid(random);
            throw new NotImplementedException($"{typeof(T).Name} is not implemented");
        private static string GenerateRandomString(Random random, int minLength, int maxLength)
            var length = random.Next(minLength, maxLength);
            var builder = new StringBuilder(length);
                var rangeSelector = random.Next(0, 3);
                if (rangeSelector == 0)
                    builder.Append((char) random.Next('a', 'z'));
                else if (rangeSelector == 1)
                    builder.Append((char) random.Next('A', 'Z'));
                    builder.Append((char) random.Next('0', '9'));
        private static Guid GenerateRandomGuid(Random random)
            byte[] bytes = new byte[16];
            random.NextBytes(bytes);
            return new Guid(bytes);
