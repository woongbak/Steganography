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

            int R = 0, G = 0, B = 0;    // R, G, B 를 0으로 설정한다.

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀의 RGB값을 저장한다.

                    R = pixel.R - pixel.R % 2;  // R값을 R- R%2 로 변경
                    G = pixel.G - pixel.G % 2;  // G값을 G- G%2 로 변경
                    B = pixel.B - pixel.B % 2;  // B값을 B- B%2 로 변경

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0) // 만약 pixelElementIndex가 8의 배수이면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) 
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 위 조건들을 만족하면 pixel값을 조정한다
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

                        switch (pixelElementIndex % 3) // pixelElementIndex 를 3으로 나눈 나머지에 따라서
                        {
                            case 0: // 0일경우 R의 값을 조정한다
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 1: // 1일경우 G의 값을 조정한다.
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: // 2일경우 B의 값을 조정한다.
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 픽셀값 설정을 바꾼다
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
                    Color pixel = bmp.GetPixel(j, i); // 픽셀값을 저장한다.
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) // colorUnitIndex를 3에 나눈값에 따라서
                        {
                            case 0: // 0일경우, charValue값을 pixel.R값에 따라 변경한다.
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1: // 1일경우, charValue값을 pixel.G값에 따라 변경한다.
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2: // 2일경우, charValue값을 pixel.B값에 따라 변경한다.
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue);

                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString();
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
    }
}
