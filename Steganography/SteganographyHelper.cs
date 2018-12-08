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
            // 문자열과 비트맵파일을 인자로 받고 비트맵파일을 반환한다
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
                    // 비트맵 이미지의 픽셀 단위를 읽어들인다
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    // R, G, B는 각각 픽셀의 RGB 값에서 LSB 값을 0으로 세팅한 값을 저장한다
                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)
                        {
                            // 하나의 문자를 숨기는 데에 8개의 비트를 사용하므로 8개의 비트를 사용하면 처리를 해준다
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                /*
                                  컴퓨터의 아스키코드값은 0~255이기 때문에 이를 표현하기 위해 8비트가 필요하다
                                  하나의 픽셀은 3비트를 표현한다(RGB 각각의 LSB)
                                  따라서 픽셀과 문자가 나누어 떨어지지 않는 경우가 발생한다(픽셀 세개 : 9비트, 문자 하나 : 8비트 )
                                  그 경우를 처리해주기 위해 zeros를 이용한다
                                */
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    // 마지막에 하나의 픽셀 값을 처리하지 못한 경우가 발생했을 때 처리해준다
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length)
                            {
                                // 숨기고자 하는 텍스트의 길이를 넘어가면 상태를 바꾼다
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                // 그렇지 않으면 charIndex를 증가하여 다음 문자를 저장한다
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {
                            // 픽셀의 RGB값을 바꾸어(0으로 세팅된 LSB를 1또는 0으로 다시 세팅) 저장한다 
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
                            //Filling_With_Zeros 상태이면 zeros를 하나씩 더해준다 
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
                    // 픽셀 하나하나를 읽어들인다
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            // 하나의 픽셀에 RGB 값을 읽어들이고 읽어들인 순서대로 이진법으로 표현한다
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
                            // 문자 하나 당 8개의 비트를 사용하므로 8개가 되면 조건문을 실행한다 
                            charValue = reverseBits(charValue);
                            // 저장할 때 2로 나눈 나머지를 픽셀에 저장하는 방식으로 하였기 때문에 이진법은 읽어들인 순서의 역으로 읽혀야 한다
                            if (charValue == 0)
                            {
                                // 저장된 문자가 없는 경우 함수를 끝낸다
                                return extractedText;
                            }
                            char c = (char)charValue;
                            
                            extractedText += c.ToString();
                            // 역으로 읽은 수를 문자로 변환하여 문자열 형태로 저장한다
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
