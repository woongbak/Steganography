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

        public static Bitmap embedText(string text, Bitmap bmp)         // 데이터를 숨기는 함수
        {
            State state = State.Hiding; // 상태를 '숨김'으로 설정

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            // 이미지의 사이즈 만큼의 모든 픽셀을 이중 반복문을 통해서 순회
            for (int i = 0; i < bmp.Height; i++)            // 이미지의 높이만큼 반복문 실행
            {
                for (int j = 0; j < bmp.Width; j++)         // 이미지의 너비많큰 반복문 실행
                {
                    Color pixel = bmp.GetPixel(j, i);       // 해당 픽셀을 얻어낸다.

                    // Color.R G B의 값을 이용해, 정수형 R, G, B에 새로운 값을 담는다.
                    // 이때 새로운 값은  [기존 RGB의 값] - [기존 RGB의 값]%2 의 값이다.
                    // 즉 RGB값을 2진수로 표현했을때, 맨 마지막 비트를, RGB가 짝수면 0, 홀수면 1을 빼서 0으로 맞춰주는 것이다.
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    // 한 픽셀의 R, G, B값을 반복적으로 확인해 주어진 조건에 만족하면 그 값을 바꾼다.
                    for (int n = 0; n < 3; n++)     // 한 픽셀의 R, G, B 값을 반복
                    {
                        if (pixelElementIndex % 8 == 0) // 조건 1
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)    // 조건 2
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)    // 조건3
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    // 조건 3까지 만족하면 픽셀의 정보를 set한다.
                                }

                                return bmp; // 위의 모든 조건이 만족하면 bmp 반환
                            }

                            if (charIndex >= text.Length)   // 조건 4, 조건 2를 만족하지 않으면 조건 4를 체크한다.
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else  //  조건 4를 만족하지 않으면 수행
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        // 픽셀요소인덱스를 3으로 나눈 나머지 값으로 switch문을 실행
                        switch (pixelElementIndex % 3)
                        {
                            case 0: // 나머지가 0이면
                                {
                                    if (state == State.Hiding)  // state가 Hiding 이라면 (함수 초반에 state를 Hiding으로 셋팅했다)
                                    {
                                        R += charValue % 2; // R의 값을 변경한다.
                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 1: // 나머지가 1이면
                                {
                                    if (state == State.Hiding) // state가 Hiding 이라면 (함수 초반에 state를 Hiding으로 셋팅했다)
                                    {
                                        G += charValue % 2; // G의 값을 변경한다.

                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if (state == State.Hiding) // state가 Hiding 이라면 (함수 초반에 state를 Hiding으로 셋팅했다)
                                    {
                                        B += charValue % 2; // B의 값을 변경한다.

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    // 변경된 RGB값으로 픽셀을 세팅한다.
                                }
                                break;
                        }

                        pixelElementIndex++;    

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp)       // 데이터를 추출하는 함수
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)    // 이미지의 높이 만큼 반복 수행
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지의 너비 만큼 반복 수행
                {
                    Color pixel = bmp.GetPixel(j, i);   // 이미지의 픽셀을 얻어온다.

                    
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) // 색개체인덱스/3 의 나머지로 swich문 실행
                        {
                            case 0: // 나머지가 0이면
                                {
                                    charValue = charValue * 2 + pixel.R % 2;    // R픽셀을 2로 나눈나머지를 charValue*2의 갑과 더한 후 charValue에 저장한다.
                                }
                                break;
                            case 1: // 나머지가 1이면
                                {
                                    charValue = charValue * 2 + pixel.G % 2;    // G픽셀을 2로 나눈나머지를 charValue*2의 갑과 더한 후 charValue에 저장한다.
                                }
                                break;
                            case 2: // 나머지가 2이면
                                {
                                    charValue = charValue * 2 + pixel.B % 2;    // B픽셀을 2로 나눈나머지를 charValue*2의 갑과 더한 후 charValue에 저장한다.
                                }
                                break;
                        }

                        colorUnitIndex++;   //색개체인덱스를 증가시킨다.

                        if (colorUnitIndex % 8 == 0)    // 색개체인덱스/8의 나머지가 0이면
                        {
                            charValue = reverseBits(charValue); 

                            if (charValue == 0) // charValue의 값이 0이면
                            {
                                return extractedText;   // extractedText 스트링을 반환한다.
                            }
                            char c = (char)charValue;   //charValue를 char형으로 형변환 해서 문자 하나를 얻어낸다.

                            extractedText += c.ToString();  // 그 문자를 extractedText에 추가한다.
                        }
                    }
                }
            }

            return extractedText;   // 모든 반복문이 종료되면 extractedText를 반환
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
