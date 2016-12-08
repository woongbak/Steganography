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

        public static Bitmap embedText(string text, Bitmap bmp)//이미지를 숨기기 위한 메소드이다.
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;//zeros의 값을 0으로 놓는다.

            int R = 0, G = 0, B = 0; //R,G,B의 값을 0으로 놓는다.

            for (int i = 0; i < bmp.Height; i++) //이미지의 높이(height)를 0부터 1씩 증가시킨다.
            {
                for (int j = 0; j < bmp.Width; j++) //이미지의 길이(width)를 0부터 1씩 증가시킨다.
                {
                    Color pixel = bmp.GetPixel(j, i); //pixel 인자값에 현재의 j,i값을 넣는다.

                    R = pixel.R - pixel.R % 2; 장
                    G = pixel.G - pixel.G % 2; 
                    B = pixel.B - pixel.B % 2; 
                    //LSB의 값을 0으로 바꾸어 준다.
                    for (int n = 0; n < 3; n++) //R,G,B의 값을 지정하기 위해 3번 반복한다.
                    {
                        if (pixelElementIndex % 8 == 0)//pixelElement의 값을 8로 나눈값이 0일 경우
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//0의 갯수가 8개일 경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)//pixel값을 바꾸기 위한 작업
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;//문자가 숨겨진 이미지를 반환한다.
                            }

                            if (charIndex >= text.Length)//text를 다 숨겼을 경우에 state를 변경한다.
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else//text를 다 못숨겼을 경우
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)//pixelElement의 값을 3으로 나눴을 경우의 나머지
                        {
                            case 0://R의 경우
                                {
                                    if (state == State.Hiding) //hiding 상태이면
                                    {
                                        R += charValue % 2;//R의 값을 변경
                                        charValue /= 2;
                                    }
                                } break;
                            case 1://G의 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;//G의 값을 변경

                                        charValue /= 2;
                                    }
                                } break;
                            case 2://B의 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;//B의 값을 변경

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//바뀐 RGB 값으로 변경
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;//1씩 증가
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp)//문자를 추출하기 위한 메소드이다.
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;//이미지에서 text를 추출한다.

            for (int i = 0; i < bmp.Height; i++) //이미지의 높이(height)를 0부터 1씩 증가시킨다.
            {
                for (int j = 0; j < bmp.Width; j++)//이미지의 높이(width)를 0부터 1씩 증가시킨다.
                {
                    Color pixel = bmp.GetPixel(j, i);//각 픽셀 값을 받는다.
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)//LSB의 값을 가져와 3으로 나눈다.
                        {
                            case 0://R의 경우
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1://G의 경우
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2://B의 경우
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)//8개의 비트를 다 읽은 경우
                        {
                            charValue = reverseBits(charValue);

                            if (charValue == 0)//추출 값이 0인 경우
                            {
                                return extractedText;//문자를 추출한다.
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString();//문자형으로 저장
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n) //역변환
        {
            int result = 0; // 결과값을 0으로 지정

            for (int i = 0; i < 8; i++) //8번 반복
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }
    }
}
