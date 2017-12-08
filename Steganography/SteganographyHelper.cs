using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State // 두가지 상태, 숨김 상태, 0으로 채울 상태
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp) // 숨김
        {
            State state = State.Hiding; // 숨김 상태로 설정

            int charIndex = 0; // 캐릭터 인덱스

            int charValue = 0; // 캐릭터 값

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++) // 높이만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++) // 너비만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀을 가져옴

                    // 짝수로 만들어서 R,G,B에 넣음
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) // R, G, B 
                    {
                        if (pixelElementIndex % 8 == 0) // char 형은 1바이트 == 8비트
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 상태가 0으로 채우는 상태이고 zeros가 8이면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 픽셀 변경
                                }

                                return bmp; // 반환
                            }

                            if (charIndex >= text.Length) // 숨길 텍스트의 길이보다 인덱스가 크거나 같을 경우
                            {
                                state = State.Filling_With_Zeros; // 상태 변경
                            }
                            else
                            {
                                charValue = text[charIndex++]; // 텍스트의 인덱스 번째의 글자를 대입 (글자를 불러옴)
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {
                            case 0: // 3으로 나눈 나머지가 0이면
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2; // R의 최하위 비트에 넣어줌
                                        charValue /= 2; // 자릿수 변경
                                    }
                                } break;
                            case 1: // 3으로 나눈 나머지가 1이면
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; // G의 최하위 비트에 넣어줌

                                        charValue /= 2; // 자릿수 변경
                                    }
                                } break;
                            case 2: // 3으로 나눈 나머지가 2이면
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2; // B의 최하위 비트에다가 넣어줌

                                        charValue /= 2; // 자릿수 변경
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 바꾼것으로 덮어씌움
                                } break;
                        }

                        pixelElementIndex++; // 인덱스 증가

                        if (state == State.Filling_With_Zeros) // 0을 채울 상태 이면
                        {
                            zeros++; // zeros 증가
                        }
                    }
                }
            }

            return bmp; // 반환
        }

        public static string extractText(Bitmap bmp) // 추출
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; // 초기화

            for (int i = 0; i < bmp.Height; i++) // 높이만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++) // 너비만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀을 가져옴
                    for (int n = 0; n < 3; n++) // R, G, B 
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // R을 짝수화 시켜서 숨겨놓은 것을 복구 (2를 곱해서 자릿수를 바꿔주고 더해줌)
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // G을 짝수화 시켜서 숨겨놓은 것을 복구 (2를 곱해서 자릿수를 바꿔주고 더해줌)
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // B을 짝수화 시켜서 숨겨놓은 것을 복구 (2를 곱해서 자릿수를 바꿔주고 더해줌)
                                } break;
                        }

                        colorUnitIndex++; // 인덱스 증가

                        if (colorUnitIndex % 8 == 0) // 8비트를 다 복구 == 1글자 읽어냄
                        {
                            charValue = reverseBits(charValue); // 비트를 역순으로 바꿈

                            if (charValue == 0) // 0글자일 경우
                            {
                                return extractedText; // 그냥 반환
                            }
                            char c = (char)charValue; // c에 캐릭터형으로 변환해서 대입

                            extractedText += c.ToString(); // c를 넣어줌
                        }
                    }
                }
            }

            return extractedText; // 반환
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
