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

        public static Bitmap embedText(string text, Bitmap bmp) // 숨기기 위한 메소드
        {
            State state = State.Hiding; // 처음에 우리는 이미지에 State를 숨깁니다.

            int charIndex = 0; // 감춰진 문자에 인덱스를 넣습니다.

            int charValue = 0; // 정수로 환산한 문자의 값을 가지고 있습니다.

            long pixelElementIndex = 0; // 현재 처리 중인 색상 요소(R 또는 G 또는 B)의 인덱스를 보유합니다.

            int zeros = 0; // 프로세스를 완료하고 추가된 후에 행에 0의 수를 넣기 위해 선언합니다.

            int R = 0, G = 0, B = 0; // 색상을 가지기 위해 선언합니다.

            for (int i = 0; i < bmp.Height; i++) // bmp의 줄을 지나갑니다.
            {
                for (int j = 0; j < bmp.Width; j++) // bmp의 열을 지나갑니다.
                {
                    Color pixel = bmp.GetPixel(j, i); // 현재 처리중인 픽셀을 담아둡니다.
                    // 이제 각 픽셀 요소에서 최소 유의 비트를 삭제합니다.
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    //각 픽셀에 대해 해당 요소(RGB)를 통과 합니다.
                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0) // 새 8비트가 처리되었는지 확인합니다.
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 전 과정이 끝났는지 확인하고 0을 8개 추가하면 메소드 실행합니다.
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length) // 모든 문자가 숨겨져 있는지 확인합니다.
                            {
                                state = State.Filling_With_Zeros; // 텍스트의 끝을 표시하기 위해 0을 추가합니다.
                            }
                            else
                            {
                                charValue = text[charIndex++]; // 다음 문자로 옮깁니다.
                            }
                        }

                        switch (pixelElementIndex % 3) // LSB에서 비트를 숨길 수 있는 픽셀 요소를 확인합니다.
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        // 문자에서 가장 오른쪽 비트는 (charValue % 2)가 된다.
                                        // 픽셀 요소의 LSB 대신 이 값을 넣습니다
                                        // 픽셀 요소의 LSB가 지워집니다.
                                        R += charValue % 2;
                                        charValue /= 2; //문자의 가장 오른쪽에 추가된 비트를 제거합니다.
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
                            zeros++; // 0 값이 8이 될 때까지 증가합니다.
                        }
                    }
                }
            }

            return bmp; // 이미지에 숨긴 파일 리턴
        }

        public static string extractText(Bitmap bmp) // 텍스트를 추출하기 위한 메소드
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; // 이미지에서 추출될 텍스트를 보관합니다.

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); // 각 픽셀에 대해 해당 요소를 통과합니다.
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    // 픽셀 요소에서 LSB를 가져옵니다.(픽셀).R % 2)
                                    // 그런 다음 현재 문자 오른쪽에 비트 1개를 추가합니다.
                                    // (charValue = charValue * 2)로 수행할 수 있습니다.
                                    // 추가된 비트로 바뀝니다.
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

                        // 8비트가 추가된 경우입니다.
                        // 그런 다음 현재 문자를 결과를 텍스트에 추가합니다.
                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue); // 매번 프로세스가 발생하기 때문에 reverse 해줍니다.

                            if (charValue == 0) // 문자를 읽다가 멈춰야만 0이 될수 있습니다.
                            {
                                return extractedText;
                            }
                            char c = (char)charValue; // 문자 값을 int에서 char로 변환합니다.

                            extractedText += c.ToString(); // 결과 텍스트에 현재 문자를 추가합니다.
                        }
                    }
                }
            }

            return extractedText; // 숨겨진 텍스트를 추출합니다.
        }

        public static int reverseBits(int n) //결과 값을 출력합니다.
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



















































































































































































































            {









