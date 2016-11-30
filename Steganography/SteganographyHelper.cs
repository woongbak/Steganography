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

        public static Bitmap embedText(string text, Bitmap bmp) // 이미지를 숨기기 위한 함수
        {
            State state = State.Hiding;

            int charIndex = 0; // 숨겨지는 문자의 index값

            int charValue = 0; // 문자의 값을 int형으로 변환

            long pixelElementIndex = 0; // 픽셀(RGB)의 인덱스를 보유

            int zeros = 0; // 0이 8개가 되면 종료시켜야 하기 위해 0의 갯수를 세기 위한 변수

            int R = 0, G = 0, B = 0; // 픽셀 값을 넣기 위한 변수들

            for (int i = 0; i < bmp.Height; i++) // i를 0부터 이미지의 높이만큼 1씩 증가시킨다.
            {
                for (int j = 0; j < bmp.Width; j++) // // j를 0부터 이미지의 길이만큼 1씩 증가시킨다.
                {
                    Color pixel = bmp.GetPixel(j, i); // Color pixel에 현재 픽셀의 값을 넣는다.

                    R = pixel.R - pixel.R % 2; // R 픽셀을 2로 나눈 나머지 값을 R에 넣는다.
                    G = pixel.G - pixel.G % 2; // G 픽셀을 2로 나눈 나머지 값을 G에 넣는다.
                    B = pixel.B - pixel.B % 2; // B 픽셀을 2로 나눈 나머지 값을 B에 넣는다.
					// 각 픽셀에서 최하위 비트(LSB)를 지우는 작업

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0) // pixelElementIndex을 8로 나눈 값이 0과 같다면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 0이 8개일 경우 끝내기 위한 조건문
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // 이미지의 마지막 픽셀을 적용시키는 조건문
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp; // 문자가 숨겨진 bitmap 반환
                            }

                            if (charIndex >= text.Length) // 모든 문자가 숨겨졌는지 확인
                            {
                                state = State.Filling_With_Zeros; // 0을 추가한다.
                            }
                            else
                            {
                                charValue = text[charIndex++]; // 다음 문자로 이동하여 처리한다.
                            }
                        }

                        switch (pixelElementIndex % 3) // 어떤 픽셀의 LSB비트에 숨겨야 하는지에 대한 조건문
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2; // 문자의 LSB가 charValue%2가 된다.
                                        charValue /= 2; // 추가된 문자의 LSB 제거
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; // G값에 대하여 R값과 동일하게 동작

                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2; // G값에 대하여 R값과 동일하게 동작

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++; // 값이 8이 될때까지 증가시킨다.
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp) // 이미지를 추출하기위한 함수
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; // 이미지에서 추출된 문자를 넣는다.

            for (int i = 0; i < bmp.Height; i++)  // i를 0부터 이미지의 높이만큼 1씩 증가시킨다.
            {
                for (int j = 0; j < bmp.Width; j++)  // j를 0부터 이미지의 너비만큼 1씩 증가시킨다.
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀값을 받아온다.
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) // 픽셀에서 LSB값을 가져와 현재 문자의 오른쪽에 1비트를 추가한다.
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

                        if (colorUnitIndex % 8 == 0) // 8비트가 추가된 경우, 현재 문자를 결과 텍스트에 추가한다.
                        {
                            charValue = reverseBits(charValue);

                            if (charValue == 0) // 0이 8개인 경우에만 0이 될 수 있다.
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString(); // 현재 문자를 결과 텍스트에 추가.
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
