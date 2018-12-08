using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        //enum : 열거형 상수 표현
        //기본 int, 0, 1, 2...
        public enum State
        {
            //숨김
            Hiding,
            //0으로 채움
            Filling_With_Zeros
        };

        //숨기기 위한 메소드
        public static Bitmap embedText(string text, Bitmap bmp)
        {

            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            //전체 픽셀 순회
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    //현재 처리중인 픽셀
                    Color pixel = bmp.GetPixel(j, i);

                    //각 픽셀 LSB 제거 - 0으로
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    //R,G,B 반복문
                    for (int n = 0; n < 3; n++)
                    {
                        //한 문자가 다 처리된 경우
                        if (pixelElementIndex % 8 == 0)
                        {
                            //0이 8개 연속으로 채워지면 텍스트의 끝
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                //이미지 마지막 픽셀 적용
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                //숨겨진 이미지 반환
                                return bmp;
                            }

                            //모든 문자가 다 숨겨진 경우
                            if (charIndex >= text.Length)
                            {
                                //텍스트의 끝 표시
                                state = State.Filling_With_Zeros;
                            }
                            //아닌 경우
                            else
                            {
                                //다음 문자 처리
                                charValue = text[charIndex++];
                            }
                        }

                        //LSB 비트에 숨길 픽셀요소 확인
                        switch (pixelElementIndex % 3)
                        {
                            //R
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        //R에 charValue LSB값으로 변경
                                        R += charValue % 2;
                                        //charValue LSB 제거
                                        charValue /= 2;
                                    }
                                }
                                break;
                            //G
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                }
                                break;
                            //B
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    //R,G,B 변경 값 저장
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
                                break;
                        }

                        //+1
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

        //추출을 위한 메소드
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            //추출 텍스트 저장 변수
            string extractedText = String.Empty;

            //전체 픽셀 순회
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    //해당 픽셀 값
                    Color pixel = bmp.GetPixel(j, i);

                    //R,G,B
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            //R
                            case 0:
                                {
                                    //charValue 좌로 shift R의 LSB 추가
                                    charValue = charValue * 2 + pixel.R % 2;
                                }
                                break;
                            //G
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                }
                                break;
                            //B
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                }
                                break;
                        }

                        colorUnitIndex++;

                        //한 문자가 다 처리된 경우
                        if (colorUnitIndex % 8 == 0)
                        {
                            //charValue 방향 역순으로
                            charValue = reverseBits(charValue);

                            //문자열이 더이상 없음
                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;

                            //추출 텍스트 저장
                            extractedText += c.ToString();
                        }
                    }
                }
            }

            return extractedText;
        }

        //역순 변경
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
