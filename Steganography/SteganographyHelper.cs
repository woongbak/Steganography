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

        public static Bitmap embedText(string text, Bitmap bmp) // 인자로 입력받은 txt와 open한 이미지를 받는다.
        {
            State state = State.Hiding; // 숨기는 상태이므로 state에 설정해준다.

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);   // bmp의 넓이만큼 픽셀 하나하나 색깔을 받는다.

                    R = pixel.R - pixel.R % 2;          // RGB 색깔들을 본래 값에서 2를 나눈 나머지 값을 빼준다.
                    G = pixel.G - pixel.G % 2;          // 각 색깔의 LSB를 초기화해줌
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)     // RGB 세가지에 대해 수행해야하므로 3
                    {
                        if (pixelElementIndex % 8 == 0) // 한 문자 숨기기 끝나면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)    // 숨길 문자가 더 이상 없을 때
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)    // 숨길 것이 남아있으면 마저 세팅해준다.
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp; // 위와 같은 경우가 아니면 bmp를 return해준다.
                            }

                            if (charIndex >= text.Length)   // 다 숨겼으면 state를 설정해준다.
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else // 숨길 문자가 더 있을 시
                            {
                                charValue = text[charIndex++];  // 다음 문자에 대해 수행
                            }
                        }

                        switch (pixelElementIndex % 3)  // R,G,B의 경우의 수
                        {
                            case 0: // 나머지가 0일 때, R
                                {
                                    if (state == State.Hiding)  // Hiding 상태일시
                                    {
                                        R += charValue % 2; // R의 LSB charValue를 2로 나눈 나머지를 더해준다.
                                        charValue /= 2; // 다음 문자로..?
                                    }
                                } break;
                            case 1: // 나머지가 1일 때, G
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; // 위와 동일

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: // 나머지가 2일 때, B
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2; // 위와 동일

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    // j, i 좌표의 픽셀에 RGB 픽셀 데이터를 설정해준다.
                                } break;
                        }

                        pixelElementIndex++;    // pixelElementIndex를 1 더해준다.

                        if (state == State.Filling_With_Zeros) // filing_with_zeros 상태일시, zeros를 1 더해준다.
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp; // bmp를 반환해준다.
        }

        public static string extractText(Bitmap bmp)    // 추출
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)    // 숨기는 것과 동일하게 모든 픽셀 탐색
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);   // 픽셀 값을 가져온다.
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) // RGB 세 가지의 픽셀 값을 위의 숨기는 것과 반대의 연산을 통해 LSB를 복구한다.
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

                        if (colorUnitIndex % 8 == 0)    // 추출 연산이 끝난 후,
                        {
                            charValue = reverseBits(charValue); // 픽셀 비트값이 뒤집어져있으므로 reverse 함수를 통해 원상복귀

                            if (charValue == 0) // 더 이상 추출할 것이 없으면
                            {
                                return extractedText;   // 추출한 값 반환
                            }
                            char c = (char)charValue;   // 추출한 값을 문자로 바꿔줌

                            extractedText += c.ToString();  // 추출된 문자를 차곡차곡 문자열에 추가해줌
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
