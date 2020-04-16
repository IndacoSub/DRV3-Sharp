﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using V3Lib;
using V3Lib.Srd;
using V3Lib.Srd.BlockTypes;

namespace SrdTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SRD Tool by CaptainSwag101\n" +
                "Version 0.0.4, built on 2020-04-10\n");

            if (args.Length != 1)
            {
                Console.WriteLine("Usage: SrdTool.exe <SRD file to analyze>");
                return;
            }

            FileInfo info = new FileInfo(args[0]);
            if (!info.Exists)
            {
                Console.WriteLine($"ERROR: \"{args[0]}\" does not exist.");
                return;
            }

            if (info.Extension.ToLower() != ".srd")
            {
                Console.WriteLine("ERROR: Input file does not have the \".srd\" extension.");
                return;
            }

            SrdFile srd = new SrdFile();
            srd.Load(args[0]);

            // Search for linked files like SRDI and SRDV and load them
            byte[] srdi = new byte[0];
            if (File.Exists(args[0] + 'i'))
                srdi = File.ReadAllBytes(args[0] + 'i');

            byte[] srdv = new byte[0];
            if (File.Exists(args[0] + 'v'))
                srdv = File.ReadAllBytes(args[0] + 'v');

            // Process commands
            while (true)
            {
                Console.Write("Type a command to perform on this SRD (print_blocks, extract_models, exit): ");
                string command = Console.ReadLine().ToLowerInvariant();

                switch (command)
                {
                    case "print_blocks":
                        Console.WriteLine($"\"{info.FullName}\" contains the following blocks:\n");
                        PrintBlocks(srd.Blocks, 0);
                        break;

                    case "extract_models":
                        // Export the vertices and faces as an ASCII OBJ
                        StringBuilder sb = new StringBuilder();
                        int totalVerticesProcessed = 0;
                        foreach (Block b in srd.Blocks)
                        {
                            if ((b is VtxBlock) && (b.Children[0] is RsiBlock))
                            {
                                VtxBlock vtx = (VtxBlock)b;
                                RsiBlock rsi = (RsiBlock)b.Children[0];

                                BinaryReader rsiReader = new BinaryReader(new MemoryStream(rsi.ResourceData));
                                BinaryReader srdiReader = new BinaryReader(new MemoryStream(srdi));

                                // Extract vertex data
                                List<float[]> vertexList = new List<float[]>();
                                List<float[]> normalList = new List<float[]>();
                                List<float[]> texmapList = new List<float[]>();
                                uint vertexBlockOffset = rsiReader.ReadUInt32() & 0x1FFFFFFF;    // NOTE: This might need to be 0x00FFFFFF
                                uint vertexBlockLength = rsiReader.ReadUInt32();

                                int combinedSize = 0;
                                foreach (var subBlock in vtx.VertexSubBlockList)
                                {
                                    combinedSize += subBlock.Size;
                                }
                                if ((vertexBlockLength / combinedSize) != vtx.VertexCount)
                                {
                                    Console.WriteLine("WARNING: Total vertex block length and expected vertex count are misaligned.");
                                }

                                for (int sbNum = 0; sbNum < vtx.VertexSubBlockCount; ++sbNum)
                                {
                                    srdiReader.BaseStream.Seek(vertexBlockOffset + vtx.VertexSubBlockList[sbNum].Offset, SeekOrigin.Begin);
                                    for (int vNum = 0; vNum < vtx.VertexCount; ++vNum)
                                    {
                                        int bytesRead = 0;
                                        switch (sbNum)
                                        {
                                            case 0: // Vertex/Normal data (and Texture UV for boneless models)
                                                {
                                                    float[] vertex = new float[3];
                                                    vertex[0] = srdiReader.ReadSingle() * -1.0f;    // X
                                                    vertex[1] = srdiReader.ReadSingle();            // Y
                                                    vertex[2] = srdiReader.ReadSingle();            // Z
                                                    vertexList.Add(vertex);

                                                    float[] normal = new float[3];
                                                    normal[0] = srdiReader.ReadSingle() * -1.0f;    // X
                                                    normal[1] = srdiReader.ReadSingle();            // Y
                                                    normal[2] = srdiReader.ReadSingle();            // Z
                                                    normalList.Add(normal);

                                                    if (vtx.VertexSubBlockCount == 1)
                                                    {
                                                        float[] texmap = new float[3];
                                                        texmap[0] = srdiReader.ReadSingle();        // U
                                                        texmap[1] = srdiReader.ReadSingle() * -1.0f;// V
                                                        texmapList.Add(texmap);
                                                        bytesRead = 32;
                                                    }
                                                    else
                                                    {
                                                        bytesRead = 24;
                                                    }

                                                    break;
                                                }

                                            case 1: // Bone weights
                                                {
                                                    
                                                }
                                                break;

                                            case 2: // Texture UVs (only for models with bones)
                                                {
                                                    float[] texmap = new float[3];
                                                    texmap[0] = srdiReader.ReadSingle();            // U
                                                    texmap[1] = srdiReader.ReadSingle() * -1.0f;    // V
                                                    texmapList.Add(texmap);
                                                    bytesRead = 8;
                                                    break;
                                                }
                                        }

                                        // Skip data we don't currently use, though I may add support for this data later
                                        srdiReader.BaseStream.Seek(vtx.VertexSubBlockList[sbNum].Size - bytesRead, SeekOrigin.Current);
                                    }
                                }

                                // Skip RSI reader to the next 16-byte aligned offset
                                Utils.ReadPadding(ref rsiReader, 16);

                                // Extract face data
                                List<ushort[]> faceList = new List<ushort[]>();
                                uint faceBlockOffset = rsiReader.ReadUInt32() & 0x1FFFFFFF;    // NOTE: This might need to be 0x00FFFFFF
                                uint faceBlockLength = rsiReader.ReadUInt32();

                                srdiReader.BaseStream.Seek(faceBlockOffset, SeekOrigin.Begin);
                                while (srdiReader.BaseStream.Position < (faceBlockOffset + faceBlockLength))
                                {
                                    ushort[] faceIndices = new ushort[3];
                                    for (int i = 0; i < 3; ++i)
                                    {
                                        ushort index = srdiReader.ReadUInt16();
                                        faceIndices[i] = index;
                                    }
                                    faceList.Add(faceIndices);
                                }

                                // Close and Dispose our reader instances, we don't need them anymore
                                srdiReader.Close();
                                rsiReader.Close();
                                srdiReader.Dispose();
                                rsiReader.Dispose();

                                

                                // Write mesh object data
                                sb.Append($"o {rsi.ResourceStrings[0]}\n");

                                int verticesProcessed = 0;
                                foreach (float[] v in vertexList)
                                {
                                    sb.Append($"v {v[0]} {v[1]} {v[2]}\n");
                                    ++verticesProcessed;
                                }
                                sb.Append("\n");
                                
                                foreach (float[] vn in normalList)
                                {
                                    sb.Append($"vn {vn[0]} {vn[1]} {vn[2]}\n");
                                }
                                sb.Append("\n");

                                
                                foreach (float[] vt in texmapList)
                                {
                                    sb.Append($"vt {vt[0]} {vt[1]}\n");
                                }
                                sb.Append("\n");

                                foreach (ushort[] f in faceList)
                                {
                                    int faceIndex1 = f[0] + totalVerticesProcessed + 1;
                                    int faceIndex2 = f[1] + totalVerticesProcessed + 1;
                                    int faceIndex3 = f[2] + totalVerticesProcessed + 1;
                                    sb.Append($"f {faceIndex1}/{faceIndex1}/{faceIndex1} {faceIndex2}/{faceIndex2}/{faceIndex2} {faceIndex3}/{faceIndex3}/{faceIndex3}\n");
                                }

                                totalVerticesProcessed += verticesProcessed;
                                sb.Append('\n');
                            }
                        }

                        string objString = sb.ToString();
                        File.WriteAllText(args[0] + ".obj", objString);
                        break;

                    case "exit":
                        return;

                    default:
                        Console.WriteLine("Invalid command.");
                        break;
                }
            }
        }

        private static void PrintBlocks(List<Block> blockList, int tabLevel)
        {
            foreach (Block block in blockList)
            {
                Console.Write(new string('\t', tabLevel));
                Console.WriteLine($"Block Type: {block.BlockType}" + ((block is UnknownBlock) ? " (unknown block type)" : ""));    // Print an extra message for debugging if the block type is considered "unknown"

                // Print block-specific info
                string[] blockInfoLines = block.GetInfo().Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in blockInfoLines)
                {
                    Console.Write(new string('\t', tabLevel + 1));
                    Console.WriteLine(line);
                }
                
                // Print child block info
                if (block.Children.Count > 0)
                {
                    Console.Write(new string('\t', tabLevel + 1));
                    Console.WriteLine($"Child Blocks: {block.Children.Count:n0}");
                    Console.WriteLine();

                    PrintBlocks(block.Children, tabLevel + 1);
                }

                Console.WriteLine();
            }
        }
    }
}
