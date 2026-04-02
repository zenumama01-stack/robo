    internal abstract class NotEqualInstruction : Instruction
        private NotEqualInstruction()
        internal sealed class NotEqualBoolean : NotEqualInstruction
                frame.Push(((bool)frame.Pop()) != ((bool)frame.Pop()));
        internal sealed class NotEqualSByte : NotEqualInstruction
                frame.Push(((sbyte)frame.Pop()) != ((sbyte)frame.Pop()));
        internal sealed class NotEqualInt16 : NotEqualInstruction
                frame.Push(((Int16)frame.Pop()) != ((Int16)frame.Pop()));
        internal sealed class NotEqualChar : NotEqualInstruction
                frame.Push(((char)frame.Pop()) != ((char)frame.Pop()));
        internal sealed class NotEqualInt32 : NotEqualInstruction
                frame.Push(((Int32)frame.Pop()) != ((Int32)frame.Pop()));
        internal sealed class NotEqualInt64 : NotEqualInstruction
                frame.Push(((Int64)frame.Pop()) != ((Int64)frame.Pop()));
        internal sealed class NotEqualByte : NotEqualInstruction
                frame.Push(((byte)frame.Pop()) != ((byte)frame.Pop()));
        internal sealed class NotEqualUInt16 : NotEqualInstruction
                frame.Push(((UInt16)frame.Pop()) != ((UInt16)frame.Pop()));
        internal sealed class NotEqualUInt32 : NotEqualInstruction
                frame.Push(((UInt32)frame.Pop()) != ((UInt32)frame.Pop()));
        internal sealed class NotEqualUInt64 : NotEqualInstruction
                frame.Push(((UInt64)frame.Pop()) != ((UInt64)frame.Pop()));
        internal sealed class NotEqualSingle : NotEqualInstruction
                frame.Push(((Single)frame.Pop()) != ((Single)frame.Pop()));
        internal sealed class NotEqualDouble : NotEqualInstruction
                frame.Push(((double)frame.Pop()) != ((double)frame.Pop()));
        internal sealed class NotEqualReference : NotEqualInstruction
                frame.Push(frame.Pop() != frame.Pop());
                case TypeCode.Boolean: return s_boolean ??= new NotEqualBoolean();
                case TypeCode.SByte: return s_SByte ??= new NotEqualSByte();
                case TypeCode.Byte: return s_byte ??= new NotEqualByte();
                case TypeCode.Char: return s_char ??= new NotEqualChar();
                case TypeCode.Int16: return s_int16 ??= new NotEqualInt16();
                case TypeCode.Int32: return s_int32 ??= new NotEqualInt32();
                case TypeCode.Int64: return s_int64 ??= new NotEqualInt64();
                case TypeCode.UInt16: return s_UInt16 ??= new NotEqualInt16();
                case TypeCode.UInt32: return s_UInt32 ??= new NotEqualInt32();
                case TypeCode.UInt64: return s_UInt64 ??= new NotEqualInt64();
                case TypeCode.Single: return s_single ??= new NotEqualSingle();
                case TypeCode.Double: return s_double ??= new NotEqualDouble();
                        return s_reference ??= new NotEqualReference();
            return "NotEqual()";
