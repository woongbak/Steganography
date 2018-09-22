using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);

                    R = pixel.R - pixel.R % 0b10000;
                    G = pixel.G - pixel.G % 0b10000;
                    B = pixel.B - pixel.B % 0b10000;

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 2 == 0)
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length)
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 0b10000;
                                        charValue /= 0b10000;
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 0b10000;

                                        charValue /= 0b10000;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 0b10000;

                                        charValue /= 0b10000;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 0b10000 + pixel.R % 0b10000;
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 0b10000 + pixel.G % 0b10000;
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 0b10000 + pixel.B % 0b10000;
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 2 == 0)
                        {
                            // charValue = reverseBits(charValue);
                            charValue = reverseBits4(charValue);

                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString();

                            charValue = 0;
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }
        public static int reverseBits4(int n)
        {
            int result = 0;

            for (int i = 0; i < 2; i++)
            {
                result = result * 0b10000 + n % 0b10000;

                n /= 0b10000;
            }

            return result;
        }
    }
}
