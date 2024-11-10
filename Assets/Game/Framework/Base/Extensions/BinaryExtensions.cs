using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MOYV
{
    public interface IBinaryData
    {
        byte[] ToBytes();

        void FromBytes(byte[] bytes);
    }

    public interface IBinaryStreamData
    {
        void ToBytes(BinaryWriter bw);

        void FromBytes(BinaryReader br);
    }

    public static class BinaryExtensions
    {
        public static void WriteTo(this Rect rect, BinaryWriter writer)
        {
            writer.Write(rect.x);
            writer.Write(rect.y);
            writer.Write(rect.width);
            writer.Write(rect.height);
        }

        public static void WriteTo(this Vector2 vector, BinaryWriter writer)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
        }

        public static void WriteTo(this Vector4 vector, BinaryWriter writer)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
            writer.Write(vector.w);
        }

        public static void WriteTo(this Color color, BinaryWriter writer)
        {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }

        public static void WriteTo(this Color32 color, BinaryWriter writer)
        {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }

        public static void WriteTo(this byte[] bytes, BinaryWriter writer)
        {
            if (bytes == null)
            {
                writer.Write(0);
                return;
            }

            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        public static void WriteTo<T>(this T[] items, BinaryWriter writer) where T : IBinaryStreamData
        {
            if (items == null)
            {
                writer.Write(0);
                return;
            }

            writer.Write(items.Length);
            for (int i = 0; i < items.Length; i++)
            {
                items[i].ToBytes(writer);
            }
        }

        public static void WriteTo<T>(this List<T> items, BinaryWriter writer) where T : IBinaryStreamData
        {
            if (items == null)
            {
                writer.Write(0);
                return;
            }

            writer.Write(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                items[i].ToBytes(writer);
            }
        }

        public static void WriteTo<T>(this List<T> items, BinaryWriter writer, Action<T, BinaryWriter> action)
        {
            if (items == null || action == null)
            {
                writer.Write(0);
                return;
            }

            writer.Write(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                action(items[i], writer);
            }
        }

        public static void WriteTo<T>(this T[] items, BinaryWriter writer, Action<T, BinaryWriter> action)
        {
            if (items == null || action == null)
            {
                writer.Write(0);
                return;
            }

            writer.Write(items.Length);
            for (int i = 0; i < items.Length; i++)
            {
                action(items[i], writer);
            }
        }

        public static Rect ReadRect(this BinaryReader br)
        {
            Rect rect = default;
            rect.x = br.ReadSingle();
            rect.y = br.ReadSingle();
            rect.width = br.ReadSingle();
            rect.height = br.ReadSingle();
            return rect;
        }

        public static Vector2 ReadVector2(this BinaryReader br)
        {
            Vector2 vector = default;
            vector.x = br.ReadSingle();
            vector.y = br.ReadSingle();
            return vector;
        }

        public static Vector4 ReadVector4(this BinaryReader br)
        {
            Vector4 vector = default;
            vector.x = br.ReadSingle();
            vector.y = br.ReadSingle();
            vector.z = br.ReadSingle();
            vector.w = br.ReadSingle();
            return vector;
        }

        public static Color ReadColor(this BinaryReader br)
        {
            Color color;
            color.r = br.ReadSingle();
            color.g = br.ReadSingle();
            color.b = br.ReadSingle();
            color.a = br.ReadSingle();
            return color;
        }

        public static Color32 ReadColor32(this BinaryReader br)
        {
            Color32 color = default;
            color.r = br.ReadByte();
            color.g = br.ReadByte();
            color.b = br.ReadByte();
            color.a = br.ReadByte();
            return color;
        }

        public static T[] ReadArray<T>(this BinaryReader br) where T : IBinaryStreamData
        {
            int count = br.ReadInt32();
            T[] items = new T[count];
            for (int i = 0; i < count; i++)
            {
                var instance = Activator.CreateInstance<T>();
                instance.FromBytes(br);
                items[i] = instance;
            }

            return items;
        }

        public static List<T> ReadList<T>(this BinaryReader br) where T : IBinaryStreamData
        {
            int count = br.ReadInt32();
            List<T> items = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                var instance = Activator.CreateInstance<T>();
                instance.FromBytes(br);
                items.Add(instance);
            }

            return items;
        }

        public static T[] ReadArray<T>(this BinaryReader br, Func<BinaryReader, T> action)
        {
            int count = br.ReadInt32();
            T[] items = new T[count];
            for (int i = 0; i < count; i++)
            {
                var instance = action(br);
                items[i] = instance;
            }

            return items;
        }

        public static List<T> ReadList<T>(this BinaryReader br, Func<BinaryReader, T> action)
        {
            int count = br.ReadInt32();
            List<T> items = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                var instance = action(br);
                items.Add(instance);
            }

            return items;
        }

        public static byte[] ReadBytes(this BinaryReader br)
        {
            byte[] bytes = null;
            int contentLength = br.ReadInt32();
            if (contentLength > 0)
            {
                bytes = br.ReadBytes(contentLength);
            }

            return bytes;
        }
    }
}