    internal abstract class SubInstruction : Instruction
        private SubInstruction()
        internal sealed class SubInt32 : SubInstruction
                frame.Data[frame.StackIndex - 2] = ScriptingRuntimeHelpers.Int32ToObject(unchecked((Int32)l - (Int32)r));
        internal sealed class SubInt16 : SubInstruction
                frame.Data[frame.StackIndex - 2] = (Int16)unchecked((Int16)l - (Int16)r);
        internal sealed class SubInt64 : SubInstruction
                frame.Data[frame.StackIndex - 2] = (Int64)unchecked((Int64)l - (Int64)r);
        internal sealed class SubUInt16 : SubInstruction
                frame.Data[frame.StackIndex - 2] = (UInt16)unchecked((UInt16)l - (UInt16)r);
        internal sealed class SubUInt32 : SubInstruction
                frame.Data[frame.StackIndex - 2] = (UInt32)unchecked((UInt32)l - (UInt32)r);
        internal sealed class SubUInt64 : SubInstruction
                frame.Data[frame.StackIndex - 2] = (UInt64)unchecked((Int16)l - (Int16)r);
        internal sealed class SubSingle : SubInstruction
                frame.Data[frame.StackIndex - 2] = (Single)((Single)l - (Single)r);
        internal sealed class SubDouble : SubInstruction
                frame.Data[frame.StackIndex - 2] = (double)l - (double)r;
                case TypeCode.Int16: return s_int16 ??= new SubInt16();
                case TypeCode.Int32: return s_int32 ??= new SubInt32();
                case TypeCode.Int64: return s_int64 ??= new SubInt64();
                case TypeCode.UInt16: return s_UInt16 ??= new SubUInt16();
                case TypeCode.UInt32: return s_UInt32 ??= new SubUInt32();
                case TypeCode.UInt64: return s_UInt64 ??= new SubUInt64();
                case TypeCode.Single: return s_single ??= new SubSingle();
                case TypeCode.Double: return s_double ??= new SubDouble();
            return "Sub()";
    internal abstract class SubOvfInstruction : Instruction
        private SubOvfInstruction()
        internal sealed class SubOvfInt32 : SubOvfInstruction
                frame.Data[frame.StackIndex - 2] = ScriptingRuntimeHelpers.Int32ToObject(checked((Int32)l - (Int32)r));
        internal sealed class SubOvfInt16 : SubOvfInstruction
                frame.Data[frame.StackIndex - 2] = (Int16)checked((Int16)l - (Int16)r);
        internal sealed class SubOvfInt64 : SubOvfInstruction
                frame.Data[frame.StackIndex - 2] = (Int64)checked((Int64)l - (Int64)r);
        internal sealed class SubOvfUInt16 : SubOvfInstruction
                frame.Data[frame.StackIndex - 2] = (UInt16)checked((UInt16)l - (UInt16)r);
        internal sealed class SubOvfUInt32 : SubOvfInstruction
                frame.Data[frame.StackIndex - 2] = (UInt32)checked((UInt32)l - (UInt32)r);
        internal sealed class SubOvfUInt64 : SubOvfInstruction
                frame.Data[frame.StackIndex - 2] = (UInt64)checked((Int16)l - (Int16)r);
        internal sealed class SubOvfSingle : SubOvfInstruction
        internal sealed class SubOvfDouble : SubOvfInstruction
                case TypeCode.Int16: return s_int16 ??= new SubOvfInt16();
                case TypeCode.Int32: return s_int32 ??= new SubOvfInt32();
                case TypeCode.Int64: return s_int64 ??= new SubOvfInt64();
                case TypeCode.UInt16: return s_UInt16 ??= new SubOvfUInt16();
                case TypeCode.UInt32: return s_UInt32 ??= new SubOvfUInt32();
                case TypeCode.UInt64: return s_UInt64 ??= new SubOvfUInt64();
                case TypeCode.Single: return s_single ??= new SubOvfSingle();
                case TypeCode.Double: return s_double ??= new SubOvfDouble();
            return "SubOvf()";
