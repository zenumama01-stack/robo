    /// Information about invocation of a method in an object model wrapped by an instance of <see cref="CmdletAdapter{TObjectInstance}"/>
    public sealed class MethodInvocationInfo
        /// Creates a new instance of MethodInvocationInfo.
        /// <param name="name">Name of the method to invoke.</param>
        /// <param name="parameters">Method parameters.</param>
        /// <param name="returnValue">Return value of the method (ok to pass <see langword="null"/> if the method doesn't return anything).</param>
        public MethodInvocationInfo(string name, IEnumerable<MethodParameter> parameters, MethodParameter returnValue)
            // returnValue can be null
            MethodName = name;
            ReturnValue = returnValue;
            KeyedCollection<string, MethodParameter> mpk = new MethodParametersCollection();
            foreach (var parameter in parameters)
                mpk.Add(parameter);
            Parameters = mpk;
        /// Name of the method to invoke.
        public string MethodName { get; }
        /// Method parameters.
        public KeyedCollection<string, MethodParameter> Parameters { get; }
        /// Return value of the method.  Can be <see langword="null"/> if the method doesn't return anything.
        public MethodParameter ReturnValue { get; }
        internal IEnumerable<T> GetArgumentsOfType<T>() where T : class
            List<T> result = new();
            foreach (var methodParameter in this.Parameters)
                if ((methodParameter.Bindings & MethodParameterBindings.In) != MethodParameterBindings.In)
                if (methodParameter.Value is T objectInstance)
                    result.Add(objectInstance);
                if (methodParameter.Value is IEnumerable objectInstanceArray)
                    foreach (object element in objectInstanceArray)
                        if (element is T objectInstance2)
                            result.Add(objectInstance2);
