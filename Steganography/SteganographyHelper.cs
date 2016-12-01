using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding, // 메세지를 숨기는 상태
            Filling_With_Zeros // 메세지를 다 숨기고 0으로 채우는 상태
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++) // 이중 FOR문으로 이미지의 모든 픽셀에 대한 참조
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀 정보 저장

                    R = pixel.R - pixel.R % 2; //해당 픽셀의 R,G,B 각각 최하위 비트 0으로 세팅
                    G = pixel.G - pixel.G % 2; //모든 픽셀의 최하위비트는 0으로 세팅됨.
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) // R,G,B에 각각 비트를 저장하기 위해 3번 실행되는 반복문
                    {
                        if (pixelElementIndex % 8 == 0) // 최대 8비트로 표현할 수 있는 값을 숨기도록 설계
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 숨길 문자열을 다 저장하고, 추가로 0을 8번 저장 다했을 경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 마지막으로 비트 세팅
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length) // 문자열을 다 숨겼을 경우 state 값 변경.
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else // 숨길 문자열이 남은 경우
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3) // R,G,B에 차례대로 1비트씩 저장, charValue값을 2로 나눈 나머지 0 또는 1을 저장
                        {                              // 한개의 charValue값당 8번 나누므로, 값을 표현할 때 그 이상의 비트 값이 필요한 문자는
                            case 0:                    // 저장이 불가능 ex) 알파벳 저장가능 but 한글은 저장 불가능
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

            for (int i = 0; i < bmp.Height; i++) //이중 FOR문을 이용한 모든 픽셀에 대한 참조
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) // R,G,B 차례대로 참조
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // 값을 2로 나눈 나머지를 저장했으므로 
                                } break;                                     // 본래 값을 복구하기 위한 계산식
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

                        if (colorUnitIndex % 8 == 0) // 한 문자에 복구가 끝난경우
                        {
                            charValue = reverseBits(charValue); // 복구할 경우 비트의 순서가 반대로 되어있기때문에
                                                                // 순서를 맞춰주기 위해 reverseBits 함수 호출
                            if (charValue == 0)
                            {
                                return extractedText; // 값의 복구가 끝이난 경우 리턴
                            }
                            char c = (char)charValue; // int형 타입으로 저장된 문자를 다시 char형으로 형변환

                            extractedText += c.ToString(); // extractedText에 추출한 문자 저장
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n) // 순서가 뒤집힌 비트들의 순서를 다시 뒤집어 주기 위한 함수
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
