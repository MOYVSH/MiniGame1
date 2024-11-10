using System;
using System.Collections.Generic;
using System.IO;

namespace MOYV
{
    public interface IBinarySerializable
    {
        byte[] ToBytes();
        void ToBytes(BinaryWriter bw);

        void FromBytes(byte[] data);
        void FromBytes(BinaryReader br);
    }


    public abstract class BinaryObject<T> : IBinarySerializable
    {
        public virtual T Value { get; set; }

        public virtual byte[] ToBytes()
        {
            using (var mm = new MemoryStream())
            {
                using (var bw = new BinaryWriter(mm))
                {
                    ToBytes(bw);
                    return mm.ToArray();
                }
            }
        }

        public abstract void ToBytes(BinaryWriter bw);


        public virtual void FromBytes(byte[] data)
        {
            using (var mm = new MemoryStream(data))
            {
                using (var br = new BinaryReader(mm))
                {
                    FromBytes(br);
                }
            }
        }


        public abstract void FromBytes(BinaryReader br);

        public static T FromRawBytes(byte[] data)
        {
            var t = Activator.CreateInstance<T>();
            (t as IBinarySerializable).FromBytes(data);
            return t;
        }
    }


    public abstract class BinaryList<T> : BinaryObject<List<T>>
    {
        private List<T> valueList;

        public override List<T> Value
        {
            get
            {
                if (valueList == null)
                {
                    valueList = new List<T>();
                }

                return valueList;
            }
            set => valueList = value;
        }

        public override void ToBytes(BinaryWriter bw)
        {
            bw.Write(Value.Count);
            for (int i = 0; i < Value.Count; i++)
            {
                WriteItem(Value[i], bw);
            }
        }

        public override void FromBytes(BinaryReader br)
        {
            var c = br.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                ReadItem(i, br);
            }
        }

        protected abstract void WriteItem(T item, BinaryWriter bw);
        protected abstract void ReadItem(int index, BinaryReader br);
    }
}