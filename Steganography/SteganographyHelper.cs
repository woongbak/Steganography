using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State // 구조체로 현재 상태를 나타내기 위해 정의한다.
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp) // 이것이 원하는 텍스트를 숨기는 메소드이다.
        {
            State state = State.Hiding; // 현재 상태를 숨기고 있는 상태로 나타낸다.

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0; // 여기까지 초기화 과정이다.

            for (int i = 0; i < bmp.Height; i++) // 변수 i를 1씩 증가시키며 bmp의 높이 만큼 반복하는 반복문이다.
            {
                for (int j = 0; j < bmp.Width; j++) // 위와 동일하되 변수는 j이고, bmp의 너비이다. 위의 반복문의 안에 존재하므로 결론적으로 bmp의 모든 픽셀을 들리게 된다.
                {
                    Color pixel = bmp.GetPixel(j, i); // GetPixel로 bmp의 현재 픽셀의 컬러 값을 받는다.

                    R = pixel.R - pixel.R % 2; // R의 값을 pixel의 R 값의 연산을 통해 설정.
                    G = pixel.G - pixel.G % 2; // G의 값을 pixel의 G 값의 연산을 통해 설정
                    B = pixel.B - pixel.B % 2; // B의 값을 pixel의 B 값의 연산을 통해 설정

                    for (int n = 0; n < 3; n++) // n의 값이 내부에서 변하지 않으면 3번 수행한다.
                    {
                        if (pixelElementIndex % 8 == 0) // pixelElementIndex를 8으로 나눈 나머지가 0일 경우, 즉 값이 8의 배수일 경우,
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 현재 상태가 0으로 채우기 상태고, zeros의 값이 8일 경우,
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // (pixelElementIndex - 1)를 3으로 나눈 나머지가 1 이하일 경우,
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // bmp 픽셀의 정보를 위치는 j, i를 통해, 색 정보는 Color.FromArgb(R, G, B) 값으로 설정한다.
                                }

                                return bmp; // bmp 반환
                            }

                            if (charIndex >= text.Length) // charIndex가 text.Length 이상일 경우,
                            {
                                state = State.Filling_With_Zeros; // 현재 상태를 0으로 채우기로 바꾼다.
                            }
                            else
                            {
                                charValue = text[charIndex++]; // charValue의 값을 text[charIndex++] 값으로 설정한다.
                            }
                        }

                        switch (pixelElementIndex % 3) // pixelElementIndex를 3으로 나눈 나머지에 따라 분기,
                        {
                            case 0: // 위의 값이 0일 경우,
                                {
                                    if (state == State.Hiding) // 현재 상태가 숨기기 상태일 경우,
                                    {
                                        R += charValue % 2; // R에 charValue를 2로 나눈 나머지을 더한다.
                                        charValue /= 2; // charValue의 값을 2로 나눈 몫으로 설정한다.
                                    }
                                } break;
                            case 1: // 위의 값이 1일 경우,
                                {
                                    if (state == State.Hiding) // 현재 상태가 숨기기 상태일 경우,
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: // 위의 값이 2일 경우,
                                {
                                    if (state == State.Hiding) // 현재 상태가 숨기기 상태일 경우,
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // bmp 픽셀의 정보를 위치는 j, i를 통해, 색 정보는 Color.FromArgb(R, G, B) 값으로 설정한다.
                                } break;
                        }

                        pixelElementIndex++; // pixelElementIndex의 값을 1 증가시킨다.

                        if (state == State.Filling_With_Zeros) // 현재 상태가 0으로 채우기 상태일 경우
                        {
                            zeros++; // zeros의 값을 1 증가시킨다.
                        }
                    }
                }
            }

            return bmp; // bmp를 반환한다.
        }

        public static string extractText(Bitmap bmp) // 위가 숨기는 메소드였다면, 이것은 추출하는 메소드이다.
		
            int colorUnitIndex = 0; // 초기화
            int charValue = 0; // 초기화

            string extractedText = String.Empty; // 추출해낸 텍스트를 저장한 문자열 선언

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++) // 여기까지는 위의 메소드와 동일하게 이미지 전체의 각 픽셀을 들리기 위한 반복문이다.
                    {
                        switch (colorUnitIndex % 3) // colorUnitIndex를 3으로 나눈 나머지 값에 따라 분기,
                        {
                            case 0: // 나머지가 0일 경우
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // charValue의 값은 charValue * 2 + pixel.R % 2의 값이 된다.
                                } break;
                            case 1: // 나머지가 1일 경우
                                {
                                    charValue = charValue * 2 + pixel.G % 2; 
                                } break;
                            case 2: // 나머지가 2일 경우
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++; // colorUnitIndex의 값을 1 증가시킨다.

                        if (colorUnitIndex % 8 == 0) // colorUnitIndex를 8으로 나눈 나머지가 0, 즉 8의 배수일 경우,
                        {
                            charValue = reverseBits(charValue); // charValue는 reverseBits 메소드에 스스로를 집어넣은 결과 값과 같다.

                            if (charValue == 0) // charValue의 값이 0일 경우,
                            {
                                return extractedText; // 추출된 문자열을 반환한다.
                            }
                            char c = (char)charValue; // 문자 c의 값은 charValue이다.

                            extractedText += c.ToString(); // 추출된 문자열에 문자 c를 문자열로서 더한다.
                        }
                    }
                }
            }

            return extractedText; // 추출된 문자열을 반환한다.
        }

        public static int reverseBits(int n) // 추출 메소드에서 사용되는 내부 메소드이다.
        {
            int result = 0; // 초기화

            for (int i = 0; i < 8; i++) // i의 값에 따른 반복문, 내부에서 i의 값의 변동이 없다면, 8번 수행한다.
            {
                result = result * 2 + n % 2; // result의 값은 result * 2 + n % 2 이다.

                n /= 2; // n은 자신을 2로 나눈 몫의 값을 가진다.
            }

            return result; // result를 반환한다.
        } 
    }
}
