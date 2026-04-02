    internal abstract class MulInstruction : Instruction
        private MulInstruction()
        internal sealed class MulInt32 : MulInstruction
                frame.Data[frame.StackIndex - 2] = ScriptingRuntimeHelpers.Int32ToObject(unchecked((Int32)l * (Int32)r));
        internal sealed class MulInt16 : MulInstruction
                frame.Data[frame.StackIndex - 2] = (Int16)unchecked((Int16)l * (Int16)r);
        internal sealed class MulInt64 : MulInstruction
                frame.Data[frame.StackIndex - 2] = (Int64)unchecked((Int64)l * (Int64)r);
        internal sealed class MulUInt16 : MulInstruction
                frame.Data[frame.StackIndex - 2] = (UInt16)unchecked((UInt16)l * (UInt16)r);
        internal sealed class MulUInt32 : MulInstruction
                frame.Data[frame.StackIndex - 2] = (UInt32)unchecked((UInt32)l * (UInt32)r);
        internal sealed class MulUInt64 : MulInstruction
                frame.Data[frame.StackIndex - 2] = (UInt64)unchecked((Int16)l * (Int16)r);
        internal sealed class MulSingle : MulInstruction
                frame.Data[frame.StackIndex - 2] = (Single)((Single)l * (Single)r);
        internal sealed class MulDouble : MulInstruction
                frame.Data[frame.StackIndex - 2] = (double)l * (double)r;
                case TypeCode.Int16: return s_int16 ??= new MulInt16();
                case TypeCode.Int32: return s_int32 ??= new MulInt32();
                case TypeCode.Int64: return s_int64 ??= new MulInt64();
                case TypeCode.UInt16: return s_UInt16 ??= new MulUInt16();
                case TypeCode.UInt32: return s_UInt32 ??= new MulUInt32();
                case TypeCode.UInt64: return s_UInt64 ??= new MulUInt64();
                case TypeCode.Single: return s_single ??= new MulSingle();
                case TypeCode.Double: return s_double ??= new MulDouble();
            return "Mul()";
    internal abstract class MulOvfInstruction : Instruction
        private MulOvfInstruction()
        internal sealed class MulOvfInt32 : MulOvfInstruction
                frame.Data[frame.StackIndex - 2] = ScriptingRuntimeHelpers.Int32ToObject(checked((Int32)l * (Int32)r));
        internal sealed class MulOvfInt16 : MulOvfInstruction
                frame.Data[frame.StackIndex - 2] = (Int16)checked((Int16)l * (Int16)r);
        internal sealed class MulOvfInt64 : MulOvfInstruction
                frame.Data[frame.StackIndex - 2] = (Int64)checked((Int64)l * (Int64)r);
        internal sealed class MulOvfUInt16 : MulOvfInstruction
                frame.Data[frame.StackIndex - 2] = (UInt16)checked((UInt16)l * (UInt16)r);
        internal sealed class MulOvfUInt32 : MulOvfInstruction
                frame.Data[frame.StackIndex - 2] = (UInt32)checked((UInt32)l * (UInt32)r);
        internal sealed class MulOvfUInt64 : MulOvfInstruction
                frame.Data[frame.StackIndex - 2] = (UInt64)checked((Int16)l * (Int16)r);
        internal sealed class MulOvfSingle : MulOvfInstruction
        internal sealed class MulOvfDouble : MulOvfInstruction
                case TypeCode.Int16: return s_int16 ??= new MulOvfInt16();
                case TypeCode.Int32: return s_int32 ??= new MulOvfInt32();
                case TypeCode.Int64: return s_int64 ??= new MulOvfInt64();
                case TypeCode.UInt16: return s_UInt16 ??= new MulOvfUInt16();
                case TypeCode.UInt32: return s_UInt32 ??= new MulOvfUInt32();
                case TypeCode.UInt64: return s_UInt64 ??= new MulOvfUInt64();
                case TypeCode.Single: return s_single ??= new MulOvfSingle();
                case TypeCode.Double: return s_double ??= new MulOvfDouble();
            return "MulOvf()";
