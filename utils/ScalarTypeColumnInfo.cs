    internal sealed class ScalarTypeColumnInfo : ColumnInfo
        private readonly Type _type;
        internal ScalarTypeColumnInfo(Type type)
            : base(type.Name, type.Name)
            _type = type;
            // Strip a wrapping PSObject.
            object baseObject = ((PSObject)liveObject).BaseObject;
            if (baseObject.GetType().Equals(_type))
                return ColumnInfo.LimitString(baseObject);
    internal sealed class TypeNameColumnInfo : ColumnInfo
        internal TypeNameColumnInfo(string staleObjectPropertyName, string displayName)
            return baseObject.GetType().FullName;
    internal sealed class ToStringColumnInfo : ColumnInfo
        internal ToStringColumnInfo(string staleObjectPropertyName, string displayName, OutGridViewCommand parentCmdlet)
            // Convert to a string preserving PowerShell formatting.
            return ColumnInfo.LimitString(_parentCmdlet.ConvertToString(liveObject));
    internal sealed class IndexColumnInfo : ColumnInfo
        private int _index = 0;
        internal IndexColumnInfo(string staleObjectPropertyName, string displayName, int index)
            _index = index;
            // Every time this method is called, another raw is added to ML.
            return _index++;
