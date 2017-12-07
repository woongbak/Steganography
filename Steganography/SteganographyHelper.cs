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

        public static Bitmap embedText(string text, Bitmap bmp)//숨기기 위한 메소드,(숨길 text,image)
        {
            State state = State.Hiding;

            int charIndex = 0;//text의 문자열인덱스 변수

            int charValue = 0;//text의 문자 정수화

            long pixelElementIndex = 0;//RGB판단용

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)//사진 높이까지 i++
            {
                for (int j = 0; j < bmp.Width; j++)//사진 넓이까지 j++
                {
                    Color pixel = bmp.GetPixel(j, i);//(j,i)의 픽셀을 pixel변수에 저장

                    R = pixel.R - pixel.R % 2;  //pixel.R의 LSB를 0으로 세팅하여 변수 R의 저장
                    G = pixel.G - pixel.G % 2;  //pixel.G의 LSB를 0으로 세팅하여 변수 G의 저장
                    B = pixel.B - pixel.B % 2;  //pixel.B의 LSB를 0으로 세팅하여 변수 B의 저장
                    //pixel.R의 비트를 1011101X라고 했을 때 식 pixel.R - pixel.R % 2를 계산하면 X의 값에 상관없이 10111010으로 세팅

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)//pixel의 처음인덱스이거나 마지막인덱스(8)일떄 실행
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//text의 모든 문자들이 숨겨졌을 때
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)            
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//픽셀의 LSB를 0으로 다시 세팅
                                }

                                return bmp;//text가 숨겨진 이미지를 반환
                            }

                            if (charIndex >= text.Length)//charIndex가 text의 길이보다 클 때 실행
                            {
                                state = State.Filling_With_Zeros;//text의 모든 문자를 숨김
                            }
                            else
                            {
                                charValue = text[charIndex++];//text의 인덱스의 문자를 charvalue의 저장하고 text의 인덱스를 1증가
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {
                            case 0://R일떄
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;//R의 마지막비트에 charvalue의 2로 나눈 나머지의 값을 더하여 R의 저장
                                        charValue /= 2; //charvalue를 2로 나눔
                                    }
                                } break;
                            case 1: //G일때
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; //G의 마지막비트에 charvalue의 2로 나눈 나머지의 값을 더하여 R의 저장

                                         charValue /= 2;//charvalue를 2로 나눔
                                    }
                                } break;//B일때
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;//B의 마지막비트에 charvalue의 2로 나눈 나머지의 값을 더하여 R의 저장

                                        charValue /= 2;//charvalue를 2로 나눔
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//이미지의 RGB를 설정(텍스트가 숨겨진 이미지이므로 RGB의 값들이 변경됨)
                                } break;
                        }

                        pixelElementIndex++;//pixelElementIndex 증가

                        if (state == State.Filling_With_Zeros)//모두 숨겨졌으면
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;//text가 숨겨진 이미지를 반환
        }

        public static string extractText(Bitmap bmp)//추출하기 위한 메소드
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;//extractText 0으로 초기화

            for (int i = 0; i < bmp.Height; i++)//이미지의 높이까지 i++
            {
                for (int j = 0; j < bmp.Width; j++)//이미지의 넓이까지 j++
                {
                    Color pixel = bmp.GetPixel(j, i);//(j,i)의 픽셀을 pixel변수에 저장
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0://R
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //R의 LSB + charvalue*2를 charvalue의 저장
                                } break;
                            case 1://G
                                {
                                    charValue = charValue * 2 + pixel.G % 2; //G의 LSB + charvalue*2를 charvalue의 저장
                                } break;
                            case 2://B
                                {
                                    charValue = charValue * 2 + pixel.B % 2; //B의 LSB + charvalue*2를 charvalue의 저장
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)//pixel의 0비트일때와 8비트 일 때
                        {
                            charValue = reverseBits(charValue);//charvalue의 값 리버스

                            if (charValue == 0)//텍스트를 모두 출력하면
                            {
                                return extractedText;//숨긴텍스트 반환
                            }
                            char c = (char)charValue;//charvalue값을 char형으로 강제형변환

                            extractedText += c.ToString();//추출한 문자 text에저장
                        }
                    }
                }
            }

            return extractedText;//숨긴 텍스트 반환
        }

        public static int reverseBits(int n)//비트 뒤집기->스택에 넣을때 거꾸로 들어가야되므로
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
