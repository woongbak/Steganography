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

            int charIndex = 0; // int : 부호 있는 32비트 정수 

            int charValue = 0;

            long pixelElementIndex = 0; //Long : 부호 있는 64비트 정수 

            int zeros = 0;

            int R = 0, G = 0, B = 0;


            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {   
                    //bmp안에 있는 pixel 중에 가로 j,세로 i에 위치한 것에 대한 색깔 값 구하기
                    Color pixel = bmp.GetPixel(j, i);

                    //픽셀의 RGB값을 가져와 2로 나눈 나머지를 빼는 과정이다.
                    //이로 인해서 RGB변수값을 2진수로 나타낸다면 일의 자리는 0이다. 
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)
                    {   
                        //pixelElementIndex값을 이진수로 나타냈을 때 맨 마지막값이 0일 때 
                        if (pixelElementIndex % 8 == 0)
                        {
                            //stage값이 십진수로 1이고, zeros값이 8이라면?
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {   
                                //pixelElmentIndex값-1이 3으로 나누었을 때 나머지 값이 2보다 작으면 
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {   
                                    //Pixel에 해당하는 RGB값을 삽입한다. 
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
                                //bmp를 반환한다. 
                                return bmp;
                            }

                            //charIndex가 데이터 길이보다 크거나 같으면
                            if (charIndex >= text.Length)
                            {
                                //state에 십진수로 1을 채운다. 
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {   //아니라면 charValue에 텍스트 charIndex위치에 있는 값을 삽입
                                //후에 charIndex 1증가
                                charValue = text[charIndex++];
                            }
                        }

                        //색깔 구분 
                        switch (pixelElementIndex % 3)
                        {
                            //빨강색
                            case 0:
                                {   
                                    //state가 0이면 
                                    if (state == State.Hiding)
                                    {   
                                        //R에 charValue 이진수 일의 자리의 값을 더한다.
                                        R += charValue % 2;
                                        //R를 이진수로 바꿨을 때 자릿수를 줄인다.
                                        charValue /= 2;
                                    }
                                } break;
                            //초록색
                            case 1:
                                {
                                    //state가 0이면
                                    if (state == State.Hiding)
                                    {   
                                        //G에 charValue 이진수 일의 자리 값을 더한다.
                                        G += charValue % 2;
                                        //G를 이진수로 바꿨을 때 자릿수를 줄인다. 
                                        charValue /= 2;
                                    }
                                } break;
                            //파랑색
                            case 2:
                                {   
                                    //state가 0이면 
                                    if (state == State.Hiding)
                                    {
                                        //B에 charValue 이진수 일의 자리 값을 더한다.
                                        B += charValue % 2;
                                        //B를 이진수로 바꿨을 때 자릿수를 줄인다.
                                        charValue /= 2;
                                    }
                                    //pixel에 RGB값에 따라 색깔 넣기
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }
                        //pixelElementIndex에 1를 더함

                        pixelElementIndex++;
                        //state가 1이라면 zeros에 1을 더한다.
                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }
            //bmp 반환하기
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
                    //bmp안에 있는 pixel 중에 가로 j,세로 i에 위치한 것에 대한 색깔 값 구하기
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        //색깔 구별하기
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {   //pixel에 있는 R의 이진수 일의자리값 더하기 + 이전에 더한 값의 자릿수 올리기
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1:
                                {
                                    //pixel에 있는 G의 이진수 일의자리값 더하기 + 이전에 더한 값의 자릿수 올리기
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2:
                                {
                                    //pixel에 있는 B의 이진수 일의자리값 더하기 + 이전에 더한 값의 자릿수 올리기
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }
                        //colorUnitIndex 값 1더하기
                        colorUnitIndex++;
                        //colorUnitIndex 값이 8로 나눠 떨어진다면
                        if (colorUnitIndex % 8 == 0)
                        {
                            //charValue값을 뒤집어서 넣기
                            charValue = reverseBits(charValue);
                            //charValue 값이 0이라면 
                            if (charValue == 0)
                            {   
                                //extractedText를 반환하고
                                return extractedText;
                            }
                            //int형으로 저장되어 있던 charValue값을 char형으로 저장훈
                            char c = (char)charValue;

                            //extratedText에 문자열형태로 바꿔 c값 넣기
                            extractedText += c.ToString();
                        }
                    }
                }
            }
            //extractedText 반환하기
            return extractedText;
        }

        public static int reverseBits(int n)
        {

            int result = 0;
            //거꾸로 이진수 값 저장하기
            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;

                n /= 2;
            }
            //거꾸로 된 이진수 값 반환
            return result;
        }
    }
}
