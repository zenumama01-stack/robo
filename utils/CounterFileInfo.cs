namespace Microsoft.PowerShell.Commands.GetCounter
    public class CounterFileInfo
        internal CounterFileInfo(DateTime oldestRecord,
            DateTime newestRecord,
            UInt32 sampleCount)
            _oldestRecord = oldestRecord;
            _newestRecord = newestRecord;
            _sampleCount = sampleCount;
        internal CounterFileInfo() { }
        public DateTime OldestRecord
                return _oldestRecord;
        private DateTime _oldestRecord = DateTime.MinValue;
        public DateTime NewestRecord
                return _newestRecord;
        private DateTime _newestRecord = DateTime.MaxValue;
        public UInt32 SampleCount
                return _sampleCount;
        private UInt32 _sampleCount = 0;
