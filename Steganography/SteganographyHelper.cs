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
        };//state라는 열거형을 선언

        public static Bitmap embedText(string text, Bitmap bmp) // 문자열과 사진을 받아서 숨기는 함수
        {
            State state = State.Hiding;// 지금의 상태를 숨기는 중이라고 설정해줌

            int charIndex = 0;// 값 초기화

            int charValue = 0;//값 초기화

            long pixelElementIndex = 0;//값 초기화

            int zeros = 0;

            int R = 0, G = 0, B = 0; //R,G,B값 초기화

            for (int i = 0; i < bmp.Height; i++)//0부터 bmp의 높이만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++)// 0부터 bmp의 길이만큼 반복 즉 bmp의 모든 픽셀을 다 돌게되어있음
                {
                    Color pixel = bmp.GetPixel(j, i); //color 픽셀에 가져온 각 픽셀의 RGB값을 넣는다

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    //각 RGB값에다 RGB값을 2로나눈나머지값을 뺀다

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)//pixelElementIndex의 8로나눈 나머지가 0이라면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//끝내는 상태이고 && zeros == 8 이면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)//pixelElementIndex - 1 을 3으로나눈 나머지가 2보다 작으면
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//픽셀의 색을 설정함
                                }

                                return bmp;//bmp를 반환
                            }

                            if (charIndex >= text.Length)//charindex가 현재시스템의 문자수보다 크거나 같으면
                            {
                                state = State.Filling_With_Zeros;//state를 끝내는 상태로바꿈
                            }
                            else
                            {
                                charValue = text[charIndex++];//charvalue 에 text[charIndex++]대입
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {
                            case 0://pixelElementIndex의 나머지가 0일때
                                {
                                    if (state == State.Hiding)//숨기는 상태일때
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }//R값에 charVaule의 2로나눈 나머지값을 저장하고 charvaule를 2로 나눈 몫을 charvaule에 저장
                                } break;
                            case 1://pixelElementIndex의 나머지가 1일때
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }//G값에 charVaule의 2로나눈 나머지값을 저장하고 charvaule를 2로 나눈 몫을 charvaule에 저장
                                } break;
                            case 2://pixelElementIndex의 나머지가 2일때
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }//B값에 charVaule의 2로나눈 나머지값을 저장하고 charvaule를 2로 나눈 몫을 charvaule에 저장

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//픽셀의 색을 설정
                                } break;
                        }

                        pixelElementIndex++;//pixelElementIndex 1증가

                        if (state == State.Filling_With_Zeros)//끝내는 상태로 전환
                        {
                            zeros++;// zero 에 1더함
                        }
                    }
                }
            }

            return bmp;//bmp반환
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;//값 초기화
            int charValue = 0;//값 초기화

            string extractedText = String.Empty;//빈문자열을 넣어서 초기화

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);//모든 픽셀의 색정보를 가져옴
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0://colorUnitIndex 를 3으로 나눈값이 0이면
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //charVaule에 charValue * 2 + pixel.R % 2값을 넣고 종료
                                } break;
                            case 1://colorUnitIndex 를 3으로 나눈값이 1이면
                                {
                                    charValue = charValue * 2 + pixel.G % 2; //charVaule에 charValue * 2 + pixel.G % 2값을 넣고 종료
                                } break;
                            case 2://colorUnitIndex 를 3으로 나눈값이 2 이면
                                {
                                    charValue = charValue * 2 + pixel.B % 2; //charVaule에 charValue * 2 + pixel.B % 2값을 넣고 종료
                                } break;
                        }

                        colorUnitIndex++;//colorUnitIndex 1증가

                        if (colorUnitIndex % 8 == 0)//8번째 마다
                        {
                            charValue = reverseBits(charValue); //비트를 반대로 바꿔줌

                            if (charValue == 0) //charvaule가 0이면
                            {
                                return extractedText;//문자열 반환
                            }
                            char c = (char)charValue;//c에 charvalue에 스트링값 저장

                            extractedText += c.ToString();//extractedText 에 문자열값으로 변환시켜줌
                        }
                    }
                }
            }

            return extractedText;//문자열 반환
        }

        public static int reverseBits(int n)//2진수를 반대로 바꿔줌
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
