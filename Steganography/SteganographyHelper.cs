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

        public static Bitmap embedText(string text, Bitmap bmp) //문자열을 숨기기 위한 메소드
        {
            State state = State.Hiding;

            int charIndex = 0; //문자열 개수

            int charValue = 0; //문자의 값

            long pixelElementIndex = 0; //long 형식의 픽셀 값

            int zeros = 0;

            int R = 0, G = 0, B = 0; //픽셀값에 대해 R,G,B 모두 0으로 초기화

            for (int i = 0; i < bmp.Height; i++) //bmp파일의 높이를 모두 돌때까지
            {
                for (int j = 0; j < bmp.Width; j++) //bmp파일의 너비를 모두 돌때까지 (즉 한그림의 모든 픽셀을 돌겠다는 의미)
                {
                    Color pixel = bmp.GetPixel(j, i); //각 픽셀의 값을 저장

                    R = pixel.R - pixel.R % 2; /*RGB에 대해 LSB Least significant bit 최소 유효 비트를 0으로 설정*/
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    
                    for (int n = 0; n < 3; n++) 
                    {
                        if (pixelElementIndex % 8 == 0) // 문자 8비트를 의미
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) //픽셀 값에 대해 연산
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //픽셀의 마지막 비트에 대해 연산을 한다.
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length) //텍스트를 모두 숨긴다.
                            {
                                state = State.Filling_With_Zeros; //state에 숨긴 문자열을 저장
                            }
                            else //문자열이 남은 경우
                            {
                                charValue = text[charIndex++]; //문자열을 한개씩 증가시켜 계속적으로 작업을 수행한다.
                            }
                        }

                        switch (pixelElementIndex % 3) //픽셀 값 3은 RGB의 각각을 의미
                        {
                            case 0: //R을 의미
                                {
                                    if (state == State.Hiding) 
                                    {
                                        R += charValue % 2; //LSB 저장
                                        charValue /= 2;
                                    }
                                } break;
                            case 1: //G를 의미
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: //B를 의미
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //LSB에 저장된 각 R G B 를 픽셀에 저장
                                } break;
                        }

                        pixelElementIndex++; //픽셀 값 증가

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp) //문자열을 추출하기 위한 메소드
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; //문자열 초기화

            for (int i = 0; i < bmp.Height; i++) //bmp 파일의 높이를 증가
            {
                for (int j = 0; j < bmp.Width; j++) //bmp 파일의 너비를 증가
                {
                    Color pixel = bmp.GetPixel(j, i); //각 픽셀 값 저장
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0: //R을 의미
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //LSB 저장
                                } break;
                            case 1: //G를 의미
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2: //B를 의미
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0) //8비트 단위
                        {
                            charValue = reverseBits(charValue); //리버스하여 문자 값을 저장

                            if (charValue == 0) // 문자열 끝에 도달할 경우
                            {
                                return extractedText; // 추출된 문자열 리턴
                            }
                            char c = (char)charValue; //char형으로 변환

                            extractedText += c.ToString(); //작업하여 만들어진 문자열 삽입
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n) //비트를 리버스한다.
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
