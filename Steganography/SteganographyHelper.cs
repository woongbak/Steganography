using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding, // 파일 숨김 모드
            Filling_With_Zeros // 비트 마지막 부분을 0으로 채운것
        };

        public static Bitmap embedText(string text, Bitmap bmp)  // 텍스트 
        {
            State state = State.Hiding;  // 숨김 모드로 설정

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)  // bmp 파일의 사이즈 만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀 하나하나 마다의 작업

                    R = pixel.R - pixel.R % 2;           // RGB 값의 끝 부분을 0으로 만들기 위해 2로 나눈 나머지 값을 빼줍니다
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // zeros 가 8일 때 암호화된 bmp파일을 반환한다.
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length)  // 암호화할 텍스트의 길이와 현재 인덱스를 비교해서 인덱스가 더 클때 상태변경
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else                           // 반대의 경우 인덱스보다 1 큰값을 charValue에 저장
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)    // 이미지 픽셀의 인덱스 값을 3으로 나눈후, 그 값에 대한 3가지 경우의 switch문
                        {                                 // 각각의 값의 LSB에 charValue의 값에 2로 나눈 나머지를 더해준다
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

        public static string extractText(Bitmap bmp)  // 암호문 추출
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)      // bmp 파일의 크기만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)  // colorUnitIndex 값을 3으로 나눈 나머지에 대한 값의 3가지 switch 구문
                        {                            // 각각의 경우에 charValue 값에 2를 곱하고 각 픽셀에 2를 나눈 나머지를 더해준다.
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

                        colorUnitIndex++;  // 수정후 유닛 인덱스값 + 1

                        if (colorUnitIndex % 8 == 0) // 한 바이트 채운 후 저장
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

            return extractedText;  // 추출된 메시지 
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
