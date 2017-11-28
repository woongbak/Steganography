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

        public static Bitmap embedText(string text, Bitmap bmp)  // 숨기기 위한 메소드
        {
            State state = State.Hiding;  // 숨길지 0으로 채울지의 상태를 저장

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;  // RGB 비트 값들을 저장할 변수 선언

            for (int i = 0; i < bmp.Height; i++)  // 이미지의 높이만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++)  // 이미지의 너비만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i);  // 이미지의 픽셀 정보를 얻는다.
                    
                    // 각각 RGB 값을 저장
                    R = pixel.R - pixel.R % 2;  
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)  // RGB 조정을 세 번 반복한다.
                    {
                        if (pixelElementIndex % 8 == 0)
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)  // 데이터를 다 숨겼으면 이미지를 반환한다.
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length)  // 문자열을 다 숨겼으면
                            {
                                state = State.Filling_With_Zeros;  // 0으로 채우는 상태로 변경
                            }
                            else
                            {
                                charValue = text[charIndex++];  // 숨길 문자열의 문자 저장
                            }
                        }

                        switch (pixelElementIndex % 3)  // RGB를 번갈아가면서 수정하여 데이터를 숨김
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)  // R의 값을 조정하여 숨긴다
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)  // G의 값을 조정하여 숨긴다
                           {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)  // B의 값을 조정하여 숨긴다
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }
                                    // 위에서 조정한 RGB값들로 하나의 픽셀 값을 설정
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        pixelElementIndex++;

                        // 0으로 채우는 상태면 zero값 증가
                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;  
                        }
                    }
                }
            }

            return bmp;  // 이미지의 모든 픽셀을 훓었으면 이미지를 반환한다.
        }

        public static string extractText(Bitmap bmp)  // 추출하기 위한 메소드
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;  // 빈 문자열 저장

            for (int i = 0; i < bmp.Height; i++)  // 이미지의 높이만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++)  // 이미지의 너비만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i);  // 이미지의 한 픽셀 값을 저장한다.

                    for (int n = 0; n < 3; n++)  // 데이터를 숨길 때와 동일하게 세 번 반복한다.
                    {
                        switch (colorUnitIndex % 3)  // 숨길 때와 같이 RGB 순서대로 조사하며, 이 때 역으로 값을 계산하여 저장한다.
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;  // R값 조정
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;  // G값 조정
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;  // B값 조정
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)  // 8개의 픽셀 조정을 했다면
                        {
                            charValue = reverseBits(charValue);  // 값을 뒤집어 저장

                            if (charValue == 0)  // 0이면
                            {
                                return extractedText;  // 추출된 데이터 반환(더 이상 값의 차이가 없음)
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString();  // 추출된 문자 저장
                        }
                    }
                }
            }

            return extractedText;  // 추출된 데이터 반환
        }

        public static int reverseBits(int n)  // 데이터 비트를 뒤집음
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
