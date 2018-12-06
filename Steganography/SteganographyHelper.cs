using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding,     // Hiding에 0을 저장
            Filling_With_Zeros  // Filling_With_Zeros에 1을 저장
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        /*
            * <텍스트와 이미지를 입력받아서 실행시키는 함수를 선언>
            * image내에 steganogram을 삽입하는(숨기는) 모듈
            * 'text' means the string to be hidden.
            * 'bmp' means the image for the string to be hidden.
        */
        {
            State state = State.Hiding; // state에 0을 저장

            int charIndex = 0;          // 변수 선언

            int charValue = 0;          // 변수 선언

            long pixelElementIndex = 0; // 변수 선언

            int zeros = 0;              // 변수 선언

            int R = 0, G = 0, B = 0;    // 변수 선언

            for (int i = 0; i < bmp.Height; i++)    // Traversing the whole pixels(해당 파일의 높이만큼 반복문을 실행)
            {
                for (int j = 0; j < bmp.Width; j++) // 해당 파일의 너비만큼 반복문을 실행
                {
                    Color pixel = bmp.GetPixel(j, i);   // Extracting the data of each pixel(픽셀의 색 값을 가져옴)
                    // Substracting the value of the remains made by dividing the number 2 to make the LSB to zero(0)
                    R = pixel.R - pixel.R % 2; // 해당 픽셀의 빨간 값을 2로 나눈 값을 해당 픽셀의 빨간 값에서 뺴줌
                    G = pixel.G - pixel.G % 2; // 해당 픽셀의 초록 값을 2로 나눈 값을 해당 픽셀의 초록 값에서 뺴줌
                    B = pixel.B - pixel.B % 2; // 해당 픽셀의 파랑 값을 2로 나눈 값을 해당 픽셀의 파랑 값에서 뺴줌

                    for (int n = 0; n < 3; n++)  // Iterating 3 times to conceal the text(string) data within the whole R, G, B
                    {
                        if (pixelElementIndex % 8 == 0) // pixelElementIndex값이 0이면 실행
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // Filling_With_Zeros가 0이고, zeros가 8이면 실행
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // pixelElementIndex에서 1뺀 값을 3으로 나눴을때의 나머지가 2 미만이면 실행.
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // R G B값을 통해 픽셀에 색을 칠함
                                }// if ((pixelElementIndex - 1) % 3 < 2)

                                return bmp; // bmp를 반환한다.
                            }// if (state == State.Filling_With_Zeros && zeros == 8)

                            if (charIndex >= text.Length)   // 들어온 문자열보다 숨겨질 문자열이 크면 실행
                            {
                                state = State.Filling_With_Zeros; // state에 1을 저장
                            }// if (charIndex >= text.Length)
                            else                        // 아니라면
                            {
                                charValue = text[charIndex++]; // 숨겨질 문자열을 늘린다.
                            }// else
                        }// if (pixelElementIndex % 8 == 0)

                        switch (pixelElementIndex % 3) // pixelElementIndex를 3으로 나눈 나머지의 값이...
                        {
                            case 0: // 0이라면
                                {
                                    if (state == State.Hiding) // state가 0이라면 실행
                                    {
                                        R += charValue % 2; // 빨강 값에 숨겨질 문자열 값 % 2 값을 추가
                                        charValue /= 2; // 숨겨질 문자열 값을 반으로 줄임
                                    }// if (state == State.Hiding)
                                } break;
                            case 1: // 1이라면
                                {
                                    if (state == State.Hiding) // state가 0이라면 실행
                                    {
                                        G += charValue % 2; // 초록 값에 숨겨질 문자열 값 % 2 값을 추가

                                        charValue /= 2; // 숨겨질 문자열 값을 반으로 줄임
                                    }// if (state == State.Hiding)
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }// if (state == State.Hiding)

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }// switch (pixelElementIndex % 3)

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros) // state가 1이라면 실행
                        {
                            zeros++; // zeros에 1을 더해줌
                        }// if (state == State.Filling_With_Zeros)
                    }// for (int n = 0; n < 3; n++)
                }// for (int j = 0; j < bmp.Width; j++)
            }// for (int i = 0; i < bmp.Height; i++)

            return bmp; //bmp를 반환함
        }// public static Bitmap embedText(string text, Bitmap bmp)

        public static string extractText(Bitmap bmp) // 사진을 입력받아 내용을 추출해주는 함수 선언
        {
            int colorUnitIndex = 0; // 변수 선언
            int charValue = 0; // 변수 선언

            string extractedText = String.Empty; // 문자열을 초기화해줌

            for (int i = 0; i < bmp.Height; i++) // 사진의 높이만큼 반복해줌
            {
                for (int j = 0; j < bmp.Width; j++) // 사진의 너비만큼 반복해줌
                {
                    Color pixel = bmp.GetPixel(j, i); // i,j 픽셀의 정보를 가져옴
                    for (int n = 0; n < 3; n++) // 3번 실행한다.
                    {
                        switch (colorUnitIndex % 3) // 픽셀을 3으로 나눈 나머지가
                        {
                            case 0: // 0이라면
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // 문자열 값을 원래 값 * 2 + 현재 픽셀 값(빨강 기준)의 2로 나눈 나머지로 설정.
                                } break;
                            case 1: // 1이라면
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // 문자열 값을 원래 값 * 2 + 현재 픽셀 값(초록 기준)의 2로 나눈 나머지로 설정.
                                } break;
                            case 2: // 2라면
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // 문자열 값을 원래 값 * 2 + 현재 픽셀 값(파랑 기준)의 2로 나눈 나머지로 설정.
                                } break;
                        }

                        colorUnitIndex++; // colorUnitIndex에 1을 더해준다.

                        if (colorUnitIndex % 8 == 0) // 픽셀 색이 8로 나눈 나머지가 0이라면 실행
                        {
                            charValue = reverseBits(charValue); // 비트 변환을 해줌.

                            if (charValue == 0) // 숨겨진 문자열 값이 0이라면 실행.
                            {
                                return extractedText; // extractedText를 반환해줌.
                            }
                            char c = (char)charValue; // 문자를 저장해줌

                            extractedText += c.ToString(); // 추출된 문자열을 저장해줌.
                        }
                    }
                }
            }

            return extractedText; // extractedText를 반환해줌.
        }

        public static int reverseBits(int n) // n을 입력받아 비트변환 연산을 해주는 함수 선언
        {
            int result = 0; // 변수 선언

            for (int i = 0; i < 8; i++) // 8번 실행하는 반복문
            {
                result = result * 2 + n % 2;

                n /= 2;
            }// for (int i = 0; i < 8; i++)

            return result; // result를 반환해줌.
        }// public static int reverseBits(int n)
    }// class SteganographyHelper
}// namespace Steganography
