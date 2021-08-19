using System;
using System.Linq;

namespace leacher.Services
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;

    public class ChunkProcessor
    {
        private const int Width = 800;
        private const int Height = 400;

        private const byte Magic = 0xFE;

        private readonly Bitmap storeBitmap;

        public ChunkProcessor()
        {
            storeBitmap = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
            ColorPalette ncp = storeBitmap.Palette;
            for (int i = 0; i < 256; i++)
                ncp.Entries[i] = Color.FromArgb(255, i, i, i);
            storeBitmap.Palette = ncp;
        }

        public void ChunkFile(string fileName, string outputFolder)
        {
            int fullSize = Width * Height;
            int maxSize = fullSize - 4;
            
            byte[] chunk = new byte[maxSize];
            byte[] fullChunk = new byte[fullSize];

            int chunkIndex = 0;

            using var sr = File.OpenRead(fileName);
            int readBytes;
            do
            {
                readBytes = sr.Read(chunk, 0, maxSize);
                if (readBytes > 0)
                {
                    byte b0 = (byte)readBytes,
                         b1 = (byte)(readBytes >> 8);
                    fullChunk[0] = Magic;
                    fullChunk[1] = b0;
                    fullChunk[2] = b1;
                    Console.WriteLine($"First three are {Magic}, {b0}, {b1}");
                    Array.Copy(chunk, 0, fullChunk, 3,  readBytes);
                    Array.Clear(fullChunk, readBytes + 3, fullSize - readBytes - 3 );
                    int checkSum = chunk.Take(readBytes).Select(Convert.ToInt32).Sum() % 256;
                    fullChunk[Width * Height - 1] = (byte)checkSum;
                    this.StoreFile(outputFolder, chunkIndex, fullChunk);
                    chunkIndex++;
                }

                    
            }
            while (readBytes > 0);
        }

        private void StoreFile(string outputFolder, int index, byte[] contents)
        {
            if (!Directory.Exists(outputFolder))
            {
                throw new ArgumentException($"Folder {outputFolder} does not exist", nameof(outputFolder));
            }

            var boundRect = new Rectangle(0, 0, Width, Height);

            BitmapData bmpData = storeBitmap.LockBits(boundRect,
                ImageLockMode.WriteOnly,
                storeBitmap.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int bytes = bmpData.Stride * storeBitmap.Height;
            Marshal.Copy(contents, 0, ptr, bytes);
            storeBitmap.UnlockBits(bmpData);

            string imageFileName = Path.Combine(outputFolder, $"{index}.png");
            this.storeBitmap.Save(imageFileName, ImageFormat.Png);
        }
    }
}
