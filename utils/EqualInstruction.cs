    internal abstract class EqualInstruction : Instruction
        // Perf: EqualityComparer<T> but is 3/2 to 2 times slower.
        private static Instruction s_reference, s_boolean, s_SByte, s_int16, s_char, s_int32, s_int64, s_byte, s_UInt16, s_UInt32, s_UInt64, s_single, s_double;
        private EqualInstruction()
        internal sealed class EqualBoolean : EqualInstruction
                frame.Push(((bool)frame.Pop()) == ((bool)frame.Pop()));
        internal sealed class EqualSByte : EqualInstruction
                frame.Push(((sbyte)frame.Pop()) == ((sbyte)frame.Pop()));
        internal sealed class EqualInt16 : EqualInstruction
                frame.Push(((Int16)frame.Pop()) == ((Int16)frame.Pop()));
        internal sealed class EqualChar : EqualInstruction
                frame.Push(((char)frame.Pop()) == ((char)frame.Pop()));
        internal sealed class EqualInt32 : EqualInstruction
                frame.Push(((Int32)frame.Pop()) == ((Int32)frame.Pop()));
        internal sealed class EqualInt64 : EqualInstruction
                frame.Push(((Int64)frame.Pop()) == ((Int64)frame.Pop()));
        internal sealed class EqualByte : EqualInstruction
                frame.Push(((byte)frame.Pop()) == ((byte)frame.Pop()));
        internal sealed class EqualUInt16 : EqualInstruction
                frame.Push(((UInt16)frame.Pop()) == ((UInt16)frame.Pop()));
        internal sealed class EqualUInt32 : EqualInstruction
                frame.Push(((UInt32)frame.Pop()) == ((UInt32)frame.Pop()));
        internal sealed class EqualUInt64 : EqualInstruction
                frame.Push(((UInt64)frame.Pop()) == ((UInt64)frame.Pop()));
        internal sealed class EqualSingle : EqualInstruction
                frame.Push(((Single)frame.Pop()) == ((Single)frame.Pop()));
        internal sealed class EqualDouble : EqualInstruction
                frame.Push(((double)frame.Pop()) == ((double)frame.Pop()));
        internal sealed class EqualReference : EqualInstruction
                frame.Push(frame.Pop() == frame.Pop());
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            // Boxed enums can be unboxed as their underlying types:
            var typeToUse = type.IsEnum ? Enum.GetUnderlyingType(type) : type;
            switch (typeToUse.GetTypeCode())
                case TypeCode.Boolean: return s_boolean ??= new EqualBoolean();
                case TypeCode.SByte: return s_SByte ??= new EqualSByte();
                case TypeCode.Byte: return s_byte ??= new EqualByte();
                case TypeCode.Char: return s_char ??= new EqualChar();
                case TypeCode.Int16: return s_int16 ??= new EqualInt16();
                case TypeCode.Int32: return s_int32 ??= new EqualInt32();
                case TypeCode.Int64: return s_int64 ??= new EqualInt64();
                case TypeCode.UInt16: return s_UInt16 ??= new EqualInt16();
                case TypeCode.UInt32: return s_UInt32 ??= new EqualInt32();
                case TypeCode.UInt64: return s_UInt64 ??= new EqualInt64();
                case TypeCode.Single: return s_single ??= new EqualSingle();
                case TypeCode.Double: return s_double ??= new EqualDouble();
                case TypeCode.Object:
                    if (!type.IsValueType)
                        return s_reference ??= new EqualReference();
                    // TODO: Nullable<T>
            return "Equal()";
