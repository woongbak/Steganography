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

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding; // 현재 State 를 Hinding 상태로 변환
            // 27줄까지 변수 초기화
            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;
            //
            for (int i = 0; i < bmp.Height; i++) // 읽어온 Bitmap 파일의 높이 읽어오기 
            {
                for (int j = 0; j < bmp.Width; j++) // 읽어온 Bitmap 파일의 너비 읽어오기
                {
                    Color pixel = bmp.GetPixel(j, i); // Color pixel 변수에 bmp의 픽셀을 너비부터 한줄씩 가져옴

                    R = pixel.R - pixel.R % 2; //연산결과 색의 값 R에 저장
                    G = pixel.G - pixel.G % 2; //연산결과 색의 값 G에 저장
                    B = pixel.B - pixel.B % 2; //연산결과 색의 값 B에 저장

                    for (int n = 0; n < 3; n++) // R,G,B 세개의 값으로 저장되있는 pixel 에 접근하기위해 포문 진행
                    {
                        if (pixelElementIndex % 8 == 0)
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 상태가 Filling_With_Zeros 상태이고 Zeros 가 8이면 진행
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // pixel ElementIndex 의 값 -1 이 3으로 나눈 나머지값이 2보다 작으면 진행
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // Bitmap에 Color의 FromArgb 함수에 현재 RGB 값을 입력후 결과값을 Set.
                                }

                                return bmp; // bmp에 저장
                            }

                            if (charIndex >= text.Length) // Text의 길이가 charindex 보다 작거나 같을 때 진행
                            {
                                state = State.Filling_With_Zeros; // 현재 상태 변환
                            }
                            else
                            {
                                charValue = text[charIndex++]; // 위 조건문이 아니면 charValue에 텍스트의 길이만큼 배열 생성
                            }
                        }

                        switch (pixelElementIndex % 3) // pixelElementIndex의 값이 3보다 작은 케이스들을 나눔
                        {
                            case 0:
                                {
                                    if (state == State.Hiding) // 현재 상태가 Hiding 일 때
                                    {
                                        R += charValue % 2; // R의 값에 + CharValue%2 결과값 + 저장
                                        charValue /= 2; // CharValue 를 2로 나눈뒤 저장
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; // G의 값에 + CharValue%2 결과값 + 저장                                        
                                        charValue /= 2;  // CharValue 를 2로 나눈뒤 저장
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;// B의 값에 + CharValue%2 결과값 + 저장                                                                                
                                        charValue /= 2; // CharValue 를 2로 나눈뒤 저장
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // B의 값에 현재까지 RGB값 전달하여 Set
                                } break;
                        }

                        pixelElementIndex++; // 픽셀의 Element 인덱스 1 증가

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++; // 위와 같은 조건문에 해당하지 않으면 zeros에 1 증가
                        }
                    }
                }
            }

            return bmp; // For문을 돌고 난 결과값을 bmp(Bitmap)파일로 Return
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; // 비어있는 스트링 값을 extractedText 값에 저장

            for (int i = 0; i < bmp.Height; i++) // Bitmap 의 높이만큼 for 문 진행
            {
                for (int j = 0; j < bmp.Width; j++) // Bitmap 의 너비만큼 for 문 진행
                {
                    Color pixel = bmp.GetPixel(j, i); // Color pixel 변수에 bmp의 픽셀을 너비부터 한줄씩 가져옴
                    for (int n = 0; n < 3; n++) // R,G,B 세개의 값으로 저장되있는 pixel 에 접근하기위해 포문 진행 
                    {
                        switch (colorUnitIndex % 3) // colorUnitIndex %3 인 나머지가 0이거나 1,2 일 때 진행
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2; 
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++; // 유닛 하나씩 증가시킴

                        if (colorUnitIndex % 8 == 0) // 8로 나눴을 때 0이면
                        {
                            charValue = reverseBits(charValue); // charValue 에 reverseBits 함수 결과값을 저장

                            if (charValue == 0) // charValue의 값이 0이면
                            {
                                return extractedText; //retrun 한다.
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString(); // 값이 찾아지면 extractedText에 값을 계속 저장
                        }
                    }
                }
            }
            return extractedText;
        }

        public static int reverseBits(int n) // 숨겨진 비트를 찾기위한 함수
        {
            int result = 0;

            for (int i = 0; i < 8; i++) // for문을 돌며 result에 값 저장
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result; // result 값 출력
        }
    }
}
