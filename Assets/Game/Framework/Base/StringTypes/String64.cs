using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MOYV.StringTypes.InlineStructs;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MOYV.StringTypes
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct String64 : IBinarySerializable, ISpanStringType
    {
        [FieldOffset(0)] internal InlineString64 e0;

        [FieldOffset(0)] [HideInInspector] [SerializeField]
        public int length;


        [FieldOffset(4)] [SerializeField] public fixed char chars[30];

        public String64(string str)
        {
            e0 = default;

            var span = e0.AsSpan<InlineString64, char>(sizeof(String64) / 2);
            length = StringTypeUtils.CharsFromString<String64>(str, span);
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

        public static implicit operator String64(string str)
        {
            return new String64(str);
        }

        public static implicit operator string(String64 str)
        {
            return str.ToString();
        }
    }
}