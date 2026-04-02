    internal abstract class NumericConvertInstruction : Instruction
        internal readonly TypeCode _from, _to;
        protected NumericConvertInstruction(TypeCode from, TypeCode to)
            _from = from;
            _to = to;
            return InstructionName + "(" + _from + "->" + _to + ")";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        internal sealed class Unchecked : NumericConvertInstruction
            public override string InstructionName { get { return "UncheckedConvert"; } }
            public Unchecked(TypeCode from, TypeCode to)
                : base(from, to)
                frame.Push(Convert(frame.Pop()));
            private object Convert(object obj)
                switch (_from)
                    case TypeCode.Byte: return ConvertInt32((byte)obj);
                    case TypeCode.SByte: return ConvertInt32((sbyte)obj);
                    case TypeCode.Int16: return ConvertInt32((Int16)obj);
                    case TypeCode.Char: return ConvertInt32((char)obj);
                    case TypeCode.Int32: return ConvertInt32((Int32)obj);
                    case TypeCode.Int64: return ConvertInt64((Int64)obj);
                    case TypeCode.UInt16: return ConvertInt32((UInt16)obj);
                    case TypeCode.UInt32: return ConvertInt64((UInt32)obj);
                    case TypeCode.UInt64: return ConvertUInt64((UInt64)obj);
                    case TypeCode.Single: return ConvertDouble((Single)obj);
                    case TypeCode.Double: return ConvertDouble((double)obj);
            private object ConvertInt32(int obj)
                    switch (_to)
                        case TypeCode.Byte: return (byte)obj;
                        case TypeCode.SByte: return (sbyte)obj;
                        case TypeCode.Int16: return (Int16)obj;
                        case TypeCode.Char: return (char)obj;
                        case TypeCode.Int32: return (Int32)obj;
                        case TypeCode.Int64: return (Int64)obj;
                        case TypeCode.UInt16: return (UInt16)obj;
                        case TypeCode.UInt32: return (UInt32)obj;
                        case TypeCode.UInt64: return (UInt64)obj;
                        case TypeCode.Single: return (Single)obj;
                        case TypeCode.Double: return (double)obj;
            private object ConvertInt64(Int64 obj)
            private object ConvertUInt64(UInt64 obj)
            private object ConvertDouble(double obj)
        internal sealed class Checked : NumericConvertInstruction
            public override string InstructionName { get { return "CheckedConvert"; } }
            public Checked(TypeCode from, TypeCode to)
                checked
