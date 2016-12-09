using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State   //State를 enum을 이용해 선언하고 있습니다.
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp) // 숨기기 위한 메소드
        {
            State state = State.Hiding; // Hiding의 상태로 초기화합니다.

            int charIndex = 0;
            int charValue = 0;
            long pixelElementIndex = 0;
            int zeros = 0;
            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);   // pixel값을 받아옵니다.

                    R = pixel.R - pixel.R % 2;  // 마지막 자리수를 뺍니다. 픽셀의 RGB값을 조정하고 있습니다.
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0) // 8의배수 index에 왔을때 ( 0-base 에서 )  
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)   // 이전에서 문자가 없이 0으로 채워오고 오고있었으면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)    // 새 픽셀을 시작하는 부분이 아니면 (24번째 혹은 48번째)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // RGB를 그대로 두고 return 한다.
                                }
                                return bmp;
                            }

                            if (charIndex >= text.Length)   // 넣을 char가 넣을 text의 길이를 벗어난다면 Filling with zeros 해야합니다.
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else    // 아니라면 다음 char로 넘어 갑니다.
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)  
                        {
                            case 0:                 // 픽셀의 원소 인덱스가 0이면 R에 정보를 숨깁니다. 
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 1:                 // 픽셀의 원소 인덱스가 1이면 G에 정보를 숨깁니다. 
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2:                 // 픽셀의 원소 인덱스가 2이면 B에 정보를 숨깁니다. 
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    // 숨긴 데이터가 들어가있는 RGB 값으로 SetPixel합니다.
                                } break;
                        }

                        pixelElementIndex++;    // 다음 픽셀 원소 값으로 넘어갑니다. R G B 순으로 3회 진행됩니다.

                        if (state == State.Filling_With_Zeros)  // 더이상 넣을 char가 없을시 zeros 를 증가시킵니다.
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp) //추출하기 위한 메소드
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)    // 두 개의 for문을 통해 좌상단부터 시작합니다.
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀을 가져와서
                    for (int n = 0; n < 3; n++) 
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:                 // 픽셀의 원소 인덱스가 0이면 R에서 charValue의 값을 복원합니다.
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1:                 // 픽셀의 원소 인덱스가 1이면 G에서 charValue의 값을 복원합니다.
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2:                 // 픽셀의 원소 인덱스가 2이면 B에서 charValue의 값을 복원합니다.
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)    // embedText시 거꾸로 저장해나갔으므로 reverseBits를 통해 bit를 역순으로 바꿔줘야합니다.
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

        public static int reverseBits(int n)    // n의 bit를 역순의 bits로 바꿔주는 메소드입니다. 
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
