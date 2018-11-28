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

        public static Bitmap embedText(string text, Bitmap bmp) // 이미지에 메시지를 숨기는 메소드 (text = 메시지 , bmp = 이미지)
        {
            State state = State.Hiding;

            int charIndex = 0; // 숨기는 메시지 인덱스

            int charValue = 0; // 숨기는 메시지 값 

            long pixelElementIndex = 0; // 이미지 pixel의 index값 

            int zeros = 0; // 메시지 숨길 때 사용할 픽셀 값 저장하는 변수

            int R = 0, G = 0, B = 0; // pixel의 red, green ,blue 값 

            for (int i = 0; i < bmp.Height; i++) // 이미지 파일 height 부분 탐색
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지 파일의 width 부분 탐색
                {
                    Color pixel = bmp.GetPixel(j, i); //  이미지의 (i,j) 부분의 픽셀을 가져옴

                    // 가져온 pixel의 RGB 영역의 LSB 값을 0으로 만들기 위한 부분
                    R = pixel.R - pixel.R % 2; 
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0) // pixelElementIndext가 0이거나 8로 나누어 떨어지면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // pixelElmentIndex에 -1 해준 값을 3으로 나누었을 때 2보다 작으면
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 픽셀 값을 RGB 값으로 셋팅
                                }

                                return bmp; // 이미지 반환
                            }

                            if (charIndex >= text.Length) // 숨기려는 메시지의 index 값이 text의 길이보다 크거나 같으면
                            {
                                state = State.Filling_With_Zeros; // state를 Filling_with_zeros 상태로 바꿔줌
                            }
                            else // if문을 만족하지 않으면
                            {
                                charValue = text[charIndex++]; // 숨기려는 메시지의 값에 text의 charIndex 요소에 1을 더한 값을 나타내는 부분을 넣어줌  
                            }
                        }

                        switch (pixelElementIndex % 3) // 이미지 pixel의 index값을 3으로 나눈 나머지 값으로 switch문 돌림
                        {
                            case 0: // R인 경우
                                {
                                    if (state == State.Hiding) // state가 Hiding 상태라면
                                    {
                                        R += charValue % 2; // R에 charValue를 2로 나눈 나머지 값을 더해줌
                                        charValue /= 2; // CharValue를 2로 나눈 몫으로 저장
                                    }
                                } break;
                            case 1: // G인 경우
                                {
                                    if (state == State.Hiding)  // state가 Hiding 상태라면
                                    {
                                        G += charValue % 2; // G에 charValue를 2로 나눈 나머지 값을 더해줌
                                        charValue /= 2; // CharValue를 2로 나눈 몫으로 저장
                                    }
                                } break;
                            case 2: // B인 경우
                                {
                                    if (state == State.Hiding) // state가 Hiding 상태라면
                                    {
                                        B += charValue % 2; // G에 charValue를 2로 나눈 나머지 값을 더해줌
                                        charValue /= 2; // CharValue를 2로 나눈 몫으로 저장
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 픽셀 값을 RGB 값으로 세팅
                                } break;
                        }

                        pixelElementIndex++; // 이미지 pixel index 값 증가

                        if (state == State.Filling_With_Zeros) // state가 Filling_with_zeros 상태라면
                        {
                            zeros++; // zeros 증가
                        }
                    }
                }
            }

            return bmp; // 이미지 반환해줌
        }

        public static string extractText(Bitmap bmp) // 숨긴 메시지를 추출하는 메소드
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; // 메시지 저장 변수

            for (int i = 0; i < bmp.Height; i++) // 이미지의 Height까지 탐색
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지의 Width까지 탐색
                {
                    Color pixel = bmp.GetPixel(j, i); // (i,j)의 pixel값 가져옴
                    for (int n = 0; n < 3; n++) 
                    {
                        switch (colorUnitIndex % 3) // colorUnitIndex를 3으로 나눈 나머지가
                        {
                            case 0: // R일 경우 
                                {
                                    // charValue 값을 1 시프트하고, 숨긴 문자를 삽입해줌 
                                    charValue = charValue * 2 + pixel.R % 2; 
                                } break;
                            case 1: // G일 경우
                                {
                                    // charValue 값을 1 시프트하고, 숨긴 문자를 삽입해줌 
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2: // B인 경우
                                {
                                    // charValue 값을 1 시프트하고, 숨긴 문자를 삽입해줌 
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++; // colorUnitIndex 1 증가

                        if (colorUnitIndex % 8 == 0) // colorUnitIndex가 0이거나 8로 나눠 떨어지면
                        {
                            charValue = reverseBits(charValue); // 비트를 뒤집어서 charValue 값에 저장

                            if (charValue == 0) // charValue가 0이면
                            {
                                return extractedText; // 추출한 메시지 반환
                            }
                            char c = (char)charValue; // c에 charValue 값을 char형으로 변환하여 저장

                            extractedText += c.ToString(); // 추출된 메시지에 c를 더해줌
                        }
                    }
                }
            }

            return extractedText; // 추출한 메시지 반환
        }

        public static int reverseBits(int n) // 비트를 뒤집어주는 메소드
        {
            int result = 0; // 결과 저장하는 정수형 변수

            for (int i = 0; i < 8; i++) // 0~8비트까지 탐색하며 비트를 뒤집는 반복문
            {
                result = result * 2 + n % 2; 

                n /= 2;
            }

            return result; // 결과값 반환
        }
    }
}
