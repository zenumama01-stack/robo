    /// An object that represents a stack of paths.
    public sealed class PathInfoStack : Stack<PathInfo>
        /// Constructor for the PathInfoStack class.
        /// The name of the stack.
        /// <param name="locationStack">
        /// A stack object containing PathInfo objects
        /// If <paramref name="locationStack"/> is null.
        /// If <paramref name="stackName"/> is null or empty.
        internal PathInfoStack(string stackName, Stack<PathInfo> locationStack) : base()
            if (locationStack == null)
                throw PSTraceSource.NewArgumentNullException(nameof(locationStack));
            Name = stackName;
            // Since the Stack<T> constructor takes an IEnumerable and
            // not a Stack<T> the stack actually gets enumerated in the
            // wrong order.  I have to push them on manually in the
            // appropriate order.
            PathInfo[] stackContents = new PathInfo[locationStack.Count];
            locationStack.CopyTo(stackContents, 0);
            for (int index = stackContents.Length - 1; index >= 0; --index)
                this.Push(stackContents[index]);
        /// Gets the name of the stack.
        public string Name { get; } = null;
