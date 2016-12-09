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
        //텍스트를 숨기는 메소드  문자열과 사진파일을 입력받음.
        {
            State state = State.Hiding; // hide state 로 설정 

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)// i = 높이 
            {
                for (int j = 0; j < bmp.Width; j++)//j = 너비 
                {
                    Color pixel = bmp.GetPixel(j, i);// 그림의 한 픽셀(i,j)을 지정

                    R = pixel.R - pixel.R % 2;// RGB값의 마지막 비트를 0으로 만듬.(1을 빼거나, 빼지 않거나)
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)// 8번의 루프마다 진행 실행 
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//filling with zeros와 zeros 둘다 8이면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)// modulo 3은 0,1,2만 가능하므로 2만 아니라면  
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//setpixel 실행 
                                }

                                return bmp;// 사진 파일을 반환
                            }

                            if (charIndex >= text.Length)//charindex가 문자열 길이 보다 크거나 같다면
                            {// 즉 문자열을 다 숨겼다면 
                                state = State.Filling_With_Zeros;//0으로 채운다.
                            }
                            else
                            {
                                charValue = text[charIndex++];//아니면 charvalue를 text의 charindex번째로 채운다.
                            }
                        }

                        switch (pixelElementIndex % 3)// pixelindex를 3으로 modulo 연산했을때
                        {
                            case 0://0이라면 
                                {
                                    if (state == State.Hiding)// 상태 체크 
                                    {
                                        R += charValue % 2;
                                        //R에 글자 마지막 비트 추가 
                                        charValue /= 2;
                                        // value를 2로 나눈다.
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;
                                        //G에 그다음 비트 추가 
                                        charValue /= 2;
                                        //value를 또 2 로 나눈다.
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;
                                        //B에 다음 비트 추가 
                                        charValue /= 2; 
                                        //2로 나눈다
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                    //그 결과를 저장한다 
                                } break;
                        }

                        pixelElementIndex++;
                        // 다음 픽셀로 이동 

                        if (state == State.Filling_With_Zeros)// state가 filling_with_zeros라면 
                        {
                            zeros++;// zeros 증가 
                        }
                    }
                }
            }

            return bmp;// 사진파일 반환 
        }

        public static string extractText(Bitmap bmp)// text 추출 
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)// 높이 
            {
                for (int j = 0; j < bmp.Width; j++)//너비 
                {
                    Color pixel = bmp.GetPixel(j, i);// 픽셀 가져옴 
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)// 문자열을 추출
                        {
                            //RGB값의 마지막 비트를 더해주면서, 위의 문자 삽입 원리에서 2씩 나눴던것 처럼 
                            //2씩 곱해준다.
                            case 0:
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

                        colorUnitIndex++;// 다음 컬러 유닛 

                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue);//비트의 순서를 반대로 해준다.

                            if (charValue == 0)// 계속 0 만 추출되었다면 
                            {
                                return extractedText;// 반환 
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString();// 추출된 텍스에 c.tostring 추가 
                        }
                    }
                }
            }

            return extractedText;
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
