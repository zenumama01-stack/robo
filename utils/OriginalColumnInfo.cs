    internal sealed class OriginalColumnInfo : ColumnInfo
        private readonly string _liveObjectPropertyName;
        private readonly OutGridViewCommand _parentCmdlet;
        internal OriginalColumnInfo(string staleObjectPropertyName, string displayName, string liveObjectPropertyName, OutGridViewCommand parentCmdlet)
            _liveObjectPropertyName = liveObjectPropertyName;
            _parentCmdlet = parentCmdlet;
                PSPropertyInfo propertyInfo = liveObject.Properties[_liveObjectPropertyName];
                // The live object has the liveObjectPropertyName property.
                object liveObjectValue = propertyInfo.Value;
                if (liveObjectValue is ICollection collectionValue)
                    liveObjectValue = _parentCmdlet.ConvertToString(PSObjectHelper.AsPSObject(propertyInfo.Value));
                    if (liveObjectValue is PSObject psObjectValue)
                        // Since PSObject implements IComparable there is a need to verify if its BaseObject actually implements IComparable.
                        if (psObjectValue.BaseObject is IComparable)
                            liveObjectValue = psObjectValue;
                            // Use the String type as default.
                            liveObjectValue = _parentCmdlet.ConvertToString(psObjectValue);
                return ColumnInfo.LimitString(liveObjectValue);
                // ignore
