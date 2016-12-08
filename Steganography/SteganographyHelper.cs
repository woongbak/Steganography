using System;
using System.Drawing;

namespace Steganography     // Steganography namespace 선언
{
    class SteganographyHelper
    {
        public enum State  // 열거형 enum 으로 Hiding, Filling_With_Zeros 선언
        {
            Hiding,  //0
            Filling_With_Zeros  //1
        };

        public static Bitmap embedText(string text, Bitmap bmp)  // 이미지에 텍스트를 삽입하는 함수 embedText 선언, 인자로 문자열과 비트맵 이미지
        {
            State state = State.Hiding;  

            int charIndex = 0;   // 읽은 문자 갯수 나타내는 변수

            int charValue = 0;  // 바이너리 형태의 문자 값이 저장될 변수

            long pixelElementIndex = 0; // RGB를 나타내기 위한 변수

            int zeros = 0;     // 문자하나의 읽은 bit 수

            int R = 0, G = 0, B = 0;  // RGB 비트가 임시 저장될 변수

            for (int i = 0; i < bmp.Height; i++)  // 이미지 파일의 세로 길이 만큼 i가 커짐 
            {
                for (int j = 0; j < bmp.Width; j++)  // 이미지 파일의 가로 길이 만큼 j가 커짐  ( 반복문을 돌며 모든 픽셀 정보를 가져옴)
                {
                    Color pixel = bmp.GetPixel(j, i);  // 해당 픽셀의 색 정보를 가져온다. Color 구조체에는 색의 3원소인 RGB가 들어있다. (png 파일은 알파까지)

                    R = pixel.R - pixel.R % 2;   // RGB에 j,i픽셀 RGB 비트의 LSB 를 0 으로 초기화 시킨 값 저장.
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)  // RGB의 마지막 비트를 변경하기 위한 for 문
                    {
                        if (pixelElementIndex % 8 == 0)  // pEI 값이 0 또는 8의 배수라면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 문자를 다 읽은 경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)  // RGB값이 모두 변경되지 않은 경우
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));  //픽셀에 RGB 값 세팅
                                }

                                return bmp;  // bmp 반환
                            }

                            if (charIndex >= text.Length)  // 숨길 문자열을 다 저장한 경우
                            {
                                state = State.Filling_With_Zeros; // state 값에 State.Filling_With_Zeros 값 대입(문자열이 다 숨겨진 상태이다)
                            }
                            else   // 다 저장하지 않았다면
                            {
                                charValue = text[charIndex++];  // 문자열의 문자 하나를 가져오고 charindex 1 증가
                            }
                        }

                        switch (pixelElementIndex % 3)   // pEI%3 값에 따라 RGB를 나타냄.
                        {
                            case 0: // pEI%3=0 인 경우 : R 인 경우
                                {
                                    if (state == State.Hiding)  // 아직 Hiding 상태인 경우 ( 문자열이 아직 숨겨지고 있는 상태이다)
                                    {
                                        R += charValue % 2;  // R에 문자 비트 저장(가장 오른쪽 비트 값)
                                        charValue /= 2;    // 저장된 문자 비트 제거(가장 오른쪽 비트 제거)
                                    }
                                } break;
                            case 1:// pEI%3=0 인 경우 : G 인 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; // G에 문자 비트 저장

                                        charValue /= 2;  // 저장된 문자 비트 제거
                                    }
                                } break;
                            case 2: // B인 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;  //위와 동일한 과정

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));  // 변경된  RGB 값을 다시 저장.
                                } break;
                        }

                        pixelElementIndex++;  // pEI 값을 1씩 증가

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;  // 이미지 반환
        }

        public static string extractText(Bitmap bmp)  // 이미지에서 텍스트를 추출하기위한 extractText
        {
            int colorUnitIndex = 0;
            int charValue = 0;    // 문자값을 저장할 변수

            string extractedText = String.Empty;  // 추출된 문자열이 저장될 extractedText 초기화

            for (int i = 0; i < bmp.Height; i++)  // 이미지의 Height 크기 만큼 반복문 진행
            {
                for (int j = 0; j < bmp.Width; j++)   // 이미지의 Width (너비) 크기만큼 반복문 진행 -> 모든 픽셀 훑기 
                {
                    Color pixel = bmp.GetPixel(j, i);  // pixel 의 이미지의 픽셀 값을 받아온다.
                    for (int n = 0; n < 3; n++)  // 한 픽셀의 RGB 값의 마지막 비트를 모두 가져오기위해
                    {
                        switch (colorUnitIndex % 3)  // cUI%3 값에 따라 swich 문 실행
                        {
                            case 0: // R 일때
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // charValue를 하나 왼쪽으로 하나 쉬프트 시키고 숨겨진 문자 비트 삽입
                                } break;
                            case 1: // G 일때 
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // charValue를 하나 왼쪽으로 하나 쉬프트 시키고 숨겨진 문자 비트 삽입
                                } break;
                            case 2:  // B 일때
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // charValue를 하나 왼쪽으로 하나 쉬프트 시키고 숨겨진 문자 비트 삽입
                                } break;
                        }

                        colorUnitIndex++;  // cUI 값하나 증가 (R,G,B 를 판단하기 위한 Index)

                        if (colorUnitIndex % 8 == 0)  // 문자 하나가 모두 추출 되었다면
                        {
                            charValue = reverseBits(charValue);  //바이너리 값을 올바른 순서로 정렬

                            if (charValue == 0)  // 읽은 문자 값이 없다면(모든 문자열을 읽은 상태라면)
                            {
                                return extractedText;  // extractedText 반환
                            }
                            char c = (char)charValue; // 텍스트에 넣기위해 형 변환

                            extractedText += c.ToString(); //extractedText 에 문자 추가.
                        }
                    }
                }
            }

            return extractedText; // 추출된 문자열 반환
        }

        public static int reverseBits(int n)
        {
            int result = 0; //리턴할 값 (원래 순서의 문자 값)

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;  // 바이너리 순서를 원래 순서로 되돌리는 과정
                                              // result를 왼쪽으로 쉬프트 후  n 의 가장 왼쪽 비트 삽입
                n /= 2;                       // n 을 오른쪽으로 하나 쉬프트
            }

            return result;  //정렬한 문자 값 리턴
        }
    }
}
