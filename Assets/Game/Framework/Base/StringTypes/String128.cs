using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MOYV.StringTypes.InlineStructs;
using Unity.Collections.LowLevel.Unsafe;

namespace MOYV.StringTypes
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct String128 : IBinarySerializable, ISpanStringType
    {
        [FieldOffset(0)] internal InlineString128 e0;

        [FieldOffset(0)] public int length;

        public String128(string str)
        {
            e0 = default;
            var span = e0.AsSpan<InlineString128, char>(sizeof(String128)/2);
            length = StringTypeUtils.CharsFromString<String128>(str, span);
        }

        public int Length
        {
            get => length;
            set => length = value;
        }

        public ref char this[int index]
        {
            get
            {
                var span = this.AsCharSpan();
                return ref span[index];
            }
        }


        public byte[] ToBytes()
        {
            return this.AsByteSpan().ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBytes(byte[] bytes)
        {
            this.LoadFromBytes(bytes);
        }

        public void ToBytes(BinaryWriter bw)
        {
            bw.Write(this.AsByteSpan());
        }

        public void FromBytes(BinaryReader br)
        {
            length = br.ReadInt32();
            var bytes = br.ReadBytes(length);
            FromBytes(bytes);
        }

        public override string ToString()
        {
            return new string(this.AsCharSpan());
        }
        
        public char[] ToChars()
        {
            return this.AsCharSpan().ToArray();
        }
        
        public static implicit operator string(String128 str)
        {
            return str.ToString();
        }
        
        public static implicit operator String128(string str)
        {
            return new String128(str);
        }
    }
}