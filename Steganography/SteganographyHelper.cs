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
            // 상태를 숨김으로 설정
            State state = State.Hiding;

            // 글자 인덱스 저장
            int charIndex = 0;

            // 글자 값 저장
            int charValue = 0;

            // 픽셀 인덱스
            long pixelElementIndex = 0;

            // 루프 카운트 저장
            int zeros = 0;

            // R G B의 값 각각 선언
            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    // 픽셀 수 만큼 루프
                    // x, y 위치의 픽셀의 데이터를 가져온다.
                    Color pixel = bmp.GetPixel(j, i);

                    // 각 색상 값의 마지막 비트를 0으로 만든다.
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)
                    {
                        // 만약 8개의 픽셀이 지났을 경우
                        if (pixelElementIndex % 8 == 0)
                        {
                            // 만약 Filling_With_Zeros 상태인 동시에 zeros 가 8인 경우
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                // 
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    // 아직 적용안된 픽셀을 적용하기 위해 존재라고 하나 (원본 소스코드)
                                    // 실제로 쓰이는지 의문
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
                                // 모든 픽셀에 적용완료
                                return bmp;
                            }

                            if (charIndex >= text.Length)
                            {
                                // 만약 모든 글자를 다 숨겼을 경우 모드를 변환한다.
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                // 아직 숨길 글자가 남아있다면 인덱스를 추가한다.
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {
                            case 0:
                                {
                                    // 만약 상태가 숨김일 경우
                                    if (state == State.Hiding)
                                    {
                                        // R에 글자의 마지막 비트를 더하고
                                        R += charValue % 2;
                                        // 글자를 /2 한다. (비트 쉬프팅)
                                        charValue /= 2;
                                    }
                                } break;
                            case 1:
                                {
                                    // 만약 상태가 숨김일 경우
                                    if (state == State.Hiding)
                                    {
                                        // G에 글자의 마지막 비트를 더하고
                                        G += charValue % 2;
                                        // 글자를 /2 한다. (비트 쉬프팅)
                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        // B에 글자의 마지막 비트를 더하고
                                        B += charValue % 2;
                                        // 글자를 /2 한다. (비트 쉬프팅)
                                        charValue /= 2;
                                    }
                                    // R G B의 값을 모두 변경했을 경우 변경된 픽셀의 값을 적용한다.
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }
                        // 피겟ㄹ의 인덱스를 1 증가 시킨다.
                        pixelElementIndex++;
                        // 만약 Filling_With_Zeros 일 경우
                        if (state == State.Filling_With_Zeros)
                        {
                            // zeros 를 1증가 시킨다.
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp)
        {
            // 숨겨진 글자를 추출하는 함수이다.

            // 픽셀
            int colorUnitIndex = 0;
            // 글자 데이터
            int charValue = 0;

            // 빈 string을 선언한다.
            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    // j, i에 위치한 픽셀의 데이터를 가져온다.
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {

                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    // R의 마지막 비트를 더하고 *2 를 한다 (비트 쉬프팅)
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1:
                                {
                                    // G의 마지막 비트를 더하고 *2 를 한다. (비트 쉬프팅)
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2:
                                {
                                    // B의 마지막 비트를 더하고 *2를 한다. (비트 쉬프팅)
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }
                        // 처리한 픽셀의 개수를 1 증가 시킨다.
                        colorUnitIndex++;

                        // 8개의 픽셀을 처리할 때마다
                        if (colorUnitIndex % 8 == 0)
                        {
                            // CharValue의 비트를 거꾸로 뒤집는다.
                            charValue = reverseBits(charValue);

                            // 만약 그 결과가 0b00000000 일 경우 (0일 경우, 더이상 데이터를 뽑을수 없을 경우)
                            if (charValue == 0)
                            {
                                // 뽑아진 글자를 리턴한다.
                                return extractedText;
                            }
                            // 뒤집어진 비트를 char c에 저장한다.
                            char c = (char)charValue;
                            // extractedText에 뽑혀진 글자를 추가한다.
                            extractedText += c.ToString();
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)
        {
            // n의 비트를 거꾸로 뒤집는다.
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                // n의 첫번째 비트를 result의 마지막으로 옮긴다.
                result = result * 2 + n % 2;
                // n 비트 쉬프팅
                n /= 2;
            }
            // 결과를 리턴한다.
            return result;
        }
    }
}
