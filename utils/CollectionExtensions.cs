    internal static class CollectionExtensions
        internal static T[] RemoveFirst<T>(this T[] array)
            T[] result = new T[array.Length - 1];
            Array.Copy(array, 1, result, 0, result.Length);
        internal static T[] AddFirst<T>(this IList<T> list, T item)
            T[] res = new T[list.Count + 1];
            res[0] = item;
            list.CopyTo(res, 1);
        internal static T[] ToArray<T>(this IList<T> list)
            T[] res = new T[list.Count];
            list.CopyTo(res, 0);
        internal static T[] AddLast<T>(this IList<T> list, T item)
            res[list.Count] = item;
