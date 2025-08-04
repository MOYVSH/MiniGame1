using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MOYV.StringTypes.InlineStructs;
using Unity.Collections.LowLevel.Unsafe;

namespace MOYV.StringTypes
{
    public unsafe static class StringTypeUtils
    {
        public static ISpanStringType ToSpanString(this string str)
        {
            var length = str.Length;

            switch (length)
            {
                case <= 6:
                    return new String16(str);
                case <= 14:
                    return new String32(str);
                case <= 30:
                    return new String64(str);
                case <= 62:
                    return new String128(str);
                case <= 126:
                    return new String256(str);
                case <= 254:
                    return new String512(str);
                case <= 510:
                    return new String1024(str);
                case <= 1022:
                    return new String2048(str);
                case <= 2046:
                    return new String4096(str);
                default:
                    throw new InvalidOperationException($"{length} > 2046  too long!\n{str} ");
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<Char> AsCharSpan<T>(this ref T buffer) where T : struct, ISpanStringType
        {
            var charLength = buffer.Length / 2;
            return buffer.AsSpan<T, char>(charLength + 2).Slice(2, charLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadFromBytes<T>(this ref T buffer, byte[] bytes) where T : struct, ISpanStringType
        {
            buffer.Length = bytes.Length;
            var span = buffer.AsByteSpan();
            bytes.CopyTo(span.Slice(4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> AsByteSpan<T>(this ref T buffer) where T : struct, ISpanStringType
        {
            return buffer.AsSpan<T, byte>(buffer.Length + 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int CharsFromString<T>(string src, Span<char> dst)
        {
            if (src.Length > dst.Length - 2)
            {
                throw new InvalidOperationException(
                    $"{typeof(T).Name}: {src.Length} > {dst.Length}  too long! \n{src}");
            }

            Span<Char> chars = src.ToCharArray();
            chars.CopyTo(dst.Slice(2));

            return chars.Length * 2;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MemoryCopy(string src, char[] dst)
        {
            var length = src.Length * 2;
            fixed (char* ptr = src.ToCharArray())
            {
                fixed (char* bptr = dst)
                {
                    Buffer.MemoryCopy(ptr, bptr, length, length);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<TElement> AsSpan<TBuffer, TElement>(this ref TBuffer buffer, int length)
            where TBuffer : struct
        {
            return MemoryMarshal.CreateSpan(ref UnsafeUtility.As<TBuffer, TElement>(ref buffer), length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref TElement FirstElementRef<TBuffer, TElement>(ref TBuffer buffer)
        {
            return ref UnsafeUtility.As<TBuffer, TElement>(ref buffer);
        }

        public static void TryMemoryCopy<T>(string src, char[] dst, int maxLength)
        {
            var length = src.Length * 2;
            if (length > maxLength)
            {
                throw new InvalidOperationException($"{typeof(T).Name}: {src} {length} > {maxLength}  too long!");
            }

            MemoryCopy(src, dst);
        }
    }
}