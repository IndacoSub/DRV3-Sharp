﻿using System;
using System.Collections.Generic;
using System.Text;

namespace V3Lib.Srd.BlockTypes
{
    public sealed class Ct0Block : Block
    {
        public override void DeserializeData(byte[] rawData, string srdiPath, string srdvPath)
        {
            // CT0 is header-only, without actual data
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
