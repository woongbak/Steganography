using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding,                 // 숨김
            Filling_With_Zeros      // 0으로 채우기 (이 상태로 데이터 숨김 여부 판단 가능)
        };

        // text 숨기기
        public static Bitmap embedText(string text, Bitmap bmp) // 인자: payload인 숨길 text & carrier가 될 이미지 bmp
        {
            State state = State.Hiding; // enum 타입에 Hiding 값 대입

            int charIndex = 0; // 숨길 문자열 인덱스값 초기화

            int charValue = 0; // 숨길 문자열의 문자 정수값 초기화

            long pixelElementIndex = 0; // 픽셀 인덱스 초기화

            int zeros = 0; // zeros 초기화

            int R = 0, G = 0, B = 0; // R, G, B 초기화

            for (int i = 0; i < bmp.Height; i++)    // 다루는 픽셀을 위에서 아래로 내려가면서 바꿈
            {
                for (int j = 0; j < bmp.Width; j++) // 다루는 픽셀을 왼쪽에서 오른쪽으로 옮겨가며 바꿈
                {                                   // 결국에는 왼쪽에서 오른쪽으로 움직이는데 끝까지 가면 한 줄 내려가는 형식으로...
                    Color pixel = bmp.GetPixel(j, i);   // 현재 다룰 픽셀값을 pixel에 저장함

                    R = pixel.R - pixel.R % 2;  // 현재 다루는 픽셀의 R(빨강)값의 LSB만 0으로 바꿈
                    G = pixel.G - pixel.G % 2;  // 마찬가지로 G(초록)값의 LSB만 0으로 바꿈
                    B = pixel.B - pixel.B % 2;  // 마찬가지로 B(파랑)값의 LSB만 0으로 바꿈

                    for (int n = 0; n < 3; n++) // R, G, B 순회
                    {
                        if (pixelElementIndex % 8 == 0) // 한 문자 (8bit) 처리가 끝난 경우
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 문자열의 모든 문자를 처리한 경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)    // R,G,B의 마지막인 B까지 처리한 경우
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    // 다루던 픽셀에 R,G,B값 저장
                                }

                                return bmp; // 최종적으로 데이터 숨겨져 있는 사진 bmp 반환
                            }

                            if (charIndex >= text.Length)   // 현재 다루고 있는 문자의 인덱스가 문자열의 길이보다 크다면
                            {
                                state = State.Filling_With_Zeros;   // enum 타입에 Filling_With_Zeros 값 대입
                            }
                            else
                            {
                                charValue = text[charIndex++];      // 현재 문자의 정수값을 charValue에 저장
                            }                                       // 그리고 다음 문자의 인덱스값을 charIndex에 저장
                        }

                        switch (pixelElementIndex % 3)      // 픽셀의 R, G, B 중 어디에 저장할지 결정
                        {
                            case 0: // R에 저장
                                {
                                    if (state == State.Hiding)  // Hiding 상태일 경우
                                    {
                                        R += charValue % 2;     // 문자의 정수값의 LSB를 R에 더함
                                        charValue /= 2;         // 문자의 정수값 2로 나눔 (LSB 없앰)
                                    }
                                } break;
                            case 1: // G에 저장
                                {
                                    if (state == State.Hiding)  // Hiding 상태일 경우
                                    {
                                        G += charValue % 2;     // 문자의 정수값의 LSB를 G에 더함

                                        charValue /= 2;         // 문자의 정수값 2로 나눔 (LSB 없앰)
                                    }
                                } break;
                            case 2: // B에 저장
                                {
                                    if (state == State.Hiding)  // Hiding 상태일 경우
                                    {
                                        B += charValue % 2;     // 문자의 정수값의 LSB를 B에 더함

                                        charValue /= 2;         // 문자의 정수값 2로 나눔 (LSB 없앰)
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    // 현재 다루고 있는 픽셀의 RGB값 저장
                                } break;
                        }

                        pixelElementIndex++;        // 픽셀 인덱스 + 1

                        if (state == State.Filling_With_Zeros)  // Filling_With_Zeros 상태일 경우
                        {
                            zeros++;    // zeros + 1
                        }
                    }
                }
            }

            return bmp;     // 데이터가 숨겨져 있는 이미지 bmp 반환
        }

        // 숨겨진 text 추출하기
        public static string extractText(Bitmap bmp)    // 인자: 숨겨진 text가 존재하는 이미지 bmp
        {
            int colorUnitIndex = 0;
            int charValue = 0;      // 추출할 문자의 정수값 초기화

            string extractedText = String.Empty;    // 추출할 문자열 초기화 (empty string)

            for (int i = 0; i < bmp.Height; i++)    // 다루는 픽셀을 위에서 아래로 옮김
            {
                for (int j = 0; j < bmp.Width; j++) // 다루는 픽셀을 왼쪽에서 오른쪽으로 옮김
                {                                   // 결국에는 왼쪽에서 오른쪽으로 움직이는데 끝까지 가면 한 줄 내려가는 형식으로...
                    Color pixel = bmp.GetPixel(j, i);   // 현재 다룰 픽셀을 pixel에 저장
                    for (int n = 0; n < 3; n++)     // 각 픽셀마다 R, G, B 총 3번 수행
                    {
                        switch (colorUnitIndex % 3) // R, G, B 중 어디에 저장되어 있는지 판단
                        {
                            case 0:     // R
                                {
                                    charValue = charValue * 2 + pixel.R % 2;    // 문자 정수값이 저장될 charValue를 한 비트씩 옮긴 후 R의 LSB저장
                                } break;
                            case 1:     // G
                                {
                                    charValue = charValue * 2 + pixel.G % 2;    // 문자 정수값이 저장될 charValue를 한 비트씩 옮긴 후 G의 LSB 저장
                                } break;
                            case 2:     // B
                                {
                                    charValue = charValue * 2 + pixel.B % 2;    // 문자 정수값이 저장될 charValue를 한 비트씩 옮긴 후 B의 LSB 저장
                                } break;
                        }   // 결과적으로 문자 정수값이 왼쪽-오른쪽 정 반대로 저장이 됨

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)    // 한 문자 (8bit) 처리 마친 경우
                        {
                            charValue = reverseBits(charValue); // 제대로된 문자의 정수값을 얻기 위해 reverse Bits

                            if (charValue == 0) // 문자의 정수값이 0일 경우 (더이상 없을 경우)
                            {
                                return extractedText;   // 추출된 문자열 반환
                            }
                            char c = (char)charValue;   // 문자의 정수값(int)을 문자(char)로 변경

                            extractedText += c.ToString();      // 추출할 string에 문자 저장
                        }
                    }
                }
            }

            return extractedText;   // 추출된 문자열 반환
        }

        // 각 bit를 왼쪽-오른쪽 reverse
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
