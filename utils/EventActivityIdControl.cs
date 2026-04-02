        internal enum ActivityControl : uint
            /// Gets the ActivityId from thread local storage.
            Get = 1,
            /// Sets the ActivityId in the thread local storage.
            Set = 2,
            /// Creates a new activity id.
            Create = 3,
            /// Sets the activity id in thread local storage and returns the previous value.
            GetSet = 4,
            /// Creates a new activity id, sets thread local storage, and returns the previous value.
            CreateSet = 5
        [LibraryImport("api-ms-win-eventing-provider-l1-1-0.dll")]
        internal static unsafe partial int EventActivityIdControl(ActivityControl controlCode, Guid* activityId);
        internal static unsafe int GetEventActivityIdControl(ref Guid activityId)
            fixed (Guid* guidPtr = &activityId)
                return EventActivityIdControl(ActivityControl.Get, guidPtr);
