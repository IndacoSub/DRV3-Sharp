using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace V3Lib.Srd.BlockTypes
{
    public sealed class CfhBlock : Block
    {
        public readonly int ExpectedMagic = 0x24434648;
        public int Magic = 0;
        public int Unk1 = 0;
        public int Unk2 = 0;
        public int Unk3 = 0;

        public override void DeserializeData(byte[] rawData, string srdiPath, string srdvPath)
        {
            Console.WriteLine("CFH Data length: " + rawData.Length);

            if(rawData.Length < 16)
            {
                Console.WriteLine("Invalid CFH data length: " + rawData.Length);
                //return;
            }

            using BinaryReader reader = new(new MemoryStream(rawData));

            Magic = reader.ReadInt32BE(); // Now in Big Endian?
            Unk1 = reader.ReadInt32();
            Unk2 = reader.ReadInt32();
            Unk3 = reader.ReadInt32();

            if(Magic != ExpectedMagic)
            {
                Console.WriteLine("Wrong SRD magic!");
            }
            Console.WriteLine("Unk1: " + Unk1.ToString());
            Console.WriteLine("Unk2: " + Unk2.ToString());
            Console.WriteLine("Unk3: " + Unk3.ToString());
            return;
        }

        public override byte[] SerializeData(string srdiPath, string srdvPath)
        {
            return Array.Empty<byte>();
        }
        
        public override string GetInfo()
        {
            return "";
        }
    }
}
