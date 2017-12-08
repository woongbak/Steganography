using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding, // 숨김
            Filling_With_Zeros // 0으로 채우기
        };

        public static Bitmap embedText(string text, Bitmap bmp) /* 문자열 심는 메소드 */
        {
            State state = State.Hiding; // 숨김으로 상태 변경

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++) // 사진의 높이만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++) // 사진의 폭만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i); // (j,i)에 해당하는 픽셀을 사진에서 가져옴

                    R = pixel.R - pixel.R % 2; //빨간색(R)에 해당하는 LSB를 0으로 바꿈
                    G = pixel.G - pixel.G % 2; //초록색(R)에 해당하는 LSB를 0으로 바꿈
                    B = pixel.B - pixel.B % 2; //파란색(R)에 해당하는 LSB를 0으로 바꿈

                    for (int n = 0; n < 3; n++) // 3가지 색상 (RGB)
                    {
                        if (pixelElementIndex % 8 == 0) // 나머지 8이 0인 경우, 즉 RGB 픽셀의 시작인 경우
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp; // 사진 반환
                            }

                            if (charIndex >= text.Length) // 숨긴 문자 갯수가 숨기려는 문자 갯수 이상인 경우
                            {
                                state = State.Filling_With_Zeros; // 0으로 채우기 상태로 변경
                            }
                            else
                            {
                                charValue = text[charIndex++]; // 숨기려는 문자의 정수 값을 저장, 인덱스 증가
                            }
                        }

                        switch (pixelElementIndex % 3) // 픽셀 인덱스의 나머지 값, RGB에 따라 나눠서 처리
                        {
                            case 0: // 빨간색(R)인 경우
                                {
                                    if (state == State.Hiding) // 상태가 숨김인 경우
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 1: // 초록색(G)인 경우
                                {
                                    if (state == State.Hiding) // 상태가 숨김인 경우
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: // 파란색(B)인 경우
                                {
                                    if (state == State.Hiding) // 상태가 숨김인 경우
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 변경된 RGB 값으로 (j,i) 픽셀 RGB 값 재설정
                                } break;
                        }

                        pixelElementIndex++; // 픽셀 인덱스 증가

                        if (state == State.Filling_With_Zeros) // 상태가 0으로 채우기인 경우
                        {
                            zeros++; // 0의 개수 증가
                        }
                    }
                }
            }

            return bmp; // 사진 반한
        }

        public static string extractText(Bitmap bmp) /* 문자열 추출 메소드 */
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++) // 사진의 높이만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++) // 사진의 폭만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i); // (j,i)에 해당하는 픽셀을 사진에서 가져옴
                    for (int n = 0; n < 3; n++) // 3가지 색상 (RGB)
                    {
                        switch (colorUnitIndex % 3) // 픽셀 인덱스의 나머지 값, RGB에 따라 나눠서 처리
                        {
                            case 0: // 빨산색인 경우
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

                        colorUnitIndex++; // 색 인덱스 증가

                        if (colorUnitIndex % 8 == 0) // 나머지 8이 0인 경우, 즉 RGB 픽셀의 시작인 경우
                        {
                            charValue = reverseBits(charValue);

                            if (charValue == 0)
                            {
                                return extractedText; // 추출된 문자열 반환
                            }
                            char c = (char)charValue; // 정수 -> 문자

                            extractedText += c.ToString(); // 추출 문자열 버퍼에 문자 추가
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n) /* 비트를 반전 (0->1, 1->0) */
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2; // result << 1 & 새로운 LSB 설정

                n /= 2; // n >> 1
            }

            return result; // 결과 값 반환
        }
    }
}
