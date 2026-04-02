#pragma warning disable 1634, 1691
#pragma warning disable 56506
namespace System.Management.Automation.Runspaces
    /// Defines the attribute used to designate a cmdlet parameter as one that
    /// should accept runspaces.
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RunspaceAttribute : ArgumentTransformationAttribute
        /// Transforms the input data to a Runspace.
        /// <param name="engineIntrinsics">
        /// The engine APIs for the context under which the transformation is being
        /// made.
        /// <param name="inputData">
        /// If a string, the transformation uses the input as the runspace name.
        /// If an int, the transformation uses the input as the runspace ID.
        /// If a guid, the transformation uses the input as the runspace GUID.
        /// If already a Runspace, the transform does nothing.
        /// <returns>A runspace object representing the inputData.</returns>
            if (engineIntrinsics?.Host?.UI == null)
                throw PSTraceSource.NewArgumentNullException("engineIntrinsics");
            if (inputData == null)
            // Try to coerce the input as a runspace
            Runspace runspace = LanguagePrimitives.FromObjectAs<Runspace>(inputData);
            if (runspace != null)
                return runspace;
            // Try to coerce the runspace if the user provided a string, int, or guid
            switch (inputData)
                case string name:
                    var runspacesByName = GetRunspaceUtils.GetRunspacesByName(new[] { name });
                    if (runspacesByName.Count == 1)
                        return runspacesByName[0];
                case int id:
                    var runspacesById = GetRunspaceUtils.GetRunspacesById(new[] { id });
                    if (runspacesById.Count == 1)
                        return runspacesById[0];
                case Guid guid:
                    var runspacesByGuid = GetRunspaceUtils.GetRunspacesByInstanceId(new[] { guid });
                    if (runspacesByGuid.Count == 1)
                        return runspacesByGuid[0];
                    // Non-convertible type
            // If we couldn't get a single runspace, return the inputData
        /// Gets a flag indicating whether or not null optional parameters are transformed.
        public override bool TransformNullOptionalParameters { get { return false; } }
#pragma warning restore 56506
