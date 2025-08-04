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
    public unsafe struct String256 : IBinarySerializable, ISpanStringType
    {
        [FieldOffset(0)] internal InlineString256 e0;

        [FieldOffset(0)] [HideInInspector] public int length;

        [FieldOffset(4)] [SerializeField] public fixed char chars[126];


        public String256(string str)
        {
            e0 = default;

            var span = e0.AsSpan<InlineString256, char>(sizeof(String256) / 2);
            length = StringTypeUtils.CharsFromString<String256>(str, span);
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

        public static implicit operator string(String256 str)
        {
            return new string(str.AsCharSpan());
        }

        public static implicit operator String256(string str)
        {
            return new String256(str);
        }
    }
}