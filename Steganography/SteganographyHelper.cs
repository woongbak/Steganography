using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding,  //숨기는 상태를 나타냄         
            Filling_With_Zeros  //텍스트를 다 숨기고 0비트를 채우는 상태를 나타냄
        };

        public static Bitmap embedText(string text, Bitmap bmp)   //Bitmap파일에 string을 인코딩하는 함수이다.
        {
            State state = State.Hiding;  //state변수를 Hiding상태로 만든다.

            int charIndex = 0;  //Text 파일의 어디 부분을 읽는지 확인하기 위한 charIndex 변수 선언 후 0으로 초기화 

            int charValue = 0;  //Text 파일의 읽는 부분의 아스키 코드값을 저장하는 변수 선언 후 0으로 초기화

            long pixelElementIndex = 0; //R, G, B 단위로 증가하는 pixelElementIndex 변수 선언 후 0으로 초기화 

            int zeros = 0;  //Text문자를 전부 인코딩 후 0을 몇 개 채웠는지 알 수 있는 zeros변수 선언 후 0으로 초기화

            int R = 0, G = 0, B = 0; //픽셀의 R, G, B값을 저장할 변수 선언 후 0으로 초기화

            for (int i = 0; i < bmp.Height; i++)  //비트맵의 높이까지 반복
            {
                for (int j = 0; j < bmp.Width; j++)  //비트맵의 넓이까지 반복
                {
                    Color pixel = bmp.GetPixel(j, i);  //pixel변수 선언 후, 해당 좌표의 픽셀을 얻는다.

                    R = pixel.R - pixel.R % 2;   //각각의 R G B의 LSB값을 0으로 만든다.
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)  //한 픽셀, 즉 R, G, B를 순회한다.
                    {
                        if (pixelElementIndex % 8 == 0)  //8 비트 한 글자를 모두 썼을 경우, 즉 새로운 글자의 시작부분
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)  //모든 텍스트를 다 인코딩 하고 0을 8개 입력했을 경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)   //텍스트와 zero 8개를 인코딩 후, 한 픽셀에 대해 B값을 순회하지 않아 SetPixel함수를 호출하지 못했을 경우
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));  //해당 좌표에 R, G, B값을 바탕으로 픽셀을 설정한다.
                                }

                                return bmp;  //비트맵 반환, 즉 종료
                            }

                            if (charIndex >= text.Length)  //모든 텍스트를 다 인코딩했을 경우
                            {
                                state = State.Filling_With_Zeros;  //state 변수를 Filling_With_Zeros상태로 바꿔준다.
                            }
                            else   //아닌 경우, 즉 남은 Text를 인코딩 해야할 경우
                            {
                                charValue = text[charIndex++];   //charValue값에 인코딩 해야할 Text의 아스키코드값을 대입하고, charIndex에 1을 더한다.
                            }
                        }

                        switch (pixelElementIndex % 3)  //한 픽셀에 R, G, B 에 대한 switch문
                        {
                            case 0:  //픽셀의 R부분일 경우
                                {
                                    if (state == State.Hiding)   //state가 Hiding 상태이면
                                    {
                                        R += charValue % 2;   //R의 LSB값에 charValue의 LSB값을 대입한다.
                                        charValue /= 2;   //charValue를 2로 나눈다.
                                    }
                                } break;
                            case 1:  //픽셀의 G부분일 경우
                                {
                                    if (state == State.Hiding)  //state가 Hiding 상태이면
                                    {
                                        G += charValue % 2;  //G의 LSB값에 charValue의 LSB값을 대입한다.

                                        charValue /= 2;  //charValue를 2로 나눈다.
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)  //state가 Hiding 상태이면
                                    {
                                        B += charValue % 2;  //B의 LSB값에 charValue의 LSB값을 대입한다.

                                        charValue /= 2;  //charValue를 2로 나눈다.
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));  //R, G, B 3개의 값에 인코딩될 텍스트의 3비트가 저장되 있으므로 SetPixel함수로 해당 좌표에 픽셀을 설정한다.
                                } break;
                        }

                        pixelElementIndex++;  //pixelElemnetIndex의 값을 1 증가시킨다.

                        if (state == State.Filling_With_Zeros)  //state가 Filling_With_Zeros 상태이면
                        {
                            zeros++;  //zeros의 값을 1 증가시킨다.
                        }
                    }
                }
            }

            return bmp;  //bmp을 반환한다, 즉 종료
        }

        public static string extractText(Bitmap bmp)  //비트맵 파일에서 숨겨진 텍스트를 추출하는 함수다.
        {
            int colorUnitIndex = 0;  //R, G, B 단위로 증가하는 colorUnitIndex 변수 선언 후 0으로 초기화
            int charValue = 0;  //추출한 텍스트의 아스키코드값을 나타낼 charValue 변수 선언 후 0으로 초기화 

            string extractedText = String.Empty;  //반환할 extractedText 변수를 선언 후 공백상태로 초기화

            for (int i = 0; i < bmp.Height; i++)  //비트맵 파일의 높이만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++)  //비트맵 파일의 높이만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i);  //pixel 변수 선언 후 해당좌표의 픽셀을 대입한다.
                    for (int n = 0; n < 3; n++)   //한 픽셀, 즉 R, G, B를 순회한다.
                    {
                        switch (colorUnitIndex % 3)  //한 픽셀에 R, G, B 에 대한 switch문
                        {
                            case 0:  //픽셀의 R부분일 경우
                                {
                                    charValue = charValue * 2 + pixel.R % 2;  //charValue를 왼쪽으로 한 번 쉬프트 한 후 pixel.R의 LSB값을 더한다.
                                } break;
                            case 1:  //픽셀의 G부분일 경우
                                {
                                    charValue = charValue * 2 + pixel.G % 2;  //charValue를 왼쪽으로 한 번 쉬프트 한 후 pixel.G의 LSB값을 더한다.
                                } break;
                            case 2:  //픽셀의 B부분일 경우
                                {
                                    charValue = charValue * 2 + pixel.B % 2;  //charValue를 왼쪽으로 한 번 쉬프트 한 후 pixel.B의 LSB값을 더한다.
                                } break;
                        }

                        colorUnitIndex++;  //colorUnitIndext 값을 1 증가시킨다.

                        if (colorUnitIndex % 8 == 0)   //8 비트 한 글자를 모두 읽었을 경우, 즉 새로 읽을 글자의 시작부분
                        {
                            charValue = reverseBits(charValue);  //인코딩한 내용이 원래 아스키코드값의 반전형태이므로 reverseBits함수를 이용해 반전시켜 charvalue에 대입한다.

                            if (charValue == 0)  //charValue가 0인 경우, 즉 모든 내용을 다 읽고 zero가 8개인 부분을 읽었을 경우
                            {
                                return extractedText;  //추출한 텍스트를 반환한다.
                            }
                            char c = (char)charValue;  //char형 변수 c를 선언하고 추출한 텍스트를 대입한다.

                            extractedText += c.ToString();  //c를 스트링으로 바꾼 후 extractedText에 추가한다.
                        }
                    }
                }
            }

            return extractedText;  //extractedText를 반환한다, 즉 종료
        }

        public static int reverseBits(int n)  //8비트의 값을 반전시키는 함수
        {
            int result = 0; //결과를 저장하는 변수 선언 후 0으로 초기화

            for (int i = 0; i < 8; i++)  //8번 반복
            {
                result = result * 2 + n % 2;  //result를 왼쪽으로 한 번 쉬프트 한 후 n의 LSB값을 더한다. 

                n /= 2; //n을 2로 나눈다.
            }

            return result;  //반전된 결과를 반환
        }
    }
}
