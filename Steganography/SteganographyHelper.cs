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
        //이미지에 데이터를 숨기는 함수
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;//숨기는 기능 state 설정

            int charIndex = 0;//숨길 데이터의 index를 저장하는 변수

            int charValue = 0;//숨길 데이터의 문자를 저장하는 변수

            long pixelElementIndex = 0;//픽셀 R,G,B의 인덱스를 저장하는 변수

            int zeros = 0;//0의 갯수를 세기 위해 선언된 변수

            int R = 0, G = 0, B = 0;//픽셀 R,G,B의 값을 각각 저장하는 변수

            for (int i = 0; i < bmp.Height; i++)//이미지의 높이만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++)//이미지의 너비만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i);//픽셀값을 가져옴

                    R = pixel.R - pixel.R % 2;//R에서 R을 2로 나눈 나머지로 뺀 값을 R에 저장
                    G = pixel.G - pixel.G % 2;//G픽서 G을 2로 나눈 나머지로 뺀 값을 G에 저장
                    B = pixel.B - pixel.B % 2;//B에서 B을 2로 나눈 나머지로 뺀 값을 B에 저장

                    for (int n = 0; n < 3; n++)//픽셀 R,G,B를 세팅하기 위해 3번 반복
                    {
                        if (pixelElementIndex % 8 == 0)//숨길 문자를 숨기고 다음 문자를 숨겨야 할 경우 
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//더이상 숨길 문자가 없고 0의 갯수가 8개일 경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//픽셀 R,G,B 저장
                                }

                                return bmp;//데이터가 숨겨진 이미지 반환
                            }

                            if (charIndex >= text.Length)//데이터를 모두 숨겼을 경우
                            {
                                state = State.Filling_With_Zeros;//Hiding에서 Filling_With_Zeros로 state 변경
                            }
                            else//숨길 데이터를 아직 다 숨기지 못했을 경우
                            {
                                charValue = text[charIndex++];//숨길 다음 문자 처리
                            }
                        }

                        switch (pixelElementIndex % 3)//픽셀 R,G,B를 각각 변경
                        {
                            case 0://R을 변경
                                {
                                    if (state == State.Hiding)//state가 Hiding일 경우
                                    {
                                        R += charValue % 2;//숨길 문자를 2로 나눈 나머지를 R에 저장
                                        charValue /= 2;//숨길 문자를 2로 나눈 값을 저장
                                    }
                                } break;
                            case 1://G를 변경
                                {
                                    if (state == State.Hiding)//state가 Hiding일 경우
                                    {
                                        G += charValue % 2;//숨길 문자를 2로 나눈 나머지를 G에 저장

                                        charValue /= 2;//숨길 문자를 2로 나눈 값을 저장
                                    }
                                } break;
                            case 2://B를 변경
                                {
                                    if (state == State.Hiding)//state가 Hiding일 경우
                                    {
                                        B += charValue % 2;//숨길 문자를 2로 나눈 나머지를 B에 저장

                                        charValue /= 2;//숨길 문자를 2로 나눈 값을 저장
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//픽셀 R,G,B 저장
                                } break;
                        }

                        pixelElementIndex++;//pixelElementIndex 1 증가

                        if (state == State.Filling_With_Zeros)//state가 Filling_with_Zeros일 경우
                        {
                            zeros++;//zeros 1 증가 
                        }
                    }
                }
            }

            return bmp;//데이터가 숨겨진 이미지 반환
        }
        //이미지에서 데이터를 추출하는 함수
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;//픽셀 R,G,B의 index를 저장하는 변수
            int charValue = 0;//숨길 데이터의 문자를 저장하는 변수

            string extractedText = String.Empty;//추출된 데이터를 저장하기 위한 변수

            for (int i = 0; i < bmp.Height; i++)//이미지의 높이만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++)//이미지의 너비만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i);//픽셀값을 가져옴
                    for (int n = 0; n < 3; n++)//픽셀 R,G,B를 세팅하기 위해 3번 반복
                    {
                        switch (colorUnitIndex % 3)//픽셀 R,G,B에 따른 데이터 추출
                        {
                            case 0://R일 경우
                                {    
                                    charValue = charValue * 2 + pixel.R % 2;//charValue에 charValue를 2배하여 R을 2로 나눈 나머지를 더한 값을 저장
                                } break;
                            case 1://G일 경우
                                {
                                    charValue = charValue * 2 + pixel.G % 2;//charValue에 charValue를 2배하여 G을 2로 나눈 나머지를 더한 값을 저장
                                } break;
                            case 2://B일 경우
                                {
                                    charValue = charValue * 2 + pixel.B % 2;//charValue에 charValue를 2배하여 B을 2로 나눈 나머지를 더한 값을 저장
                                } break;
                        }

                        colorUnitIndex++;//colorUnitIndex 1 증가

                        if (colorUnitIndex % 8 == 0)//하나의 문자를 추출한 경우
                        {
                            charValue = reverseBits(charValue);//추출되는 과정에서 반대로 저장된 비트들을 원상태로 되돌리기 위해 reverseBits 함수 호출

                            if (charValue == 0)//더이상 추출될 데이터가 없을 경우
                            {
                                return extractedText;//추출된 데이터 반환
                            }
                            char c = (char)charValue;//추출된 데이터를 char형으로 형변환

                            extractedText += c.ToString();//extractedText에 추출된 데이터 저장
                        }
                    }
                }
            }

            return extractedText;//추출된 데이터 반환
        }

        public static int reverseBits(int n)//반대로 저장된 비트들을 다시 원상태로 되돌리는 함수
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
