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
            State state = State.Hiding;

            int charIndex = 0; // 정수형 charindex 변수값을 선언한다.

            int charValue = 0; // 정수형 charvValue 변수 값을 선언한다.

            long pixelElementIndex = 0; // picelElementIndex 변수 값을 선언한다.

            int zeros = 0;// 정수형 zeros 변수 값을 선언한다.

            int R = 0, G = 0, B = 0;// R,G,B 변수 값을 선언한다.

            for (int i = 0; i < bmp.Height; i++) // bmp파일의 높이값에 반복문을 실행한다.
            {
                for (int j = 0; j < bmp.Width; j++)  // bmp파일의 넓이값에 반복문을 실행한다.
                {
                    Color pixel = bmp.GetPixel(j, i); // bmp 파일의 픽셀값을 가져온다.

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2; // R,G,B의 값을 0으로 만든다.

                    for (int n = 0; n < 3; n++) // 0,1,2 일때 실행, 한 픽셀에 대한 반복문을 실행한다.
                    {
                        if (pixelElementIndex % 8 == 0) //pixelElementindex를 8로 나누었을 때 나머지가 0일 때 실행
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // state가 1이고, zeros가 8일때 실행
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // pixelElenetInde01 -1 을 3으로 나눈 나머지가 2보다 작을 때
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//j,i에 픽셀 정보를 넣는다.
                                }

                                return bmp; // bmp를 반환한다.
                            }

                            if (charIndex >= text.Length)  // charindex가 text.length의 길이보다 크거나 같을 때
                            {
                                state = State.Filling_With_Zeros; // state에 state 구조체의 filling_with_zeros 저장하고, 0으로 만들어준다.
                            }
                            else
                            {
                                charValue = text[charIndex++]; // charvalue를 chardindex에 저장하고 charindex 값을 1 증가시킨다.
                            }
                        }

                        switch (pixelElementIndex % 3) // RGB 순서대로 값을 넣어준다. 
                        {
                            case 0:
                                {
                                    if (state == State.Hiding) // state가 state.hiding 일 때
                                    {
                                        R += charValue % 2; // charvalue/2의 나머지를 더한 값을 R에 저장한다.
                                        charValue /= 2; // charvalue/2로 나눈 값을 설정
                                    }
                                } break;
                            case 1: // 나머지가 1일 때
                                {
                                    if (state == State.Hiding) // state가 state.hiding 일 때
                                    {
                                        G += charValue % 2; // charvalue/2 의 나머지를 G값에 더해준다.

                                        charValue /= 2; //charvalue/2로 나눈 값을 설정
                                    }
                                } break;
                            case 2:  // 나머지가 2일 때
                                {
                                    if (state == State.Hiding) // state가 state.hiding 일 때
                                    {
                                        B += charValue % 2; // charvalue/2 의 나머지를 B값에 더해준다.

                                        charValue /= 2;  //charvalue/2로 나눈 값을 설정
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //세팅된 RGB값을 pixel에 넣는다.
                                } break;
                        }

                        pixelElementIndex++; //pixelElementindex값을 1 증가시킨다. 

                        if (state == State.Filling_With_Zeros) // state가 0일 때
                        {
                            zeros++; //zeros를 1증가 시킨다.
                        }
                    }
                }
            }

            return bmp; // bmp 반환한다.
        }

        public static string extractText(Bitmap bmp) // 데이터를 추출하기 위한 메소드
        {
            int colorUnitIndex = 0;// int형 변수 colorunitindex를 0값으로 선언한다. 
            int charValue = 0; // int형 변수 charvalue를 0값으로 선언한다.

            string extractedText = String.Empty;  // 추출할 문자를 string.empty로 설정해준다.

            for (int i = 0; i < bmp.Height; i++)  // bmp파일의 높이값에 반복문을 실행한다
            {
                for (int j = 0; j < bmp.Width; j++) // bmp파일의 넓이값에 반복문을 실행한다
                {
                    Color pixel = bmp.GetPixel(j, i); // bmp 파일의 픽셀값을 가져온다.
                    for (int n = 0; n < 3; n++) // 0,1,2 일때 실행, 한 픽셀에 대한 반복문을 실행한다. 
                    {
                        switch (colorUnitIndex % 3) // colorunitindex/3의 나머지의 switch문
                        {
                            case 0:  //나머지가 0일 때
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // R값의 LSB 추출한다.
                                } break;
                            case 1:  //나머지가 1일 때
                                {
                                    charValue = charValue * 2 + pixel.G % 2;  // G값의 LSB 추출한다.
                                } break;
                            case 2:  //나머지가 2일 때
                                {
                                    charValue = charValue * 2 + pixel.B % 2;   // B값의 LSB추출한다.
                                } break;
                        }

                        colorUnitIndex++;  // colorunitindex 값에 1을 증가시킨다.

                        if (colorUnitIndex % 8 == 0)  //colorunitindex/8로 나눈 나머지가 0 일때
                        {
                            charValue = reverseBits(charValue); // bit값을 reverse 해준다. 

                            if (charValue == 0) // 추출한 문자가 0일때
                            {
                                return extractedText;  //추출된 텍스트를 리턴한다.
                            }
                            char c = (char)charValue; // charvalue를 저장한다.

                            extractedText += c.ToString();  // 추출된 문자를 text로 저장한다.
                        }
                    }
                }
            }

            return extractedText;  // 추출된 문자를 반환한다.
        }

        public static int reverseBits(int n) // 거꾸로 저장된 비트를 reverse 해준다.
        {
            int result = 0;

            for (int i = 0; i < 8; i++) // 8비드마다 반복한다.
            {
                result = result * 2 + n % 2; // result에 n의 lsb를 누적한다.

                n /= 2;
            }

            return result;
        }
    }
}
