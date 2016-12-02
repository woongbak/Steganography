using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State   //Hiding은 데이터 숨김이 끝나지 않음을 의미, Filling_With_Zeros는 끝을 알리는 의미로 0으로 set.
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp)  //이미지(bmp) 안에 데이터(text)를 숨기는 역할 수행.
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;   // 8의 배수가 되면 ASCII 1문자를 숨긴 것을 의미.

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)   
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;        // LSB 비트를 0으로 셋팅
                  
                    for (int n = 0; n < 3; n++)   // 숨길 문자 중 앞에서 부터 한 문자씩 3bit를 숨기는 역할 수행.
                    {
                        if (pixelElementIndex % 8 == 0)  // 한개의 문자를 1bit 씩 나누어 8번 수행했을 경우(즉, ASCII 1문자 숨김 완료일 경우), 다음으로 숨길 문자를 가져옴.
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)  //0으로 변경된 R, G, B 값을 bmp 객체에 적용 안되고 끝날 경우, 변경된 픽셀값을 적용함.
                                { 
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length)   // 숨길 문자열이 없는 경우, 마지막으로 8bit를 0으로 채운다.
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else                           // 아직 숨길 데이터가 남은 경우, 숨길 문자 1개를 더 가져옴.
                            {
                                charValue = text[charIndex++]; 
                            }
                        }

                        switch (pixelElementIndex % 3)    // 하나의 픽셀 당 3bit를 숨김. 각각 R, G, B의 LSB 비트에 1bit를 숨김.
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

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));   // 수정한 R,G,B 값을 반영함. 기존 픽셀에서 R, G, B의 LSB만 수정됨.
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

        public static string extractText(Bitmap bmp)   //데이터를 숨긴 경우 복호화.
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);    //하나의 픽셀을 가져온다.
                    for (int n = 0; n < 3; n++)          // 픽셀에서 R, G, B의 LSB비트를 가져와서 하나의 ASCII문자열을 만들어 나감.
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

                        if (colorUnitIndex % 8 == 0)  // 하나의 ASCII 문자열이 만들어진 경우 
                        {
                            charValue = reverseBits(charValue);  

                            if (charValue == 0)      // 문자열이 끝났음을 인식하고 결과값 반환.
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString();    // 만들어진 문자열을 더해나감, 최종적으로 이 값 반환.
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)   //embedText() 과정에서 반대로 집어넣었음으로 bit열을 reverse 시킨다.
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
