using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding,//아직 덜 숨긴 상태
            Filling_With_Zeros//숨긴 상태
        };
        /*
        class System.Drawing.Bitmap
        Bitmap 이미지 픽셀 데이터에 정의 된 작업에 사용되는 개체
        */
        public static Bitmap embedText(string text, Bitmap bmp)//text를 숨기는 함수
        {
            State state = State.Hiding;//숨기고있는 상태

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;//RGB의 기본 색상을 0, 0, 0으로 초기화

            for (int i = 0; i < bmp.Height; i++)//bmp의 height의 0부터 height 마지막 전까지 반복문
            {
                for (int j = 0; j < bmp.Width; j++)//bmp의 width의 0부터 width 마지막 전까지 반복문
                {
                    Color pixel = bmp.GetPixel(j, i);//현재 픽셀의 색깔을 저장
                    /*
                     해당 픽셀의 R, G, B값에서 값이 짝수면 0을 빼고 홀수이면 1을 뺀다.
                     */
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)//세번을 반복한다.
                    {
                        if (pixelElementIndex % 8 == 0)//pixelElementIndex 를 8로 나누었을때 나머지가 0이면 실행->문자 한개를 숨겼을경우
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//데이터를 모두 숨겼으면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//해당 bmp에서 (j,i)좌표에 있는 RGB값을 FromArgb(R, G, B)값으로 바꾼다
                                }

                                return bmp;//return
                            }

                            if (charIndex >= text.Length)//charIndex<=text.Length 의 뜻은 모든 text가 숨겨졌다는 뜻
                            {
                                state = State.Filling_With_Zeros;//따라서 state를 Filling_with_Zeros로 변경
                            }
                            else//모든 텍스트가 숨겨지지 않았다면...
                            {
                                charValue = text[charIndex++];//text[charIndex]값을 int자료형 charValue에 저장해놓고 charIndex+=1을 해준다.
                            }
                        }

                        switch (pixelElementIndex % 3)//RGB값을 돌아가면서 설정
                        {
                            case 0://R값
                                {
                                    if (state == State.Hiding)//숨기고있는 상태라면
                                    {
                                        R += charValue % 2;//R = R+charValue%2;
                                        charValue /= 2;//charValue = charValue/2;
                                    }
                                } break;
                            case 1://G값
                                {
                                    if (state == State.Hiding)//숨기고있는 상태라면
                                    {
                                        G += charValue % 2;//G = G+charValue%2;

                                        charValue /= 2;//charValue = charValue/2;
                                    }
                                } break;
                            case 2://B값
                                {
                                    if (state == State.Hiding)//숨기고있는 상태라면
                                    {
                                        B += charValue % 2;//B = B+charValue%2;

                                        charValue /= 2;//charValue = charValue/2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//해당 bmp에서 (j,i)좌표에 있는 RGB값을 FromArgb(R, G, B)값으로 바꾼다
                                } break;
                        }

                        pixelElementIndex++;//pixelElementIndex+1

                        if (state == State.Filling_With_Zeros)//숨김완료 상태라면
                        {
                            zeros++;//zeros+1
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;//추출된 텍스트는 비어있는 상태

            for (int i = 0; i < bmp.Height; i++)//이미지파일의 높이0부터Height 직전까지 반복
            {
                for (int j = 0; j < bmp.Width; j++)//이미지파일의 길이0부터Width 직전까지 반복
                {
                    Color pixel = bmp.GetPixel(j, i);//이미지파일 (j,i)위치의 픽셀을 pixel에 저장
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)//RGB값을 돌아가면서 설정
                        {
                            case 0://R값
                                {
                                    charValue = charValue * 2 + pixel.R % 2;//charValue*2에 R값을2로 나눈 나머지에 더한 후 charValue값에 저장
                                } break;
                            case 1://G값
                                {
                                    charValue = charValue * 2 + pixel.G % 2;//charValue*2에 G값을2로 나눈 나머지에 더한 후 charValue값에 저장
                                } break;
                            case 2://B값
                                {
                                    charValue = charValue * 2 + pixel.B % 2;//charValue*2에 B값을2로 나눈 나머지에 더한 후 charValue값에 저장
                                } break;
                        }

                        colorUnitIndex++;//colorUnitIndex+1

                        if (colorUnitIndex % 8 == 0)//colorUnitIndex를 8로 나누었을때 나머지가 0이면
                        {
                            charValue = reverseBits(charValue);//비트를 역순으로 바꾸어서 저장한다.

                            if (charValue == 0)//charValue가 0이면
                            {
                                return extractedText;//추출된 텍스트를 return
                            }
                            char c = (char)charValue;//char변수 c에 charValue를 저장

                            extractedText += c.ToString();//추출된 텍스트에 c 문자열을 추가한다
                        }
                    }
                }
            }

            return extractedText;//추출된 텍스트 return
        }

        public static int reverseBits(int n)//비트를 역순으로 바꾸는 함수
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
