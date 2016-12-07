using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State //enum을 이용해 state 안의 값에 숫자를 선언
        {
            Hiding, // 0
            Filling_With_Zeros // 1
        };

        public static Bitmap embedText(string text, Bitmap bmp) // 텍스트를 숨기기위한 함수으로, bitmap 클래스를 이용해 이미지를 이용할수 있도록함
        {
            State state = State.Hiding; // 열거형 타입의 변수 state를 선언하고 hiding을 할당. 0값이 state에 들어감

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0; // 빨강 초록 파랑 값을 0으로 초기화

            for (int i = 0; i < bmp.Height; i++) // 루프문을 i=0부터 bmp의 높이 까지 돌림
            {
                for (int j = 0; j < bmp.Width; j++) // 루프문을 j =0부터 bmp의 넓이까지 돌림
                {
                    Color pixel = bmp.GetPixel(j, i); // 컬러 구조체를 이용해 pixel을 선언, bmp의 j,i좌표의 픽셀을 받아옴

                    R = pixel.R - pixel.R % 2; // 빨강 값에서 빨강 값을 2로나눈 나머지값을 뺴서 R에 넣음
                    G = pixel.G - pixel.G % 2; // 초록 값에서 초록 값을 2로나눈 나머지값을 뺴서 R에 넣음
                    B = pixel.B - pixel.B % 2; // 파랑 값에서 파랑 값을 2로나눈 나머지값을 뺴서 R에 넣음

                    for (int n = 0; n < 3; n++) // 루프를 3번 돌돌림
                    {
                        if (pixelElementIndex % 8 == 0) // pixelElementIndex변수가 8로나누어 떨어지면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // state변수가 1(enum에 의해 오른쪽 값 1가리킴) 이고 0의 개수가 8이면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // pixelElementIndex변수에서 -1한 값에서 3을 나눈 나머지가 2보다 작다면,
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // bmp의 j,i 좌표의 픽셀 부분을 위에서 받은 R G B 값으로 설정                                }

                                    return bmp; // bmp를 반환
                                }
                            }
                            if (charIndex >= text.Length) // charIndex 변수가 text의 길이보다 크거나 같다면
                            {
                                state = State.Filling_With_Zeros; // state 변수를 1로 만듬
                            }
                            else // charIndex 변수가 text의 길이보다 작다면
                            {
                                charValue = text[charIndex++]; // text의 인덱스가 가리키는 값을 charValue에 넣고, 인덱스를 1증가
                            }
                        }

                        switch (pixelElementIndex % 3) // pixelElementIndex % 3 연산으로
                        {
                            case 0: // 나머지가 0이면
                                {
                                    if (state == State.Hiding) // state 변수를 0으로 만듬
                                    {
                                        R += charValue % 2; // 빨강 값에 charValue를 2로나눈 나머지를 더해줌
                                        charValue /= 2; // charvalue에 자신을 2로 나눈 값을 넣음 
                                    }
                                } break;
                            case 1: // 나머지가 1이면
                                {
                                    if (state == State.Hiding) // state 변수를 0으로 만듬
                                    {
                                        G += charValue % 2; // 초록 값에 charValue를 2로나눈 나머지를 더해줌

                                        charValue /= 2; // charvalue에 자신을 2로 나눈 값을 넣음 
                                    }
                                } break;
                            case 2: // 나머지가 2면
                                {
                                    if (state == State.Hiding) // state 변수를 0으로 만듬
                                    {
                                        B += charValue % 2; // 파랑 값에 charValue를 2로나눈 나머지를 더해줌

                                        charValue /= 2; // charvalue에 자신을 2로 나눈 값을 넣음 
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // bmp의 j.i좌표의 픽셀에 위의 R G B 색깔값을 넣음
                                } break;
                        }

                        pixelElementIndex++; // pixelElementIndex 1증가

                        if (state == State.Filling_With_Zeros) // state 변수가 1이면
                        {
                            zeros++; // zeros를 1더함
                        }
                    }
                }
            }

            return bmp; // bmp를 반환
        }

        public static string extractText(Bitmap bmp) // 텍스트를 추출하는 함수 bmp를 인자로 받음
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; // 비어있는 extractedText의 string을 초기화

            for (int i = 0; i < bmp.Height; i++) // 0부터 bmp의 높이까지 루프를 돌림
            {
                for (int j = 0; j < bmp.Width; j++) // 0부터 bmp의 넓이까지 루프를 돌림
                {
                    Color pixel = bmp.GetPixel(j, i); // bmp의 j.i 좌표의 픽셀을 받아와 pixel을 color선언으로 ARGB를 나타내도록함
                    for (int n = 0; n < 3; n++) // 루프를 3번 돌림
                    {
                        switch (colorUnitIndex % 3) // colorUnitIndex를 3으로 나눈 나머지
                        {
                            case 0: // 0이면
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // 픽셀의 빨강 값을 2로 나눈 나머지와 charvalue에 2를 곱한 값을 더해 charValue에 넣음
                                } break;
                            case 1: // 1이면
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // 픽셀의 초록 값을 2로 나눈 나머지와 charvalue에 2를 곱한 값을 더해 charValue에 넣음
                                } break;
                            case 2: // 2라면
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // 픽셀의 파랑 값을 2로 나눈 나머지와 charvalue에 2를 곱한 값을 더해 charValue에 넣음
                                } break;
                        }

                        colorUnitIndex++; // colorUnitIndex 1 증가시킴

                        if (colorUnitIndex % 8 == 0) //colorUnitIndex가 8로 나누어 떨어진다면,
                        {
                            charValue = reverseBits(charValue); // reversBits를 이용해 charvalue에 넣음 (밑에 함수설명에서 다룸)

                            if (charValue == 0) // charValue가 0이면
                            {
                                return extractedText; // extractedText를 반환
                            }
                            char c = (char)charValue; // charvalue를 char형으로 c에 넣음

                            extractedText += c.ToString(); // 숫자서식 문자를 제공, extractedText에 값을 더해줌
                        }
                    }
                }
            }

            return extractedText; // extractedText 반환
        }

        public static int reverseBits(int n) // n의 비트를 거꾸로 뒤집는 함수
        {
            int result = 0;

            for (int i = 0; i < 8; i++) // 0부터 7까지 루프를 돌림
            {
                result = result * 2 + n % 2; // 인자로 받은값을 2로나눈 나머지와 결과값에 2를 곱한값을 result에 넣음

                n /= 2; // n을 2로 나눠서 n에 넣음
            }

            return result; // result를 반환
        }
    }
}
