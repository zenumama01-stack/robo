    internal interface IMethodInvoker
        Delegate Invoker { get; }
        object[] CreateInvokerArgs(Delegate methodToInvoke, object?[]? methodToInvokeArgs);
