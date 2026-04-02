    /// This represents a bound dispmember on a IDispatch object.
    internal sealed class DispCallable : IPseudoComObject
        internal DispCallable(IDispatchComObject dispatch, string memberName, int dispId)
            DispatchComObject = dispatch;
            MemberName = memberName;
        public override string ToString() => $"<bound dispmethod {MemberName}>";
        public IDispatchComObject DispatchComObject { get; }
        public IDispatch DispatchObject => DispatchComObject.DispatchObject;
        public string MemberName { get; }
            return new DispCallableMetaObject(parameter, this);
            return obj is DispCallable other && other.DispatchComObject == DispatchComObject && other.DispId == DispId;
            return DispatchComObject.GetHashCode() ^ DispId;
