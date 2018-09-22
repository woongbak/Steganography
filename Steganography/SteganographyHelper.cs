
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
            State state = State.Hiding; //하이딩 버튼 클릭 상태

            int charIndex = 0; //택스트 자리수

            int charValue = 0; //택스트값

            long pixelElementIndex = 0; 

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); //픽셀 정보

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)// 한글자 끝나면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) 
                                //밑의 케이스 2번의 픽셀 정보 변경이 아니라 0 1번에서 끝나면 여기서 변경
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //픽셀 변경
                                }

                                return bmp;
                            }


                            if (charIndex >= text.Length)
                            {
                                state = State.Filling_With_Zeros; //아스키코드 RGB에 넣는거 끝난거 알려줌
                                
                            }
                            else
                            {

                                charValue = text[charIndex++]; // aaa넣으면 아스키로 97 97 97  여기서 자리별로 차례로 나눠줌  

                            }
                        }



                        switch (pixelElementIndex % 3)//a 기준 아스키 97
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;//R 변경(97%2) 1
                                        charValue /= 2; // 97/2 = 48
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;// 48%2 0

                                        charValue /= 2;// 48/2 = 24
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2; // 24%2 0

                                        charValue /= 2; // 24 /2 = 12 ....
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //픽셀설정
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        } //제로 8번까지 올려줌
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) //RGB 3개여서 케이스 3번  
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // 위의 케이스 과정 역 픽셀에서 2진수 값 빼오는 과정
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

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0) //8개씩 끊어줌 글자하나단위로
                        {
                            charValue = reverseBits(charValue); // 2진수 10진수 변환 함수 호출

                            if (charValue == 0)// 문자값 비여있으면 실행
                            {
                                return extractedText; //문자열 반환
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString(); //문자열로 변환
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)
        {
            int result = 0; //위의 케이스 과정 한번더 해주는데 8개씩 글자 하나로 나눠서 2진수 10진수 아스키로 바꿔줌

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }
    }
}
