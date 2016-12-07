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

        public static Bitmap embedText(string text, Bitmap bmp)   // 숨기려는 text와 bmp 받는다.
        {
            State state = State.Hiding;   //state에 hiding 값 저장

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++) // 해당 bmp height 만큼 반복문 진행
            {
                for (int j = 0; j < bmp.Width; j++) //해당 bmp width만큼 반복문 진행
                {
                    Color pixel = bmp.GetPixel(j, i);    //해당 bmp 픽셀 값을 pixel 변수에 저장

                    R = pixel.R - pixel.R % 2;     //해당 pixel 마지막 비트를 0으로 셋팅, 2진수로 표현했을 때 0 또는 1만 가진다. 어떤 경우에도 모두 LSB는 0이 된다.
                    G = pixel.G - pixel.G % 2;     //해당 pixel 마지막 비트를 0으로 셋팅
                    B = pixel.B - pixel.B % 2;     //해당 pixel 마지막 비트를 0으로 셋팅

                    for (int n = 0; n < 3; n++)  // R, G, B 세 가지 경우에 대해 진행
                    {
                        if (pixelElementIndex % 8 == 0)       // 한 개의 문자 1byte만큼 반복
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // RGB 모두 변경되지 않은 경우
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //변경된 R, G, B 값을 적용
                                }

                                return bmp; // 변경된 bmp 반환
                            }

                            if (charIndex >= text.Length)
                            {
                                state = State.Filling_With_Zeros;
                            } // 입력된 text 다 숨긴 경우 
                            else
                            {
                                charValue = text[charIndex++];
                            } // 입력된 text의 한 문자를 가져온다
                        }

                        switch (pixelElementIndex % 3)
                        {
                            case 0: // R에 해당하는 경우 pixelElementIndex%3의 결과가 0인 경우
                                {
                                    if (state == State.Hiding) // 아직 문자를 숨기고 있는 경우 
                                    {
                                        R += charValue % 2;  // 문자의 해당 비트 저장 
                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 1: // G에 해당하는 경우 pixelElementIndex%3의 결과가 1인 경우
                                {
                                    if (state == State.Hiding) // 아직 문자를 숨기고 있는 경우 
                                    {
                                        G += charValue % 2;  //문자의 해당 비트 저장

                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 2: // B에 해당하는 경우 pixelElementIndex%3의 결과가 2인 경우
                                {
                                    if (state == State.Hiding) // 아직 문자를 숨기고 있는 경우 
                                    {
                                        B += charValue % 2; //문자의 해당 비트 저장

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // RGB값 변경 다 되었을 경우 변경된 RGB 값 적용
                                }
                                break;
                        } // pixelElementindex 값에 따라 switch 문 실행. pixel 값마다 각 case에 대해 한 번씩 수행하게 된다. R, G, B 값 바꾸는 switch문이다.

                        pixelElementIndex++; //pixelElementindex 값 8까지 증가 

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp; // bmp 반환
        }

        public static string extractText(Bitmap bmp)  // text 숨긴 bmp 인자 받아온다
        {
            int colorUnitIndex = 0;
            int charValue = 0;   // 숨긴 문자 저장 charValue에 저장

            string extractedText = String.Empty; // 추출한 text extractedText에 저장

            for (int i = 0; i < bmp.Height; i++) // bmp의 높이만큼 반복문 실행
            {
                for (int j = 0; j < bmp.Width; j++) // bmp의 너비만큼 반복문 실행
                {
                    Color pixel = bmp.GetPixel(j, i); // bmp의 각 픽셀을 받아온다
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) // R, G, B 각각에 대해 수행
                        {
                            case 0: //R 
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // 숨긴 문자 추출
                                }
                                break;
                            case 1: //G
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // 숨긴 문자 추출
                                }
                                break;
                            case 2: //B
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // 숨긴 문자 추출
                                }
                                break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue);

                            if (charValue == 0) // 데이터 추출이 끝났을 경우
                            {
                                return extractedText; // 문자열 반환
                            }
                            char c = (char)charValue; // 문자열에 넣기 위해 형 변환

                            extractedText += c.ToString(); // 반환할 문자열에 추가 
                        }
                    }
                }
            }

            return extractedText; // 문자열 반환
        }

        public static int reverseBits(int n) // 역순으로 변경된 데이터 올바른 순서로 되돌린다
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
