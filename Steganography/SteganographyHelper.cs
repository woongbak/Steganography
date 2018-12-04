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

        public static Bitmap embedText(string text, Bitmap bmp) //숨기는 함수 인자값으로 숨기고자하는 text와 사진을 받음.
        {
            State state = State.Hiding; 

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;
            // 사용되는 변수에 대한 초기화


            for (int i = 0; i < bmp.Height; i++)    //파일의 pixel 높이 만큼 한 픽셀씩 (행)
            {
                for (int j = 0; j < bmp.Width; j++) // 각 행의 열
                {
                    Color pixel = bmp.GetPixel(j, i);   //pixel이라는 변수에 
                                                        //왼쪽 위부터 오른쪽으로 그리고 아래로 이동하면서 pixel 할당
                    R = pixel.R - pixel.R % 2;  // R값을 0으로 맞춤 R = 1이라면 1 - 1, 0이라면 0 - 0
                    G = pixel.G - pixel.G % 2;  // R과 같은 방식으로 G 값을 0으로
                    B = pixel.B - pixel.B % 2;  // R과 같은 방식으로 B 값을 0으로

                    for (int n = 0; n < 3; n++) // 3번 반복문 각 픽셀은 R 8bits G 8bits B 8bits로 구성
                    {
                        if (pixelElementIndex % 8 == 0) 
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) 
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    
                                }

                                return bmp; //모든 픽셀을 완료한 리턴.
                            } // 

                            if (charIndex >= text.Length) //text의 길이가 적을 경우
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else //텍스트의 길이가 길 경우, value에 text를 하나씩 저장
                            {
                                charValue = text[charIndex++];
                            } // 문자를 하나씩 저장하는.
                        }

                        switch (pixelElementIndex % 3)  //swich를 통해 RGB를 순서대로 넣도록 했다.
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }   // R의 LSB에 원하는 값을 넣는다. 0/1 중에 하나로 그리고 charValue 초기화;
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }   // G의 LSB에 원하는 값을 넣는다. 0/1 중에 하나로
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break; // B의 LSB에 원하는 값을 넣는다. 0/1중에 하나로 그리고 Pixel을 저장한다.
                        }

                        pixelElementIndex++;    //PixelElement의 인덱스 값을 올린다.

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp; //완료된 파일을 리턴한다.
        }

        public static string extractText(Bitmap bmp) // 문자가 숨겨진 이미지 파일을 받는다.
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++) //이미지 파일을 hide와 같이 하나의 픽셀을 행렬의 요소로 본다.
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++) // 8bits * 3 
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;    //R의 저장된 값 가져온다.
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;    //G에 저장된 값 가져온다
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;    //B에 저장된 값 가져온다.
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue);

                            if (charValue == 0)
                            {
                                return extractedText;   //추출한다. Value가 없을 때
                            }
                            char c = (char)charValue; // c라는 변수에 가져온 값 bit값을 문자값으로 변환

                            extractedText += c.ToString(); // char c를 text에 저장
                        }
                    }
                }
            }

            return extractedText; // 리턴한다.
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
