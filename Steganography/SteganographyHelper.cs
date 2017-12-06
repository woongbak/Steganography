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

        //숨기려는 메소드
        //인자 - 숨기려는 텍스트와 사진
        public static Bitmap embedText(string text, Bitmap bmp) 
        {
            State state = State.Hiding;

            int charIndex = 0;  //text의 문자를 가르키기 위한 인덱스 변수

            int charValue = 0;  //text의 문자의 정수 값

            long pixelElementIndex = 0; 

            int zeros = 0;

            int R = 0, G = 0, B = 0;    //사진의 RGB

            for (int i = 0; i < bmp.Height; i++) //사진 높이
            {
                for (int j = 0; j < bmp.Width; j++) //사진 넓이
                {
                    Color pixel = bmp.GetPixel(j, i); //(j,i)영역의 픽셀 가져오기

                    R = pixel.R - pixel.R % 2; //R에서 LSB값 0으로 설정
                    G = pixel.G - pixel.G % 2; //G의 LSB값 0으로 설정
                    B = pixel.B - pixel.B % 2; //B의 LSB값 0으로 설정

                    for (int n = 0; n < 3; n++)  //3번 
                    {
                        if (pixelElementIndex % 8 == 0) //처음 시작 또는 하나의 문자(8bits)가 처리됐을 때
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //모든 문자를 숨겨쓸 때 
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) 
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //픽셀(j,i)의 마지막 비트를 0으로 
                                }

                                return bmp; //텍스트를 숨긴 사진 반환
                            }

                            //charIndex 와 text길이 비교
                            if (charIndex >= text.Length) //charIndex가 크거나 같으면 실행 
                            {
                                state = State.Filling_With_Zeros; //문자를 모두 숨김 -> state에 State.Filling_With_Zeros대입 
                            }
                            else //text 길이가 더 길때
                            {
                                charValue = text[charIndex++]; //text[charIndex++]에 해당하는 문자의 정수값 저장
                            }
                        }

                        //해당픽셀에 정수의 값 숨기기
                        switch (pixelElementIndex % 3) 
                        {
                            case 0: //나머지가 0일때 (R)
                                {
                                    if (state == State.Hiding) //숨기는 상태
                                    {
                                        R += charValue % 2; //R의 마지막 비트에 숨기려는 문자의 정수값에서 2로 나눈 나머지의 값 더하기
                                        charValue /= 2; //2로 나누기
                                    }
                                } break;
                            case 1: //나머지가 1일때 (G)
                                {
                                    if (state == State.Hiding) //숨기는 상태
                                    {
                                        G += charValue % 2; //R의 마지막 비트에 숨기려는 문자의 정수값 에서 2로 나눈 나머지의 값 더하기

                                        charValue /= 2; //2로나누기
                                    }
                                } break;
                            case 2: //나머지가 2일때 (V)
                                {
                                    if (state == State.Hiding) //숨기는 상태
                                    {
                                        B += charValue % 2; //B의 마지막 비트에 숨기려는 문자의 정수값 에서 2로 나눈 나머지의 값 더하기
                                        charValue /= 2; //2로 나누기
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //사진의 픽셀(j,i)에 해당하는 RGB 값을 새로 설정
                                } break;
                        }

                        pixelElementIndex++; //픽셀안의 RGB에 대한 인덱스를 1증가

                        if (state == State.Filling_With_Zeros) //모두 숨겼을 때 
                        {
                            zeros++; //1증가
                        }
                    }
                }
            }

            return bmp; //숨긴 사진 반환
        }

        //추출하기 위한 메소드
        //인자 - 문자를 추출하려는 사진
        public static string extractText(Bitmap bmp) 
        {
            int colorUnitIndex = 0;
            int charValue = 0;  //숨긴 문자의 정수값

            string extractedText = String.Empty; //빈 텍스트 

            for (int i = 0; i < bmp.Height; i++) //사진의 높이
            {
                for (int j = 0; j < bmp.Width; j++) //사진의 넓이
                {
                    Color pixel = bmp.GetPixel(j, i); //사진에서 (j,i)위치의 픽셀 가져오기
                    for (int n = 0; n < 3; n++)
                    {
                        //RGB의 마지막 LSB의 값으로 8비트의(문자) 만들기
                        switch (colorUnitIndex % 3) 
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //R의 마지막 비트와 charValue*2를 더하기
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2; //G의 마지막 비트와 charValue*2를 더하기
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2; //B의 마지막 비트와 charValue*2를 더하기
                                } break;
                        }

                        colorUnitIndex++; //1증가

                        if (colorUnitIndex % 8 == 0) //처음과 한 문자(8bits)가 끝났을 때
                        {
                            charValue = reverseBits(charValue); //8비트를 int형으로 바꾸기

                            if (charValue == 0) //모든 텍스트를 추출했을 때
                            {
                                return extractedText; //텍스트 반환
                            }
                            char c = (char)charValue; //정수를 char형으로 변환

                            extractedText += c.ToString(); //추출된 텍스트에 c저장
                        }
                    }
                }
            }

            return extractedText; //숨긴 텍스트 반환
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
