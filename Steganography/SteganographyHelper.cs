using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding,   //숨김상태
            Filling_With_Zeros  //0으로 채울 상태
        };

        public static Bitmap embedText(string text, Bitmap bmp) //text = 숨길 문자 ,bmp =텍스트를 숨길 이미지
        {
            State state = State.Hiding; //숨긴다

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); //각 픽셀 데이터 추출

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2; 
                    B = pixel.B - pixel.B % 2;//2로 나눈 값을 뺴서 0으로 만든다.

                    for (int n = 0; n < 3; n++) // RGB 전체에 텍스트 숨김
                    {
                        if (pixelElementIndex % 8 == 0) //문자형은 8비트
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //텍스트를 넣고 8비트를 이동, 문자열의 크기보다 이동했다면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //0으로 설정합니다.
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length) //다 저장했으면 상태를 Filling_With_Zeros로
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else //아니면 charValue에 저장
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {
                            case 0: //나머지가 0이면 R 비트에 저장하고 마지막 비트 삭제
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 1: //R비트가 아닌 G비트에 위경우처럼
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2://R비트가 아닌 B비트에 위경우처럼
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //숨긴 이미지 픽셀 세팅
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros) //Filling_With_Zeros상태라면 zeros를 증가
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp) //추출부분입니다.
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++) //전체를 순회합니다.
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0: //한비트 이동시키고 R비트를 붙임 즉 숨겼던것의 역으로
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1://한비트 이동시키고 G비트를 붙임 즉 숨겼던것의 역으로
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2://한비트 이동시키고 B비트를 붙임 즉 숨겼던것의 역으로
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0) //8비트를 채우면
                        {
                            charValue = reverseBits(charValue); //문자 추출

                            if (charValue == 0) 
                            {
                                return extractedText; 
                            }
                            char c = (char)charValue; //문자형으로 변환

                            extractedText += c.ToString();//문자열으로 변신
                        }
                    }
                }
            }

            return extractedText; //끝
        }

        public static int reverseBits(int n) //역변환할 변수 n
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2; //8비트 데이터를 역변환시켜요

                n /= 2;
            }

            return result;
        }
    }
}
