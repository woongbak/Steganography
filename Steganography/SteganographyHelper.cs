using System;
using System.Drawing;

namespace Steganography                                           //steganography라고 이름을 붙인다
{
    class SteganographyHelper                                     //steganographyhelper 클래스 생성
    {
        public enum State                                         //열거형 변수 State 생성 접근형:public
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp)   //text를 숨기는 함수 입력받는값(문자열,사진)
        {                                                         //리턴값도 사진
            State state = State.Hiding;                           //state값을 state의hiding으로 변경

            int charIndex = 0;                                    //charindex 초기화

            int charValue = 0;                                    //charvalue 초기화

            long pixelElementIndex = 0;                           //pixelElementindex 초기화

            int zeros = 0;                                        //zeros 초기화

            int R = 0, G = 0, B = 0;                              //RGB 초기화

            for (int i = 0; i < bmp.Height; i++)                  //반복시작:
            {
                for (int j = 0; j < bmp.Width; j++)               //    반복시작:
                {
                    Color pixel = bmp.GetPixel(j, i);             //        pixel은 color변수형이고 픽셀의위치를이용해 값을 가져온다.

                    R = pixel.R - pixel.R % 2;                    //        pixel위치의 R값을 가져와서 2로 나눈 나머지값을 뺀다.
                    G = pixel.G - pixel.G % 2;                    //        pixel위치의 G값을 가져와서 2로 나눈 나머지값을 뺀다.
                    B = pixel.B - pixel.B % 2;                    //        pixel위치의 B값을 가져와서 2로 나눈 나머지값을 뺀다.
                                                                  //        나머지가 전부 0이 된다.(가장 낮은 비트가 0이 됨)
                    for (int n = 0; n < 3; n++)                   //            반복시작:
                    {
                        if (pixelElementIndex % 8 == 0)           //                pixelElementIndex을 8로 나눈 나머지가 0이면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//  state가 0으로 채워져있는상태고,zero변수가8이면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)            //  pixelElement-1을 3으로 나눈 나머지가 2보다 작으면
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//  pixel값을 변경한다. 점찍기
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length)                       //  텍스트의길이보다 charindex가 길면(문자열을 다숨기면)
                            {
                                state = State.Filling_With_Zeros;               //  state값을 변경한다.
                            }
                            else
                            {
                                charValue = text[charIndex++];                  //  else  텍스트를 가져온다. 다음값으로 변경
                            }
                        }

                        switch (pixelElementIndex % 3)                          //  pixelElementIndex에 따라서 다르게 수행
                        {
                            case 0:                                             //  0일경우
                                {
                                    if (state == State.Hiding)                  //  state를 hiding이면 (숨기는작업중)
                                    {
                                        R += charValue % 2;                     //  R값을 문자열의ascii값을 2로 나눈 나머지를 더한다.
                                        charValue /= 2;                         //  charvalue = charvalue/2 ->charvalue를 2진법으로 나타냈을때 한자리를 변경
                                    }
                                } break;
                            case 1:                                             //  case 1인 경우
                                {
                                    if (state == State.Hiding)                  //  숨길수있는상태이면
                                    {
                                        G += charValue % 2;                     //  G의 값을 위와 같이 변경

                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;                     //  B의 값을 위와 같이 변경

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//  변경한 값을 점으로 찍는다.
                                } break;
                        }

                        pixelElementIndex++;                                    //  pixelElementindex를 증가시킨다. R G B 순서로간다.

                        if (state == State.Filling_With_Zeros)                  //  한 픽셀이 끝나면
                        {
                            zeros++;                                            //  zeros를 증가시킨다. 
                        }
                    }                                               //  반복종료
                }                                             //    반복종료
            }                                       

            return bmp;
        }

        public static string extractText(Bitmap bmp)         //문자열을 추출하는 함수.
        {
            int colorUnitIndex = 0;                          //colorUnitIndex 초기화
            int charValue = 0;                               //charValue 초기화

            string extractedText = String.Empty;             //추출된텍스트를 저장할 스트링 초기화

            for (int i = 0; i < bmp.Height; i++)             //그림파일의 세로크기
            {
                for (int j = 0; j < bmp.Width; j++)          //그림파일의 가로크기
                {
                    Color pixel = bmp.GetPixel(j, i);        //픽셀의 색을 가져오는 변수
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)          //colorunitindex 0=R 1=G 2=B
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //픽셀의 R문자값을 정의 
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2; //픽셀의 G문자값을 정의 
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2; //픽셀의 B문자값을 정의 
                                } break;
                        }

                        colorUnitIndex++;                                   //다음 인덱스값으로 증가

                        if (colorUnitIndex % 8 == 0)                        //colorindex가 픽셀의 처음값을 가르킬때
                        {
                            charValue = reverseBits(charValue);             //ascii로 추출

                            if (charValue == 0)                             //문자열이 없으면
                            {
                                return extractedText;                       //추출된 텍스트를 리턴
                            }
                            char c = (char)charValue;                       //c는 ascii값을 char로 지정

                            extractedText += c.ToString();                  //extractedText에 저장
                        }
                    }
                }
            }

            return extractedText;                                           //그림파일의 픽셀을 전부 읽으면 문자열을 리턴
        }

        public static int reverseBits(int n)                                //한 픽셀에 숨어있는 문자열을 숫자로 추출
        {
            int result = 0;                                                 //결과초기화

            for (int i = 0; i < 8; i++)                                     //1픽셀은 8비트
            {
                result = result * 2 + n % 2;                                //result값을 숫자로 계산 높은자리의 비트부터 구할수있다.

                n /= 2;                                                     //8비트의 문자를 계산하는데 n을 2로 나누면 다음 비트를 구할수있다.
            }

            return result;
        }
    }
}
