using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding, // enum 의 첫번째 요소 0
            Filling_With_Zeros // 두번째 요소 1
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {// 이미지에 텍스트를 숨기는 함수 텍스트와 이미지를 인자로 입력받음  
            State state = State.Hiding; // state변수 선언및 0으로 초기화

            int charIndex = 0; // 숨기는 메세지 인덱스 변수 선언및 초기화

            int charValue = 0; // 숨기는 메세지 값 변수 선언및 초기화

            long pixelElementIndex = 0; // 이미지 픽셀의 인덱스 변수 선언및 초기화

            int zeros = 0; // 메세지 숨길때 사용하는 픽셀 값 변수 선언및 초기화

            int R = 0, G = 0, B = 0; // 색 담을 R,G,B변수 선언및 초기화

            for (int i = 0; i < bmp.Height; i++) // 이미지 높이(세로)만큼 도는 반복문
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지 너비(가로)만큼 도는 반복문
                {
                    Color pixel = bmp.GetPixel(j, i); // 해당 픽셀의 색 값을 가져옴

                    R = pixel.R - pixel.R % 2; 
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    // pixel. R,G,B값을 이용해 R,G,B의 값을 0으로 만듬
                    for (int n = 0; n < 3; n++) // 
                    {
                        if (pixelElementIndex % 8 == 0) // pixelElementIndex % 8의 나머지가 0일때
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // state가 1 이고 zeros가 8일때 
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // 연산결과가 2보다 작을때
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 만족시 해당픽셀에 R,G,B 를 통해 색을 칠함  
                                }

                                return bmp; // 이미지를 반환
                            }

                            if (charIndex >= text.Length) // 숨기는 메세지 인덱스 값이 텍스트의 길이보다 크거나 같으면
                            {
                                state = State.Filling_With_Zeros; // state를 1로 바꿔줌, 숨길게 없는 상태
                            }
                            else // 위의 조건문이 만족하지 않을시
                            {
                                charValue = text[charIndex++]; // 숨길 문자를 대입
                            }
                        }

                        switch (pixelElementIndex % 3) // pixelElementIndex 의 3으로 나눈나머지 값이
                        {
                            case 0: // 0 이면 R
                                {
                                    if (state == State.Hiding) // 숨길게 있는 상태이면 
                                    {
                                        R += charValue % 2; 
                                        charValue /= 2; // R의 값을 변경 및 charValue을 반으로 줄임
                                    }
                                } break;
                            case 1:// 1 이면 G
                                {
                                    if (state == State.Hiding)  // 숨길게 있는 상태이면
                                    {
                                        G += charValue % 2;

                                        charValue /= 2; // G의 값을 변경 및 charValue을 반으로 줄임
                                    }
                                } break;
                            case 2:// 2 이면 B
                                {
                                    if (state == State.Hiding) // 숨길게 있는 상태이면
                                    {
                                        B += charValue % 2;

                                        charValue /= 2; // B의 값을 변경 및 charValue을 반으로 줄임
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 해당픽셀에 R, G, B 를 통해 색을 칠함
                                } break;
                        }

                        pixelElementIndex++; // pixelElementIndex 1증가

                        if (state == State.Filling_With_Zeros) // 숨길게 없다면
                        {
                            zeros++; // zeros 1증가
                        }
                    }
                }
            }

            return bmp; // 이미지 반환
        }

        public static string extractText(Bitmap bmp) // 추출하기 위한 함수 인자로 이미지를 받음
        {
            int colorUnitIndex = 0; // 색 인덱스 변수 선언및 초기화
            int charValue = 0; // 숨기는 메세지 값 변수 선언및 초기화

            string extractedText = String.Empty; // 문자열 저장 변수 선언

            for (int i = 0; i < bmp.Height; i++) // 이미지 높이(세로)만큼 도는 반복문
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지 너비(가로)만큼 도는 반복문
                {
                    Color pixel = bmp.GetPixel(j, i);// 해당 픽셀의 색 값을 가져옴
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) // colorUnitIndex % 3의 나머지
                        {
                            case 0: // 0 이면 R
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // 문자값을 1시프트 , 숨긴 문자 삽입
                                } break;
                            case 1: // 1 이면 G
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // 문자값을 1시프트 , 숨긴 문자 삽입
                                } break;
                            case 2: // 2 이면 B
                                { 
                                    charValue = charValue * 2 + pixel.B % 2; // 문자값을 1시프트 , 숨긴 문자 삽입
                                } break;
                        }

                        colorUnitIndex++; // 색 인덱스 1증가

                        if (colorUnitIndex % 8 == 0) // %8 시 나머지가 0이면
                        {
                            charValue = reverseBits(charValue); // 비트를 뒤집기

                            if (charValue == 0) // charValue 0 이면
                            {
                                return extractedText; // 문자열 반환
                            }
                            char c = (char)charValue; // char형으로 바꿔서 c변수에 넣어줌

                            extractedText += c.ToString(); // 문자열에 c를 스트링으로 변화한 것을 더해줌
                        }
                    }
                }
            }

            return extractedText; // 문자열 반환
        }

        public static int reverseBits(int n) // 정수n을받아 비트를 뒤집어주는 함수
        {
            int result = 0;

            for (int i = 0; i < 8; i++) // 1바이트 반복 
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result; // 결과 반환
        }
    }
}
