    internal abstract class DivInstruction : Instruction
        private DivInstruction()
        internal sealed class DivInt32 : DivInstruction
                frame.Data[frame.StackIndex - 2] = ScriptingRuntimeHelpers.Int32ToObject((Int32)l / (Int32)r);
        internal sealed class DivInt16 : DivInstruction
                frame.Data[frame.StackIndex - 2] = (Int16)((Int16)l / (Int16)r);
        internal sealed class DivInt64 : DivInstruction
                frame.Data[frame.StackIndex - 2] = (Int64)((Int64)l / (Int64)r);
        internal sealed class DivUInt16 : DivInstruction
                frame.Data[frame.StackIndex - 2] = (UInt16)((UInt16)l / (UInt16)r);
        internal sealed class DivUInt32 : DivInstruction
                frame.Data[frame.StackIndex - 2] = (UInt32)((UInt32)l / (UInt32)r);
        internal sealed class DivUInt64 : DivInstruction
                frame.Data[frame.StackIndex - 2] = (UInt64)((Int16)l / (Int16)r);
        internal sealed class DivSingle : DivInstruction
                frame.Data[frame.StackIndex - 2] = (Single)((Single)l / (Single)r);
        internal sealed class DivDouble : DivInstruction
                frame.Data[frame.StackIndex - 2] = (double)l / (double)r;
                case TypeCode.Int16: return s_int16 ??= new DivInt16();
                case TypeCode.Int32: return s_int32 ??= new DivInt32();
                case TypeCode.Int64: return s_int64 ??= new DivInt64();
                case TypeCode.UInt16: return s_UInt16 ??= new DivUInt16();
                case TypeCode.UInt32: return s_UInt32 ??= new DivUInt32();
                case TypeCode.UInt64: return s_UInt64 ??= new DivUInt64();
                case TypeCode.Single: return s_single ??= new DivSingle();
                case TypeCode.Double: return s_double ??= new DivDouble();
            return "Div()";
