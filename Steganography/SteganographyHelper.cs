using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding, //이친구는 0이다
            Filling_With_Zeros //이친구는 1이다
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding; //state에 0을 저장한다

            int charIndex = 0; //변수1

            int charValue = 0; //변수2

            long pixelElementIndex = 0; // 변수3

            int zeros = 0; //변수4

            int R = 0, G = 0, B = 0; //변수 5,6,7

            for (int i = 0; i < bmp.Height; i++) //bmp높이 1씩 올리기
            {
                for (int j = 0; j < bmp.Width; j++) //bmp 옆으로 1픽셀씩 옮김
                {
                    Color pixel = bmp.GetPixel(j, i); //그 비트맵에 저장된 색 가져오기

                    R = pixel.R - pixel.R % 2; //그 픽셀의 R값에서 그 R을 2로 나눈 나머지로 뺀다
                    G = pixel.G - pixel.G % 2; //그 픽셀의 G값에서 그 G를 2로 나눈 나머지로 뺀다
                    B = pixel.B - pixel.B % 2; //그 픽셀의 B값에서 그 B를 2로 나눈 나머지로 뺀다

                    for (int n = 0; n < 3; n++) //총 3번돌린다
                    {
                        if (pixelElementIndex % 8 == 0) //이 변수가 8로 나눠 0일때
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //FWZ가 0이고 zeros가 8일때
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) //PEI에서 1을 뺀것에 3으로 나눈 나머지가 2보다 작으면
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //(x,y)픽셀에 위에 RGB값으로 색칠한다
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
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
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
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2:
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
