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

        public static Bitmap embedText(string text, Bitmap bmp) // text: 숨길 문자열     bmp: 문자열을 숨길 이미지
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)    // 전체 픽셀을 순회한다
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);   // 각 픽셀의 데이터를 추출한다

                    R = pixel.R - pixel.R % 2;  // 2로 나눈 나머지 값을 빼 픽셀의 RGB에 대해 LSB를 0으로 만든다
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) // R, G, B 전체에 텍스트를 숨길 수 있도록 3번 반복한다
                    {
                        if (pixelElementIndex % 8 == 0) // char는 8비트 정수형이다. 한 문자의 비트를 모두 RGB에 세팅하였을 경우
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)    // 텍스트를 다 채운 후 8비트를 더 이동해 문자열의 크기보다 1만큼 더 이동했다면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    // RGB 값을 0으로 세팅
                                }

                                return bmp; // bmp를 리턴한다
                            }

                            if (charIndex >= text.Length)   // 텍스르를 모두 저장하면 state를 Filling_With_Zeros로 변경한다
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else    // 텍스트가 아직 남아있는 경우, 해당 index의 text 값을 charValue에 저장한다.
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {
                            case 0: // 문자의 마지막 비트를 R 비트에 저장하고 문자의 마지막 비트를 삭제한다
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 1: // 문자의 마지막 비트를 G 비트에 저장하고 문자의 마지막 비트를 삭제한다
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: // 문자의 마지막 비트를 B 비트에 저장하고 문자의 마지막 비트를 삭제한다
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    // RGB를 다 채웠으므로 픽셀에 문자를 숨긴 RGB 값을 세팅한다
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)  // state가 Filling_With_Zeros일 경우 zeros를 증가한다
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp; // 순회가 끝나면 bmp 리턴한다
        }

        public static string extractText(Bitmap bmp)    // bmp: 텍스트를 추출할 이미지
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++) // 전체 픽셀을 순회한다
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);   // 각 픽셀의 데이터를 추출한다
                    
                    for (int n = 0; n < 3; n++) // R, G, B 전체를 확인하기 위해
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0: // charValue를 한 비트 이동시키고 뒤에 R비트를 붙인다
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1: // charValue를 한 비트 이동시키고 뒤에 G비트를 붙인다
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2: // charValue를 한 비트 이동시키고 뒤에 B비트를 붙인다
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)    // char 변수형을 구성하는 8비트를 완성한 경우
                        {
                            charValue = reverseBits(charValue); // 추출한 문자의 비트를 리버스한다

                            if (charValue == 0) // 추출한 문자의 값이 0이면
                            {
                                return extractedText;   // extractedText을 리턴한다
                            }
                            char c = (char)charValue;   // int형에서 char형으로 변환한다

                            extractedText += c.ToString();  // extractedText에 추출한 문자를 이어 붙인다
                        }
                    }
                }
            }

            return extractedText;   // 순회가 끝나면 extractedText을 리턴한다
        }

        public static int reverseBits(int n)    // n: 리버스 할 int형 변수
        {
            int result = 0;

            for (int i = 0; i < 8; i++) // 8비트 데이터를 리버스한다
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }
    }
}
