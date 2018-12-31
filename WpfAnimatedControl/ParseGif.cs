using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfAnimatedControl
{
    public class ParseGif
    {
        List<int> Delays = new List<int>();

        public List<int> ParseGifDataStream(byte[] gifData, int offset)
        {
            Delays.Clear();
            offset = ParseHeader(ref gifData, offset);
            offset = ParseLogicalScreen(ref gifData, offset);
            while (offset != -1)
            {
                offset = ParseBlock(ref gifData, offset);
            }
            return Delays;
        }

        private int ParseHeader(ref byte[] gifData, int offset)
        {
            string str = System.Text.ASCIIEncoding.UTF8.GetString(gifData, offset, 3);
            if (str != "GIF")
            {
                throw new FormatException("Not a proper GIF file: missing GIF header");
            }
            return 6;
        }

        private int ParseLogicalScreen(ref byte[] gifData, int offset)
        {
            int _logicalWidth = BitConverter.ToUInt16(gifData, offset);
            int _logicalHeight = BitConverter.ToUInt16(gifData, offset + 2);

            byte packedField = gifData[offset + 4];
            bool hasGlobalColorTable = (int)(packedField & 0x80) > 0 ? true : false;

            int currentIndex = offset + 7;
            if (hasGlobalColorTable)
            {
                int colorTableLength = packedField & 0x07;
                colorTableLength = (int)Math.Pow(2, colorTableLength + 1) * 3;
                currentIndex = currentIndex + colorTableLength;
            }
            return currentIndex;
        }

        private int ParseBlock(ref byte[] gifData, int offset)
        {
            switch (gifData[offset])
            {
                case 0x21:
                    if (gifData[offset + 1] == 0xF9)
                    {
                        return ParseGraphicControlExtension(ref gifData, offset);
                    }
                    else
                    {
                        return ParseExtensionBlock(ref gifData, offset);
                    }
                case 0x2C:
                    offset = ParseGraphicBlock(ref gifData, offset);
                    return offset;
                case 0x3B:
                    return -1;
                default:
                    throw new FormatException("GIF format incorrect: missing graphic block or special-purpose block. ");
            }
        }

        private int ParseGraphicControlExtension(ref byte[] gifData, int offset)
        {
            int returnOffset = offset;
            // Extension Block
            int length = gifData[offset + 2];
            returnOffset = offset + length + 2 + 1;

            byte packedField = gifData[offset + 3];

            // Get DelayTime
            int delay = BitConverter.ToUInt16(gifData, offset + 4);
            int delayTime = (delay < 10) ? 10 : delay;
            Delays.Add(delayTime);
            while (gifData[returnOffset] != 0x00)
            {
                returnOffset = returnOffset + gifData[returnOffset] + 1;
            }

            returnOffset++;

            return returnOffset;
        }

        private int ParseExtensionBlock(ref byte[] gifData, int offset)
        {
            int returnOffset = offset;
            // Extension Block
            int length = gifData[offset + 2];
            returnOffset = offset + length + 2 + 1;
            // check if netscape continousLoop extension
            if (gifData[offset + 1] == 0xFF && length > 10)
            {
                string netscape = System.Text.ASCIIEncoding.UTF8.GetString(gifData, offset + 3, 8);
                if (netscape == "NETSCAPE")
                {
                    int _numberOfLoops = BitConverter.ToUInt16(gifData, offset + 16);
                    if (_numberOfLoops > 0)
                    {
                        _numberOfLoops++;
                    }
                }
            }
            while (gifData[returnOffset] != 0x00)
            {
                returnOffset = returnOffset + gifData[returnOffset] + 1;
            }

            returnOffset++;

            return returnOffset;
        }

        private int ParseGraphicBlock(ref byte[] gifData, int offset)
        {
            byte packedField = gifData[offset + 9];
            bool hasLocalColorTable = (int)(packedField & 0x80) > 0 ? true : false;

            int currentIndex = offset + 9;
            if (hasLocalColorTable)
            {
                int colorTableLength = packedField & 0x07;
                colorTableLength = (int)Math.Pow(2, colorTableLength + 1) * 3;
                currentIndex = currentIndex + colorTableLength;
            }
            currentIndex++; // Skip 0x00

            currentIndex++; // Skip LZW Minimum Code Size;

            while (gifData[currentIndex] != 0x00)
            {
                int length = gifData[currentIndex];
                currentIndex = currentIndex + gifData[currentIndex];
                currentIndex++; // Skip initial size byte
            }
            currentIndex = currentIndex + 1;
            return currentIndex;
        }
    }
}
