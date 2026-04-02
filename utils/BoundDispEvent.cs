using System.Management.Automation.InteropServices;
    internal sealed class BoundDispEvent : DynamicObject
        private readonly object _rcw;
        private readonly Guid _sourceIid;
        private readonly int _dispid;
        internal BoundDispEvent(object rcw, Guid sourceIid, int dispid)
            _rcw = rcw;
            _sourceIid = sourceIid;
            _dispid = dispid;
        /// Provides the implementation of performing AddAssign and SubtractAssign binary operations.
        /// <param name="binder">The binder provided by the call site.</param>
        /// <param name="handler">The handler for the operation.</param>
        /// <param name="result">The result of the operation.</param>
        /// <returns>True if the operation is complete, false if the call site should determine behavior.</returns>
        public override bool TryBinaryOperation(BinaryOperationBinder binder, object handler, out object result)
            if (binder.Operation == ExpressionType.AddAssign)
                result = InPlaceAdd(handler);
            if (binder.Operation == ExpressionType.SubtractAssign)
                result = InPlaceSubtract(handler);
        private static void VerifyHandler(object handler)
            if (handler is Delegate && handler.GetType() != typeof(Delegate))
                return; // delegate
            if (handler is IDynamicMetaObjectProvider)
                return; // IDMOP
            if (handler is DispCallable)
            throw Error.UnsupportedHandlerType();
        /// Adds a handler to an event.
        /// <param name="handler">The handler to be added.</param>
        /// <returns>The original event with handler added.</returns>
        private object InPlaceAdd(object handler)
            Requires.NotNull(handler);
            VerifyHandler(handler);
            ComEventsSink comEventSink = ComEventsSink.FromRuntimeCallableWrapper(_rcw, _sourceIid, true);
            comEventSink.AddHandler(_dispid, handler);
        /// Removes handler from the event.
        /// <param name="handler">The handler to be removed.</param>
        /// <returns>The original event with handler removed.</returns>
        private object InPlaceSubtract(object handler)
            ComEventsSink comEventSink = ComEventsSink.FromRuntimeCallableWrapper(_rcw, _sourceIid, false);
            comEventSink?.RemoveHandler(_dispid, handler);
