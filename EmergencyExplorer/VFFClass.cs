using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergencyExplorer
{
    public static class VFFClass
    {
        public static byte[] VFFData;
        public static Bitmap VFFPreview; 

       public static void LoadVFF(string filePath, string palPath, string savePath, bool export)
       {
            string fileName = Path.GetFileName(filePath);
            string exPath = Path.Combine(savePath, fileName);

            byte[] pal = File.ReadAllBytes(palPath);
            byte[] data = File.ReadAllBytes(filePath);
            MemoryStream m = new MemoryStream(data);
            byte type = (byte)m.ReadByte();
            ushort count = ReadU16(m);
            switch (type)
            {
              case 2:
               int dataPointer = 4 * count + 7;
               ushort width = ReadU16(m);
               ushort height = ReadU16(m);
               for (int i = 0; i < count; i++)
               {
                        m.Seek(7 + i * 4, 0);
                        int size = (int)ReadU32(m);
                        m.Seek(dataPointer, 0);
                        byte[] imageData = new byte[size];
                        m.Read(imageData, 0, size);
                        VFFData = Decompress(imageData, width * height);
                        VFFPreview = MakeBitmap(width, height, VFFData, pal);
                        if (export == true)
                        {
                            File.WriteAllBytes(exPath + "_" + i + ".raw", VFFData);
                            Bitmap bmp = MakeBitmap(width, height, VFFData, pal);
                            bmp.Save(exPath + "_" + i + ".bmp");
                        }
                        dataPointer += size;
               }
               break;
              case 3:
              {
                ushort count3 = count;
                ushort[] widths = new ushort[count3];
                ushort[] heights = new ushort[count3];
                uint[] sizes = new uint[count3];

                for (int i = 0; i < count3; i++)
                {
                            widths[i] = ReadU16(m);
                            heights[i] = ReadU16(m);
                            sizes[i] = ReadU32(m);
                }

                for (int i = 0; i < count3; i++)
                {
                            byte[] comp = new byte[sizes[i]];
                            m.Read(comp, 0, (int)sizes[i]);
                            VFFData = Decompress(comp, widths[i] * heights[i]);
                            VFFPreview = MakeBitmap(widths[0], heights[0], VFFData, pal);
                            if (export == true)
                            {
                                File.WriteAllBytes(exPath + "_frame" + i + ".raw", VFFData);
                                Bitmap bmp = MakeBitmap(widths[i], heights[i], VFFData, pal);
                                bmp.Save(exPath + "_frame" + i + ".bmp");
                            }
                }
              }
              break;
            }

       }

        public static void DumpPalette(string palPath, string savePath)
        {
            string fileName = Path.GetFileName(palPath);
            string exPath = Path.Combine(savePath, fileName);

            byte[] pal = File.ReadAllBytes(palPath);
            Bitmap bmp = new Bitmap(16, 16);
            int pos = 0;
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    bmp.SetPixel(x, y, Color.FromArgb(255, pal[pos] * 4, pal[pos + 1] * 4, pal[pos + 2] * 4));
                    pos += 3;
                }
            bmp.Save(exPath + "_Palette.bmp");
        }

        public static Bitmap MakeBitmap(int width, int height, byte[] data, byte[] pal)
        {
            Bitmap result = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    byte val = data[y * width + x];
                    byte r = (byte)(pal[val * 3] * 4);
                    byte g = (byte)(pal[val * 3 + 1] * 4);
                    byte b = (byte)(pal[val * 3 + 2] * 4);
                    result.SetPixel(x, y, Color.FromArgb(255, r, g, b));
                }
            return result;
        }

        static ushort ReadU16(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            return BitConverter.ToUInt16(buff, 0);
        }

        static uint ReadU32(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToUInt32(buff, 0);
        }

        static byte[] Decompress(byte[] data, int sizeOut)
        {
            int posIn = 0, posOut = 0;
            byte currentByteValue;
            byte[] result = new byte[sizeOut];
            while ((currentByteValue = data[posIn]) != 0)
            {
                if (currentByteValue >= 0x80u)
                {
                    int nextBytePtr = posIn + 1;
                    int n = 127;
                    if (currentByteValue > 127)
                    {
                        byte nextByteValue = data[nextBytePtr];
                        do
                        {
                            result[posOut++] = nextByteValue;
                            ++n;
                        }
                        while (currentByteValue > n);
                    }
                    posIn = nextBytePtr + 1;
                }
                else
                {
                    posIn++;
                    for (int i = 0; currentByteValue > i; i++)
                        result[posOut++] = data[posIn++];
                }
            }
            return result;
        }

    }
}
