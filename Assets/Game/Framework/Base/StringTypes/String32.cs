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
    public unsafe struct String32 : IBinarySerializable, ISpanStringType
    {
        [FieldOffset(0)] [NonSerialized] public long l0;
        [FieldOffset(8)] [NonSerialized] public long l1;
        [FieldOffset(16)] [NonSerialized] public long l2;
        [FieldOffset(24)] [NonSerialized] public long l3;

        [FieldOffset(0)] [HideInInspector] [SerializeField]
        public int length;

        [FieldOffset(4)] [SerializeField] public fixed char chars[14];

        public String32(string str)
        {
            l0 = 0;
            l1 = 0;
            l2 = 0;
            l3 = 0;

            var span = l0.AsSpan<long, char>(sizeof(String32) / 2);
            length = StringTypeUtils.CharsFromString<String32>(str, span);
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

        public static implicit operator String32(string str)
        {
            return new String32(str);
        }

        public static implicit operator string(String32 str)
        {
            return str.ToString();
        }
    }
}