using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace V3Lib.Srd.BlockTypes
{
    public sealed class RsfBlock : Block
    {
        public readonly int ExpectedMagic = 0x24525346;
        public int Magic;
        public int Unknown14;
        public int Unknown18;
        public int Unknown1C;
        public string FolderName;

        public override void DeserializeData(byte[] rawData, string srdiPath, string srdvPath)
        {
            using BinaryReader reader = new(new MemoryStream(rawData));

            Magic = reader.ReadInt32BE(); // Now in Big Endian?

            if(Magic != ExpectedMagic)
            {
                Console.WriteLine("Unexpected magic for RSF: " + Magic + " VS " + ExpectedMagic);
            }

            Unknown14 = reader.ReadInt32();
            Unknown18 = reader.ReadInt32();
            Unknown1C = reader.ReadInt32();
            FolderName = Utils.ReadNullTerminatedString(reader, Encoding.ASCII);
        }

        public override byte[] SerializeData(string srdiPath, string srdvPath)
        {
            using MemoryStream ms = new();
            using BinaryWriter writer = new(ms);

            writer.Write(Magic);
            writer.Write(Unknown14);
            writer.Write(Unknown18);
            writer.Write(Unknown1C);
            writer.Write(Encoding.ASCII.GetBytes(FolderName));
            writer.Write((byte)0);  // Null terminator

            byte[] result = ms.ToArray();
            return result;
        }
        
        public override string GetInfo()
        {
            StringBuilder sb = new();

            sb.Append($"{nameof(Magic)}: {Magic}\n");
            sb.Append($"{nameof(Unknown14)}: {Unknown14}\n");
            sb.Append($"{nameof(Unknown18)}: {Unknown18}\n");
            sb.Append($"{nameof(Unknown1C)}: {Unknown1C}\n");
            sb.Append($"Folder Name: {FolderName}");

            return sb.ToString();
        }
    }
}
