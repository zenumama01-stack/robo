    /// Provides common utilities for filtering.
    internal static class FilterUtilities
        internal static bool TryCastItem<T>(object item, out T castItem)
            castItem = default(T);
            bool isItemUncastable = item == null && typeof(T).IsValueType;
            if (isItemUncastable)
            bool shouldCastToString = item != null && typeof(string) == typeof(T);
            if (shouldCastToString)
                // NOTE: string => T doesn't compile. We confuse the type system
                // and use string => object => T to make this work.
                object stringPropertyValue = item.ToString();
                castItem = (T)stringPropertyValue;
                castItem = (T)item;
            catch (InvalidCastException e)
                Debug.Print(e.ToString());
