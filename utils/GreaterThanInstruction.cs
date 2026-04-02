    internal abstract class GreaterThanInstruction : Instruction
        private static Instruction s_SByte, s_int16, s_char, s_int32, s_int64, s_byte, s_UInt16, s_UInt32, s_UInt64, s_single, s_double;
        private GreaterThanInstruction()
        internal sealed class GreaterThanSByte : GreaterThanInstruction
                sbyte right = (sbyte)frame.Pop();
                frame.Push(((sbyte)frame.Pop()) > right);
        internal sealed class GreaterThanInt16 : GreaterThanInstruction
                Int16 right = (Int16)frame.Pop();
                frame.Push(((Int16)frame.Pop()) > right);
        internal sealed class GreaterThanChar : GreaterThanInstruction
                char right = (char)frame.Pop();
                frame.Push(((char)frame.Pop()) > right);
        internal sealed class GreaterThanInt32 : GreaterThanInstruction
                Int32 right = (Int32)frame.Pop();
                frame.Push(((Int32)frame.Pop()) > right);
        internal sealed class GreaterThanInt64 : GreaterThanInstruction
                Int64 right = (Int64)frame.Pop();
                frame.Push(((Int64)frame.Pop()) > right);
        internal sealed class GreaterThanByte : GreaterThanInstruction
                byte right = (byte)frame.Pop();
                frame.Push(((byte)frame.Pop()) > right);
        internal sealed class GreaterThanUInt16 : GreaterThanInstruction
                UInt16 right = (UInt16)frame.Pop();
                frame.Push(((UInt16)frame.Pop()) > right);
        internal sealed class GreaterThanUInt32 : GreaterThanInstruction
                UInt32 right = (UInt32)frame.Pop();
                frame.Push(((UInt32)frame.Pop()) > right);
        internal sealed class GreaterThanUInt64 : GreaterThanInstruction
                UInt64 right = (UInt64)frame.Pop();
                frame.Push(((UInt64)frame.Pop()) > right);
        internal sealed class GreaterThanSingle : GreaterThanInstruction
                Single right = (Single)frame.Pop();
                frame.Push(((Single)frame.Pop()) > right);
        internal sealed class GreaterThanDouble : GreaterThanInstruction
                double right = (double)frame.Pop();
                frame.Push(((double)frame.Pop()) > right);
                case TypeCode.SByte: return s_SByte ??= new GreaterThanSByte();
                case TypeCode.Byte: return s_byte ??= new GreaterThanByte();
                case TypeCode.Char: return s_char ??= new GreaterThanChar();
                case TypeCode.Int16: return s_int16 ??= new GreaterThanInt16();
                case TypeCode.Int32: return s_int32 ??= new GreaterThanInt32();
                case TypeCode.Int64: return s_int64 ??= new GreaterThanInt64();
                case TypeCode.UInt16: return s_UInt16 ??= new GreaterThanUInt16();
                case TypeCode.UInt32: return s_UInt32 ??= new GreaterThanUInt32();
                case TypeCode.UInt64: return s_UInt64 ??= new GreaterThanUInt64();
                case TypeCode.Single: return s_single ??= new GreaterThanSingle();
                case TypeCode.Double: return s_double ??= new GreaterThanDouble();
            return "GreaterThan()";
