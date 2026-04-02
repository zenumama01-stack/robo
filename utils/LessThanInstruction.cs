    internal abstract class LessThanInstruction : Instruction
        private LessThanInstruction()
        internal sealed class LessThanSByte : LessThanInstruction
                frame.Push(((sbyte)frame.Pop()) < right);
        internal sealed class LessThanInt16 : LessThanInstruction
                frame.Push(((Int16)frame.Pop()) < right);
        internal sealed class LessThanChar : LessThanInstruction
                frame.Push(((char)frame.Pop()) < right);
        internal sealed class LessThanInt32 : LessThanInstruction
                frame.Push(((Int32)frame.Pop()) < right);
        internal sealed class LessThanInt64 : LessThanInstruction
                frame.Push(((Int64)frame.Pop()) < right);
        internal sealed class LessThanByte : LessThanInstruction
                frame.Push(((byte)frame.Pop()) < right);
        internal sealed class LessThanUInt16 : LessThanInstruction
                frame.Push(((UInt16)frame.Pop()) < right);
        internal sealed class LessThanUInt32 : LessThanInstruction
                frame.Push(((UInt32)frame.Pop()) < right);
        internal sealed class LessThanUInt64 : LessThanInstruction
                frame.Push(((UInt64)frame.Pop()) < right);
        internal sealed class LessThanSingle : LessThanInstruction
                frame.Push(((Single)frame.Pop()) < right);
        internal sealed class LessThanDouble : LessThanInstruction
                frame.Push(((double)frame.Pop()) < right);
                case TypeCode.SByte: return s_SByte ??= new LessThanSByte();
                case TypeCode.Byte: return s_byte ??= new LessThanByte();
                case TypeCode.Char: return s_char ??= new LessThanChar();
                case TypeCode.Int16: return s_int16 ??= new LessThanInt16();
                case TypeCode.Int32: return s_int32 ??= new LessThanInt32();
                case TypeCode.Int64: return s_int64 ??= new LessThanInt64();
                case TypeCode.UInt16: return s_UInt16 ??= new LessThanUInt16();
                case TypeCode.UInt32: return s_UInt32 ??= new LessThanUInt32();
                case TypeCode.UInt64: return s_UInt64 ??= new LessThanUInt64();
                case TypeCode.Single: return s_single ??= new LessThanSingle();
                case TypeCode.Double: return s_double ??= new LessThanDouble();
            return "LessThan()";
