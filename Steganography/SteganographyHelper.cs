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

        public static Bitmap embedText(string text, Bitmap bmp) // 숨기기 위한 메소드. 텍스트와 비트맵 이미지 파일을 인자로 받음
        {
            State state = State.Hiding; // State를 Hiding으로 설정

            int charIndex = 0; // 숨겨진 문자의 개수 저장하는 변수
            
            int charValue = 0; // 숨길 텍스트의 문자 저장하는 변수

            long pixelElementIndex = 0; // RGB 값 저장하는 변수

            int zeros = 0; // 문자의 bit 수

            int R = 0, G = 0, B = 0; // R,G,B값 저장할 변수. 0으로 초기화
            // 각각의 픽셀을 루프로 돌면서 RGB 값을 얻기 위함
            for (int i = 0; i < bmp.Height; i++) // 이미지 파일의 높이가 1씩 증가하면서 반복
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지 파일의 너비가 1씩 증가하면서 반복
                {
                    Color pixel = bmp.GetPixel(j, i); // 각각의 픽셀에 값 저장

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2; // LSB를 0으로 만듦(값을 숨기는 데 사용하려고)

                    for (int n = 0; n < 3; n++) // R,G,B에 LSB를 각각 하나씩 넣으려고 3번 반복
                    {
                        if (pixelElementIndex % 8 == 0) // 8이거나 0 이면 (하나의 문자에 8비트 사용)
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // 변환할 값이 남았으면
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 그 픽셀에 R,G,B 값을 마저 세팅해줌
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length) // 텍스트를 다 숨겼으면
                            {
                                state = State.Filling_With_Zeros; // 숨기는 걸 완료한 state로 변경
                            }
                            else // 아닐 경우엔 마저 숨겨야 하니까
                            {
                                charValue = text[charIndex++]; // 숨길 문자열 가져오고 charindex 증가시킴
                            }
                        }

                        switch (pixelElementIndex % 3) // R,G,B 각각의 경우에 대해 switch문 돌림
                        {
                            case 0: // R인 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2; // 문자의 마지막 비트 저장
                                        charValue /= 2; // 자리수 맞춰줌
                                    }
                                } break;
                            case 1: // G인 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; // 문자의 마지막 비트 저장

                                        charValue /= 2; // 자리수 맞춰줌
                                    }
                                } break;
                            case 2: // B인 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2; // 문자의 마지막 비트 저장

                                        charValue /= 2; // 자리수 맞춰줌
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 문자를 LSB에 저장한 각각의 R,G,B를 픽셀에 저장함
                                } break;
                        }

                        pixelElementIndex++; // 인덱스 증가

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp) // 추출하기 위한 메소드. 비트맵 이미지 파일에서 텍스트 추출
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; // 텍스트 초기화

            for (int i = 0; i < bmp.Height; i++) // 이미지 파일의 높이가 1씩 증가
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지 파일의 너비가 1씩 증가
                {
                    Color pixel = bmp.GetPixel(j, i); // 각각의 픽셀에 값 저장
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) // 각각 R, G, B 인 경우에 대해 switch 구문 돌림
                        {
                            case 0: // R인 경우
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // LSB 비트 넣음
                                } break;
                            case 1: // G인 경우
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // LSB 비트 넣음
                                } break;
                            case 2: // B인 경우
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // LSB 비트 넣음
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue); // 추출할 때는 비트의 순서 리버스함

                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            char c = (char)charValue; // charvalue를 char형으로 변환

                            extractedText += c.ToString(); // 추출된 문자 삽입
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)
        {
            int result = 0;

            for (int i = 0; i < 8; i++) // 8bit 반복함
            {
                result = result * 2 + n % 2; // 이진수 자릿수 맞춤

                n /= 2;
            }

            return result;
        }
    }
}
