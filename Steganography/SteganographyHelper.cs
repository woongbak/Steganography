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
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++) //사진의 세로 길이만큼 반목
            {
                for (int j = 0; j < bmp.Width; j++) //사진의 가로 길이만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i); //각 픽셀의 RGB값을 추출한다

                    G = pixel.G - pixel.G % 2; //LSB에 데이터를 삽입하기 위해 각 픽셀의 RGB값의 LSB를 0으로 만든다
                    R = pixel.R - pixel.R % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) //각 픽셀당 RGB값을 돌면서 변경한다.
                    {
                        if (pixelElementIndex % 8 == 0) //각 픽셀당 변경이 끝났는지 확인
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //모든 작업이 끝났는지 확인
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) //변경사항이 있을 경우
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //변경사항을 저장
                                }

                                return bmp; //그림 반환
                            }

                            // 모든 문자가 숨겨졌는지 확인
                            if (charIndex >= text.Length) //모든 문자가 숨겨졌으면
                            {
                                state = State.Filling_With_Zeros; //나머지를 0으로 채우라고 상태를 변경
                            }
                            else //모든 문자가 숨겨지지 않았으면
                            {
                                charValue = text[charIndex++]; //다음 문자를 숨겨라
                            }
                        }

                        //어떤 RGB값에 데이터를 숨겨야 하는지 판단
                        switch (pixelElementIndex % 3)
                        {
                            case 0: //R값일 경우
                                {
                                    if (state == State.Hiding) //상태를 숨김으로 전환
                                    {
                                        R += charValue % 2; //값을 추가한다
                                        charValue /= 2; //추가한 문자데이터를 삭제한다
                                    }
                                } break;
                            case 1: //G값일 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: //B값일 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        pixelElementIndex++; //1 증가

                        if (state == State.Filling_With_Zeros)
                        {
                            //1증가
                            zeros++;
                        }
                    }
                }
            }

            return bmp; //그림 반환
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++) //그림 높이 만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++) //그림 너비 만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i); //픽셀값 얻어오고
                    for (int n = 0; n < 3; n++) // 각 픽셀의 RGB값만큼 반복
                    {
                        switch (colorUnitIndex % 3) //RGB값 구분
                        {
                            case 0: //R일때
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // 값 추출
                                } break;
                            case 1: //G일때
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2: //B일때
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++; //1증가

                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue);

                            if (charValue == 0) //다 추출했으면
                            {
                                return extractedText; //추출한 문자열 반환
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString(); //결과데이터에 추출한 문자를 추가한다.
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n) //비트를 반대로 바꿔준다 ex) 10010101 -> 10101001
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
