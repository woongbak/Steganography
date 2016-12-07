using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding,
            Filling_With_Zeros// = 1
        };

        public static Bitmap embedText(string text, Bitmap bmp)//숨기기위한 메소드이다. text와 이미지를 인자로 받는다.
        {
            State state = State.Hiding;//state를 hiding으로 설정해준다.

            int charIndex = 0;//int형 스트링의인덱스 값 0으로 초기화 시켜준다.

            int charValue = 0;//int형 변수 값 0으로 초기화 시켜준다.

            long pixelElementIndex = 0;//long형 변수 픽셀데이터 인덱스 값 0으로 초기화 시켜준다.

            int zeros = 0;//스트링 끝날때 이용할 int형 변수 값 0으로 초기화 시켜준다.

            int R = 0, G = 0, B = 0;//색을 표현하는 int형 변수 R,G,B 값 0으로 초기화 시켜준다.

            for (int i = 0; i < bmp.Height; i++)//이미지의 높이까지 for문을 돌려준다.
            {
                for (int j = 0; j < bmp.Width; j++)//이미지의 넓이 까지 for문을 돌려준다.
                {
                    Color pixel = bmp.GetPixel(j, i);//for문을 돌려 얻은(j,i)위치로 설정해준다.

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;//0으로 설정하기 위한 과정이다. 

                    for (int n = 0; n < 3; n++)//n=0,1,2일때 실행한다.
                    {
                        if (pixelElementIndex % 8 == 0)//픽셀데이터 인덱스 값이 0일때 (8비트한번지날때)
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//state가1이고, zeros가 8일때 실행된다.
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)//아래 switch문의 두번째 case를 만족시키지 못했을 경우에 수행된다.
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//j,i번째 위치에 R,G,B값을 집어넣는다.
                                }

                                return bmp;//bmp반환해준다.
                            }

                            if (charIndex >= text.Length)//스트링의 인덱스가 들어온 문자의 길이보다 같거나 길때 수행된다.
                            {
                                state = State.Filling_With_Zeros;//state를 1로 선언해준다.
                            }
                            else//그렇지 않을경우에,
                            {
                                charValue = text[charIndex++];//charValue에 들어온문자의 스트링인덱스를 증가시킨값을 저장해준다.
                            }
                        }

                        switch (pixelElementIndex % 3)//switch문을 돌아 RGB에 순서대로 값 을 넣는다.
                        {
                            case 0://R
                                {
                                    if (state == State.Hiding)//State.Hiding일때,
                                    {
                                        R += charValue % 2;//R에 charValue %2 한 값의 나머지를 더해준 값을 저장한다. 
                                        charValue /= 2;//charValue값을 2로 나눈값으로 다시 설정해주는데 그러면 다음비트로 이동하게 된다.
                                    }
                                } break;//탈출한다.
                            case 1://G
                                {
                                    if (state == State.Hiding)//State.Hiding일때,
                                    {
                                        G += charValue % 2;//G에 charValue %2 한 값의 나머지를 더해준 값을 저장한다.

                                        charValue /= 2;//다음비트로 이동한다.
                                    }
                                } break;//탈출한다.
                            case 2://B
                                {
                                    if (state == State.Hiding)//State.Hiding일때,
                                    {
                                        B += charValue % 2; //B에 charValue % 2 한 값의 나머지를 더해준 값을 저장한다.

                                        charValue /= 2;//다음비트로 이동한다.
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//지금까지 구한RGB를j,i번째 픽셀에 저장한다.
                                } break;//탈출한다.
                        }

                        pixelElementIndex++;//픽셀 요소 인덱스 증가

                        if (state == State.Filling_With_Zeros)//state가 1일때
                        {
                            zeros++;//zeros증가시켜준다.
                        }
                    }
                }
            }

            return bmp;//bmp반환해준다.
        }

        public static string extractText(Bitmap bmp)//추출하기 위한 메소드 이다. 이미지를 인자로 받는다.
        {
            int colorUnitIndex = 0;//int형변수 0으로 초기화시켜준다.
            int charValue = 0;//int형변수 0으로 초기화 시켜준다.

            string extractedText = String.Empty;//추출할 문자 string.Empty로 설정해준다.

            for (int i = 0; i < bmp.Height; i++)//이미지의 높이만큼 반복해준다.
            {
                for (int j = 0; j < bmp.Width; j++)//이미지의 넓이만큼 반복해준다.
                {
                    Color pixel = bmp.GetPixel(j, i);//이미지의 픽셀을 j,i번째 픽셀로 설정해준다.
                    for (int n = 0; n < 3; n++)//0,1,2일때 실행한다.
                    {
                        switch (colorUnitIndex % 3)//RGB에 값저장하기 위한것이다.
                        {
                            case 0://R
                                {
                                    charValue = charValue * 2 + pixel.R % 2;//다시 추출하기위해, 0으로 셋팅했던 LSB되돌리는 과정이다.
                                } break;//탈출
                            case 1://G
                                {
                                    charValue = charValue * 2 + pixel.G % 2;//다시 추출하기위해, 0으로 셋팅했던 LSB되돌리는 과정이다.
                                } break;//탈출
                            case 2://B
                                {
                                    charValue = charValue * 2 + pixel.B % 2;//다시 추출하기위해, 0으로 셋팅했던 LSB되돌리는 과정이다.
                                } break;//탈출
                        }

                        colorUnitIndex++;//증가시켜준다.

                        if (colorUnitIndex % 8 == 0)//8로 나눈값이 0일때 (8비트 한번 지날때)
                        {
                            charValue = reverseBits(charValue);//저장이 거꾸로되서 다시 원래형태로 돌려주기위한 과정이다.

                            if (charValue == 0)//0일때,
                            {
                                return extractedText;//추출된문자 반환해준다.
                            }
                            char c = (char)charValue;//변수의 형을 변환시켜주기위한 과정이다.

                            extractedText += c.ToString();//추출된문자에 ToString더해서 저장해준다.
                        }
                    }
                }
            }

            return extractedText;//추출된 문자 반환한다.
        }

        public static int reverseBits(int n)//저장 거꾸로 된것 원래대로 돌리기위한 함수이다.변수n
        {
            int result = 0;//int형 변수 0으로 선언해준다.

            for (int i = 0; i < 8; i++)//8비트마다 반복하기위해 8번수행함
            {
                result = result * 2 + n % 2;//result에 값 거꾸로 저장하기 위한 과정

                n /= 2;//다음비트로 이동한다.
            }

            return result;//result값 반환한다.
        }
    }
}
