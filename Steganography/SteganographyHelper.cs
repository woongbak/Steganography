using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State //상태 표시
        {
            Hiding,  //숨김
            Filling_With_Zeros //0으로 채움(데이터를 다 숨겼다는 걸 알려줌)
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding; //상태를 숨김 모드로 지정

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++) // 이미지의 높이까지 i 순회
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지의 너비까지 j 순회
                {
                    Color pixel = bmp.GetPixel(j, i); // 이미지의 (j,i) 위치에 있는 픽셀 가져오기

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2; //R,G,B 각각의 LSB를 0으로 설정

                    for (int n = 0; n < 3; n++) //3번 순회함(R/G/B)
                    {
                        if (pixelElementIndex % 8 == 0) //픽셀 인덱스가 8로 나누어 떨어진다면(=한 문자를 숨겼다면)
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //데이터를 다 숨긴 상태라면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) //픽셀 인덱스 -1 를 3으로 나눈 값이 2보다 작다면
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // (j,i) 위치인 픽셀에 R,G,B 값 저장
                                }

                                return bmp; // 데이터를 숨긴 이미지 반환
                            }

                            if (charIndex >= text.Length) //텍스트 길이보다 문자 인덱스가 크거나 같을 때
                            {
                                state = State.Filling_With_Zeros; //데이터 다 숨겼다고 상태 지정
                            }
                            else //텍스트 길이가 더 길다면
                            {
                                charValue = text[charIndex++]; //charValue에 해당 인덱스의 문자를 저장하고 인덱스 값 증가시킴.
                            }
                        }

                        switch (pixelElementIndex % 3)   //R,G,B 채널 결정
                        {
                            case 0: //R일 때
                                {
                                    if (state == State.Hiding)  //상태가 숨김 모드라면
                                    {
                                        R += charValue % 2; // R에 문자값 저장
                                        charValue /= 2; // charValue에 2로 나눈 몫 저장
                                    }
                                } break;
                            case 1: //G일 때
                                {
                                    if (state == State.Hiding)  //상태가 숨김 모드라면
                                    {
                                        G += charValue % 2; // G에 문자값 저장

                                        charValue /= 2; // charValue에 2로 나눈 몫 저장
                                    }
                                } break;
                            case 2: //B일 때
                                {
                                    if (state == State.Hiding)  //상태가 숨김 모드라면
                                    {
                                        B += charValue % 2; //B에 문자값 저장

                                        charValue /= 2; // charValue에 2로 나눈 몫 저장
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // (j,i) 위치인 픽셀에 R,G,B 값 저장
                                } break;
                        }

                        pixelElementIndex++; //픽셀 인덱스 증가

                        if (state == State.Filling_With_Zeros) // 데이터를 다 숨긴 상태라면
                        {
                            zeros++; //zeros 증가
                        }
                    }
                }
            }

            return bmp; // 데이터를 숨긴 이미지 반환
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; //추출할 텍스트는 비어있음

            for (int i = 0; i < bmp.Height; i++) // 이미지의 높이까지 i 순회
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지의 너비까지 j 순회
                {
                    Color pixel = bmp.GetPixel(j, i); // 이미지 (j,i) 위치에 있는 픽셀 가져오기
                    for (int n = 0; n < 3; n++) // 3번 순회
                    {
                        switch (colorUnitIndex % 3) // RGB 채널 결정
                        {
                            case 0:  // R
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //charValue 를 두 배한 값과, R 채널의 최하위 비트를 더하여 저장
                                } break;
                            case 1:  // G
                                {
                                    charValue = charValue * 2 + pixel.G % 2; //charValue 를 두 배한 값과, G 채널의 최하위 비트를 더하여 저장
                                } break;
                            case 2:  // B
                                {
                                    charValue = charValue * 2 + pixel.B % 2; //charValue 를 두 배한 값과, B 채널의 최하위 비트를 더하여 저장
                                } break;
                        }

                        colorUnitIndex++; //인덱스 값 증가

                        if (colorUnitIndex % 8 == 0) //인덱스가 8로 나누어 떨어지면
                        {
                            charValue = reverseBits(charValue); // 비트를 역순으로 바꿔서 저장

                            if (charValue == 0) //charValue값이 0이라면
                            {
                                return extractedText; //추출 텍스트 반환
                            }
                            char c = (char)charValue; //c에 문자형으로 바꾼 charValue를 저장

                            extractedText += c.ToString(); //추출 텍스트에 c를 더하여 저장
                        }
                    }
                }
            }

            return extractedText; //추출 텍스트 반환
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
