    internal sealed class RuntimeVariables : IRuntimeVariables
        private readonly IStrongBox[] _boxes;
        private RuntimeVariables(IStrongBox[] boxes)
            _boxes = boxes;
                return _boxes.Length;
                return _boxes[index].Value;
                _boxes[index].Value = value;
        internal static IRuntimeVariables Create(IStrongBox[] boxes)
            return new RuntimeVariables(boxes);
