using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MOYV.StringTypes.InlineStructs;
using UnityEngine;

namespace MOYV.StringTypes
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct String4096 : IBinarySerializable, ISpanStringType
    {
        [FieldOffset(0)] internal InlineString1024 e0;
        [FieldOffset(1024)] internal InlineString1024 e1;
        [FieldOffset(2048)] internal InlineString1024 e2;
        [FieldOffset(3072)] internal InlineString1024 e3;

        [FieldOffset(0)] [HideInInspector] [SerializeField]
        public int length;

        [SerializeField] [FieldOffset(4)] public fixed char chars[2046];

        public String4096(string str)
        {
            e0 = default;
            e1 = default;
            e2 = default;
            e3 = default;

            var span = e0.AsSpan<InlineString1024, char>(sizeof(String4096) / 2);
            length = StringTypeUtils.CharsFromString<String4096>(str, span);
        }

        public ref char this[int index]
        {
            get
            {
                var span = this.AsCharSpan();
                return ref span[index];
            }
        }

        public int Length
        {
            get => length;
            set => length = value;
        }

        public char[] ToChars()
        {
            return this.AsCharSpan().ToArray();
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

        public static implicit operator string(String4096 str)
        {
            return new string(str.AsCharSpan());
        }

        public static implicit operator String4096(string str)
        {
            return new String4096(str);
        }
    }
}