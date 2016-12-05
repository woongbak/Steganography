using System;
using System.Drawing;

namespace Steganography // Steganography namespace 선언
{
    class SteganographyHelper // SteganographyHelper 클래스 선언
    {
        public enum State // State 라는 enum 구조체 선언
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        // text를 사진에 숨기기위한 함수
        {
            State state = State.Hiding;
            // state 라는 State 구조체에 State.Hiding을 저장.
            int charIndex = 0;
            // charIndex라는 정수형 변수 선언
            int charValue = 0;
            // charValue라는 정수형 변수 선언
            long pixelElementIndex = 0;
            // pixelElementIndex 변수 선언
            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++) // bmp의 높이만큼 반복문 실행
            {
                for (int j = 0; j < bmp.Width; j++) // bmp의 너비만큼 반복문 실행
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀을 Get 한 후, Color pixel에 저장

                    R = pixel.R - pixel.R % 2; // R의 값을 0으로 만든다.
                    G = pixel.G - pixel.G % 2; // G의 값을 0으로 만든다.
                    B = pixel.B - pixel.B % 2; // B의 값을 0으로 만든다.

                    for (int n = 0; n < 3; n++) // 한 픽셀에 대한 반복분(RGB채널)
                    {
                        if (pixelElementIndex % 8 == 0) 
                        { // 픽셀 요소 인덱스/8 의 나머지가 0인 경우
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            { // state가 State 구조체의 Filling_With_Zeors 이고, zeros가 8인 경우
                                if ((pixelElementIndex - 1) % 3 < 2)
                                { // pixelElementInde-1 의 3으로 나눈 나머지가 2보다 작은 경우
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                    // 픽셀에 대한 정보 세팅
                                }

                                return bmp; // 위의 모든 조건이 해당되면 bmp를 반환.
                            }

                            if (charIndex >= text.Length) // charIndex가 text.Length 보다 크거나 같은 경우
                            {
                                state = State.Filling_With_Zeros;
                                // state에 State 구조체의 Filling_With_Zeros 저장. 모두 0으로 만들어줌.
                            }
                            else // charIndex가 text.Length보다 크거나 같이 않은 경우
                            {
                                charValue = text[charIndex++]; 
                                // charValue를 text배열의 charIndex 위치에 저장해주고 charIndex값 1 증가
                            }
                        }

                        switch (pixelElementIndex % 3)
                        { // 픽셀 요소 인덱스를 3으로 나눈 나머지를 통해 switch문 실행
                            case 0: // 나머지가 0인 경우
                                {
                                    if (state == State.Hiding) // state가 Hiding이라면
                                    {
                                        R += charValue % 2; // R = R + (charValue를 2로 나눈 나머지)
                                        charValue /= 2; // charValue = charValue를 2로 나눈 몫
                                    }
                                } break;
                            case 1: // 나머지가 1인 경우
                                {
                                    if (state == State.Hiding) // state가 Hiding이라면
                                    {
                                        G += charValue % 2; // G = G + (charValue를 2로 나눈 나머지)

                                        charValue /= 2; // charValue = charValue를 2로 나눈 몫
                                    }
                                } break;
                            case 2: // 나머지가 2인 경우
                                {
                                    if (state == State.Hiding) // state가 Hiding이라면
                                    {
                                        B += charValue % 2; // B = B + (charValue를 2로 나눈 나머지)

                                        charValue /= 2; // charValue = charValue를 2로 나눈 몫
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 세팅된 RGB값을 통해 pixel 세팅 
                                } break;
                        }

                        pixelElementIndex++; // 픽셀 요소 인덱스 1증가

                        if (state == State.Filling_With_Zeros) // state가 0으로 세팅되어 있는 경우
                        {
                            zeros++; // 0의 개수를 세기 위해 zeros 1씩 증가
                        }
                    }
                }
            }

            return bmp; // bmp 반환
        }

        public static string extractText(Bitmap bmp) // 데이터 추출을 위한 함수
        {
            int colorUnitIndex = 0; 
            int charValue = 0;

            string extractedText = String.Empty; // 추출한 text를 저장할 문자열 변수를 비어있는 상태로 세팅

            for (int i = 0; i < bmp.Height; i++) // 각각의 픽셀에 대한 접근을 위한 반복문(높이)
            { 
                for (int j = 0; j < bmp.Width; j++) // 각각의 픽셀에 대한 접근을 위한 반복문(너비)
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀 가져오기
                    for (int n = 0; n < 3; n++) // RGB 채널에 대한 반복문
                    {
                        switch (colorUnitIndex % 3) // colorUnitIndex를 3으로 나눈 나머지를 통한 switch문
                        {
                            case 0: // 나머지가 0인 경우
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                    // charValue = charValue*2 + (R을 2로 나눈 나머지) 
                                } break;
                            case 1: // 나머지가 1인 경우
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                    // charValue = charValue*2 + (G를 2로 나눈 나머지)
                                } break;
                            case 2: // 나머지가 2인 경우
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                    // charValue = charValue*2 + (B를 2로 나눈 나머지)
                                } break;
                        }

                        colorUnitIndex++; // colorUnitIndex 1 증가

                        if (colorUnitIndex % 8 == 0) 
                        { // colorUnitIndex를 8로 나눈 나머지가 0 일 때 => 하나의 문자를 읽었을 때
                            charValue = reverseBits(charValue);
                            // charValue 에 reverseBits(charValue) 값 저장
                            if (charValue == 0) // charValue가 0이라면 => 데이터의 끝 부분 탐색
                            {
                                return extractedText; // 추출한 text 반환
                            }
                            char c = (char)charValue; // charValue를 c에 저장(추출된 문자)

                            extractedText += c.ToString(); // 추출된 문자를 text로 저장.
                        }
                    }
                }
            }

            return extractedText; // 추출된 text 반환
        }

        public static int reverseBits(int n) // 데이터 추출 함수에서 사용되는 함수
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
