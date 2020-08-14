using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ModMaker
{
    class Program
    {
        public static List<byte> finalFile = new List<byte>();
        static void Main(string[] args)
        {
            Console.WriteLine(Properties.Resources.Banner1);
            Console.WriteLine(Properties.Resources.Banner2);
            Console.WriteLine("By kolya5544");
            Console.WriteLine();
            if (args.Length == 2)
            {
                string moddedDirectory = args[0];
                string originalDirectory = args[1];
                string[] moddedFiles = Directory.EnumerateFiles(moddedDirectory, "*", SearchOption.AllDirectories).ToArray();
                string[] originalFiles = Directory.EnumerateFiles(originalDirectory, "*", SearchOption.AllDirectories).ToArray();
                List<string> leftModded = moddedFiles.ToList();
                Console.WriteLine("Initialized successfully...");
                for (int i = 0; i<originalFiles.Length; i++)
                {
                    string Filename = originalFiles[i];
                    string AbsoluteFilename = Filename.Replace(originalDirectory, String.Empty);
                    string ModdedFilename = FindModded(moddedFiles, moddedDirectory, originalDirectory, Filename);
                    leftModded.Remove(ModdedFilename);
                    bool equal = ChecksumCollide(Filename, ModdedFilename);
                    if (!equal)
                    {
                        finalFile.AddRange(Encoding.UTF8.GetBytes(AbsoluteFilename)); finalFile.Add(0x00);
                        byte[] diff = GenerateDiff(Filename, ModdedFilename);
                        finalFile.AddRange(BitConverter.GetBytes(diff.Length));
                        finalFile.AddRange(diff);
                        Console.WriteLine("Processed " + AbsoluteFilename);
                    }
                }
                if (leftModded.Count != 0)
                {
                    Console.WriteLine("[WARNING] We've found some modded files that are not included in original. Do you want to include them in the mod?");
                    Console.Write("(Y/N):");
                    string answ = Console.ReadLine();
                    if (answ.ToLower() == "y")
                    {
                        foreach (string ModExtra in leftModded)
                        {
                            string AbsoluteFilename = ModExtra.Replace(moddedDirectory, String.Empty);
                            finalFile.AddRange(Encoding.UTF8.GetBytes(AbsoluteFilename)); finalFile.Add(0x00);
                            byte[] diff = GenerateDiff(new MemoryStream(new byte[1]{0x00}), new FileStream(ModExtra, FileMode.Open, FileAccess.Read));
                            finalFile.AddRange(BitConverter.GetBytes(diff.Length));
                            finalFile.AddRange(diff);
                            Console.WriteLine("Processed " + AbsoluteFilename);
                        }
                    }

                }
                Console.WriteLine("Mod created! Saving to newmod.hsmod");
                File.WriteAllBytes("newmod.hsmod", finalFile.ToArray());
                Console.WriteLine("In order to publish your mod to FORGERY database, please, create a submission on https://hsmod.cf/");
            }
            else
            {
                Console.WriteLine("Usage modmaker.exe Modded_Directory Original_Directory");
            }
        }

        public static bool Compare(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
            {
                return true;
            }
            if ((a1 != null) && (a2 != null))
            {
                if (a1.Length != a2.Length)
                {
                    return false;
                }
                for (int i = 0; i < a1.Length; i++)
                {
                    if (a1[i] != a2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public static bool ChecksumCollide(string filename1, string filename2)
        {
            byte[] ChecksumFirst;
            byte[] ChecksumSecond;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename1))
                {
                    ChecksumFirst = md5.ComputeHash(stream);
                }
                using (var stream = File.OpenRead(filename2))
                {
                    ChecksumSecond = md5.ComputeHash(stream);
                }
            }
            return Compare(ChecksumFirst, ChecksumSecond);
        }
        public static bool ChecksumCollide(byte[] byteArray1, byte[] byteArray2)
        {
            byte[] ChecksumFirst;
            byte[] ChecksumSecond;
            using (var md5 = MD5.Create())
            {
                using (var stream = new MemoryStream(byteArray1))
                {
                    ChecksumFirst = md5.ComputeHash(stream);
                }
                using (var stream = new MemoryStream(byteArray2))
                {
                    ChecksumSecond = md5.ComputeHash(stream);
                }
            }
            return Compare(ChecksumFirst, ChecksumSecond);
        }

        private static byte[] GenerateDiff(string filename, string moddedFilename)
        {
            var fsOriginal = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var fsModded = new FileStream(moddedFilename, FileMode.Open, FileAccess.Read);
            return GenerateDiff(fsOriginal, fsModded);
        }

        private static byte[] GenerateDiff(Stream original, Stream modded)
        {
            long ActualPosition = 0;
            List<byte[]> blocks = new List<byte[]>();
            //4 first bytes is the offset of the block, 2 bytes is the length of block, and other 1024 bytes are the contains
            while (true)
            {
                byte[] ModdedBuffer = new byte[1024];
                byte[] OriginalBuffer = new byte[1024];

                int a = modded.Read(ModdedBuffer, 0, ModdedBuffer.Length);
                int b = original.Read(OriginalBuffer, 0, OriginalBuffer.Length);

                if (a == 0 && b == 0)
                {
                    List<byte> diff = new List<byte>();
                    foreach (byte[] block in blocks)
                    {
                        diff.AddRange(block);
                    }
                    return diff.ToArray();
                }

                bool equal = ChecksumCollide(OriginalBuffer, ModdedBuffer);
                if (!equal)
                {
                    List<byte> Block = new List<byte>();
                    byte[] LocationBA = BitConverter.GetBytes((int)ActualPosition);
                    byte[] Size = BitConverter.GetBytes((short)a);
                    Block.AddRange(LocationBA);
                    Block.AddRange(Size);
                    Block.AddRange(ModdedBuffer);
                    blocks.Add(Block.ToArray());
                }
                ActualPosition = modded.Position;
            }
        }

        private static string FindModded(string[] moddedFiles, string moddedDirectory, string originalDirectory, string filename)
        {
            string absolute = filename.Replace(originalDirectory, String.Empty);
            for (int i = 0; i<moddedFiles.Length; i++)
            {
                string absoluteModded = moddedFiles[i].Replace(moddedDirectory, String.Empty);
                if (absolute == absoluteModded) return moddedFiles[i];
            }
            throw new IOException("Modded files lack part of original files. Please, check an integrity of files");
        }
    }
    class FilePair
    {
        public string ModdedFileName, OriginalFileName;
    }
}
