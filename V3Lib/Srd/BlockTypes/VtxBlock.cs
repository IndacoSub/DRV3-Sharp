﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SystemHalf;

namespace V3Lib.Srd.BlockTypes
{
    // Holds information about vertex data and index lists
    public sealed class VtxBlock : Block
    {
        public int FloatTripletCount;   // Likely the number of half-float triplets in the "float list"
        public short Unknown14;
        public short Unknown16;
        public int VertexCount;
        public short Unknown1C;
        public byte Unknown1E;
        public byte VertexSubBlockCount;
        public short BindBoneRootOffset;
        public short VertexSubBlockListOffset;
        public short FloatListOffset;
        public short BindBoneListOffset;
        public short Unknown28;
        public List<short> UnknownShortList;
        public List<(int Offset, int Size)> VertexSubBlockList;
        public short BindBoneRoot;
        public List<short> BindBoneList;
        public List<(float f1, float f2, float f3)> UnknownFloatList;
        public List<string> UnknownStringList;

        public override void DeserializeData(byte[] rawData)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(rawData));

            FloatTripletCount = reader.ReadInt32();
            Unknown14 = reader.ReadInt16();
            Unknown16 = reader.ReadInt16();
            VertexCount = reader.ReadInt32();
            Unknown1C = reader.ReadInt16();
            Unknown1E = reader.ReadByte();
            VertexSubBlockCount = reader.ReadByte();
            BindBoneRootOffset = reader.ReadInt16();
            VertexSubBlockListOffset = reader.ReadInt16();
            FloatListOffset = reader.ReadInt16();
            BindBoneListOffset = reader.ReadInt16();
            Unknown28 = reader.ReadInt16();
            Utils.ReadPadding(ref reader, 16);

            // Read unknown list of shorts
            UnknownShortList = new List<short>();
            while (reader.BaseStream.Position < VertexSubBlockListOffset)
            {
                UnknownShortList.Add(reader.ReadInt16());
            }

            // Read vertex sub-blocks
            reader.BaseStream.Seek(VertexSubBlockListOffset, SeekOrigin.Begin);
            VertexSubBlockList = new List<(int Offset, int Size)>();
            for (int s = 0; s < VertexSubBlockCount; ++s)
            {
                VertexSubBlockList.Add((reader.ReadInt32(), reader.ReadInt32()));
            }

            // Read bone list
            reader.BaseStream.Seek(BindBoneRootOffset, SeekOrigin.Begin);
            BindBoneRoot = reader.ReadInt16();

            if (BindBoneListOffset != 0)
                reader.BaseStream.Seek(BindBoneListOffset, SeekOrigin.Begin);

            BindBoneList = new List<short>();
            while (reader.BaseStream.Position < FloatListOffset)
            {
                BindBoneList.Add(reader.ReadInt16());
            }

            // Read unknown list of floats
            reader.BaseStream.Seek(FloatListOffset, SeekOrigin.Begin);
            UnknownFloatList = new List<(float f1, float f2, float f3)>();
            for (int h = 0; h < FloatTripletCount / 2; ++h)
            {
                var floatTriplet = (
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()
                    );

                UnknownFloatList.Add(floatTriplet);
            }

            // Read unknown string data
            UnknownStringList = new List<string>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                UnknownStringList.Add(Utils.ReadNullTerminatedString(ref reader, new ASCIIEncoding()));
            }

            reader.Close();
            reader.Dispose();
        }

        public override byte[] SerializeData()
        {
            throw new NotImplementedException();
        }

        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"{nameof(FloatTripletCount)}: {FloatTripletCount}\n");
            sb.Append($"{nameof(Unknown14)}: {Unknown14}\n");
            sb.Append($"{nameof(Unknown16)}: {Unknown16}\n");
            sb.Append($"{nameof(VertexCount)}: {VertexCount}\n");
            sb.Append($"{nameof(Unknown1C)}: {Unknown1C}\n");
            sb.Append($"{nameof(Unknown1E)}: {Unknown1E}\n");
            sb.Append($"{nameof(BindBoneRootOffset)}: {BindBoneRootOffset}\n");
            sb.Append($"{nameof(VertexSubBlockListOffset)}: {VertexSubBlockListOffset}\n");
            sb.Append($"{nameof(FloatListOffset)}: {FloatListOffset}\n");
            sb.Append($"{nameof(BindBoneListOffset)}: {BindBoneListOffset}\n");
            sb.Append($"{nameof(Unknown28)}: {Unknown28}\n");

            sb.Append($"{nameof(UnknownShortList)}: ");
            sb.AppendJoin(", ", UnknownShortList);
            sb.Append('\n');

            sb.Append($"{nameof(VertexSubBlockList)}: ");
            sb.AppendJoin(", ", VertexSubBlockList);
            sb.Append('\n');
            
            sb.Append($"{nameof(BindBoneRoot)}: {BindBoneRoot}\n");
            sb.Append($"{nameof(BindBoneList)}: ");
            sb.AppendJoin(", ", BindBoneList);
            sb.Append('\n');

            sb.Append($"{nameof(UnknownFloatList)}: ");
            sb.AppendJoin(", ", UnknownFloatList);
            sb.Append('\n');

            sb.Append($"{nameof(UnknownStringList)}: ");
            sb.AppendJoin(", ", UnknownStringList);

            return sb.ToString();
        }
    }
}
