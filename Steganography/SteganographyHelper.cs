using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State  //상태를 나타낸다.
        {
            Hiding,
            Filling_With_Zeros
        };

        //텍스트와 이미지를 받아 이미지 속에 텍스트를 숨긴다.
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;  

            int charIndex = 0;  //감출 문자의 인덱스를 저장하는 변수

            int charValue = 0;  //감출 문자 값를 저장하는 변수

            long pixelElementIndex = 0;  //픽셀요소의 인덱스 저장 변수

            int zeros = 0; 

            int R = 0, G = 0, B = 0;  //한 픽셀은 Red, Green, Blue로 이루어져 있다.
            //이미지의 전체의 픽셀에 하나씩 접근한다.
            for (int i = 0; i < bmp.Height; i++)  //이미지의 높이를 0부터 끝까지 접근
            {
                for (int j = 0; j < bmp.Width; j++)  //이미지의 넓이를 0부터 끝까지 접근
                {
                    Color pixel = bmp.GetPixel(j, i);  //한 픽셀을 가져온다

                    //각각 RGB의 LSB를 0으로 만든다.
                    R = pixel.R - pixel.R % 2;  
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)  //픽셀의 인덱스가 0 또는 8일때,
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)  //Filling_With_Zeros상태이고, zeros가 8일때
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));  //이미지 픽셀을 RGB값으로 설정
                                }

                                return bmp;  //이미지 반환
                            }

                            if (charIndex >= text.Length)  //가리키는 문자의 인덱스가 문자열의 길이와 같거나 더 크면
                            {
                                state = State.Filling_With_Zeros;  //state를 Filling_With_Zeros의 상태로 바꾸어준다.
                            }
                            else  //가리키는 문자의 인덱스가 문자열의 길이보다 작으면
                            {
                                charValue = text[charIndex++];  //현재 가리키고 있던 문자의 다음 문자값을 저장한다.
                            }
                        }

                        switch (pixelElementIndex % 3)  //픽셀요소의 인덱스 값을 3으로 나눈 나머지가
                        {
                            case 0:  //0일때,
                                {
                                    if (state == State.Hiding)  //Hiding 상태로 설정
                                    {
                                        R += charValue % 2;  //R에 비트저장 후
                                        charValue /= 2;  //마지막 비트 제거
                                    }
                                } break;
                            case 1:  //1일때,
                                {
                                    if (state == State.Hiding)  //Hiding 상태로 설정
                                    {
                                        G += charValue % 2;  //G에 비트저장 후
                                        charValue /= 2;  //마지막 비트 제거
                                    }
                                } break;
                            case 2:  //2일때,
                                {
                                    if (state == State.Hiding)  //Hiding 상태로 설정
                                    {
                                        B += charValue % 2;  //B에 저장 후
                                        charValue /= 2;  //마지막 비트 제거
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));  //j,i위치의 픽셀에 R,G,B값을 저장한다.
                                } break;
                        }

                        pixelElementIndex++;  //픽셀의 인덱스 1만큼 증가

                        if (state == State.Filling_With_Zeros)
                        {   //데이터를 숨겼으면 zero 증가
                            zeros++;
                        }
                    }
                }
            }

            return bmp;  //이미지 반환
        }

        public static string extractText(Bitmap bmp)  //이미지에서 숨긴 텍스트를 추출한다.
        {
            int colorUnitIndex = 0;  //변수 선언
            int charValue = 0;

            string extractedText = String.Empty;  //문자열을 추출하여 저장할 문자열 변수 설정

            //이미지의 전체 픽셀에 접근한다.
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);  //현재 위치의 픽셀을 가져온다.
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)  //colorUnitIdex를 3으로 나눈 나머지가
                        {
                            case 0:  //0이면,
                                {
                                    charValue = charValue * 2 + pixel.R % 2;  //R의 최하위비트를 가져와 charValue에 더해준다.
                                } break;
                            case 1:  //1이면,
                                {
                                    charValue = charValue * 2 + pixel.G % 2;  //G의 최하위비트를 가져와 charValue에 더해준다.
                                } break;
                            case 2:  //2이면,
                                {
                                    charValue = charValue * 2 + pixel.B % 2;  //B의 최하위비트를 가져와 charValue에 더해준다.
                                } break;
                        }

                        colorUnitIndex++;  //인덱스 증가

                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue);  //추출된 비트의 순서를 반대로 전환

                            if (charValue == 0)  //더 이상 가져올 비트가 없으면
                            {
                                return extractedText;  //추출한 문자열 반환
                            }
                            char c = (char)charValue;  //추출한 비트들을 문자로 바꿔서 저장

                            extractedText += c.ToString();  //문자열에 문자 저장
                        }
                    }
                }
            }

            return extractedText;  //추출한 문자열 반환
        }
        //비트의 순서를 반대로 바꿔준다.
        public static int reverseBits(int n)
        {   
            int result = 0;  //결과 저장 변수

            //총 8비트이므로 8번 반복
            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;
                n /= 2;
            }
            
            return result;  //결과 반환
        }
    }
}
