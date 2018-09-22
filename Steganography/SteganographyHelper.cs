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

        public static Bitmap embedText(string text, Bitmap bmp) // 이미지에 텍스트 넣기 함수
        {
            State state = State.Hiding; // 현재 상태 : 숨기기

            int charIndex = 0; 

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++) // 세로픽셀수만큼 반복 ( 다음픽셀로 )
            {
                for (int j = 0; j < bmp.Width; j++) // 가로픽셀수만큼 반복 ( 다음픽셀로 )
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀정보 가져오기
                    
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    
                    for (int n = 0; n < 3; n++) // 한 픽셀 당 R G B 총 세칸이므로 3회 반복
                    {
                        if (pixelElementIndex % 8 == 0) // 하나의 글자가 끝나면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 모든텍스트가 끝났다면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // 스위치 문에서 못한 작업(픽셀설정)이라면
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 픽셀색에 텍스트 삽입
                                }

                                return bmp; // 변경된 이미지 반환
                            }

                            if (charIndex >= text.Length) // 작업할 문자 인덱스가 문자열인덱스보다 크다면
                            {
                                state = State.Filling_With_Zeros; // 작업종료 상태로 변경
                            }
                            else
                            {
                                charValue = text[charIndex++]; // charValue에 텍스트의 아스키코드값 대입
                            }
                        }

                        switch (pixelElementIndex % 3) // RGB 값 변경
                        {
                            case 0:
                                {
                                    if (state == State.Hiding) // R
                                    {
                                        R += charValue % 2; // 현재 charValue의 이진수 기준 가장 오른쪽글자를 R에 더함
                                        charValue /= 2; // 아스키 코드의 나머지 이진수 데이터를 넣기 위해 반으로나눔
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding) // G
                                    {
                                        G += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding) //B
                                    {
                                        B += charValue % 2;
                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 픽셀색에 텍스트 삽입
                                } break;
                        }

                        pixelElementIndex++; // 픽셀 속성값 1 증가
                        
                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }
            //넣으려는 텍스트가 사진크기보다 크면
            return bmp; // 변경된 이미지 반환(텍스트짤림현상 발생)
        }

        public static string extractText(Bitmap bmp) // 이미지에서 텍스트 추출 함수
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++) // 이미지의 세로픽셀수 만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지의 가로픽셀수 만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++) // 한 픽셀 당 R G B 이므로 총 3회 반복
                    {
                        switch (colorUnitIndex % 3) // R G B 구분
                        {
                            case 0: // R
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // charValue에 2인수데이터 추가
                                } break;
                            case 1: // G
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // charValue에 2인수데이터 추가
                                } break;
                            case 2: // B
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // charValue에 2인수데이터 추가
                                } break;
                        }

                        colorUnitIndex++; 

                        if (colorUnitIndex % 8 == 0) // 한글자 작업이 끝나면
                        {
                            charValue = reverseBits(charValue); // 문자값에 reverseBits 함수 적용

                            if (charValue == 0) // 문자값이 비었다면 ( 이미지에 들어간 텍스트가 없거나 들어간 텍스트를 다 뽑아냈을 때 )
                            {
                                return extractedText; // 문자열 반환
                            }
                            char c = (char)charValue; // int 형 문자값을 char 형 c에 대입

                            extractedText += c.ToString(); // c를 string형으로 변환하여 문자열에 추가
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n) // 2진수를 10진수로 변환하는 함수
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
