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

        public static Bitmap embedText(string text, Bitmap bmp)//Hide Function
        {
            State state = State.Hiding;//사진의 상태

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)//사진의 높이만큼
            {
                for (int j = 0; j < bmp.Width; j++)//사진의 너비만큼 순회
                {
                    Color pixel = bmp.GetPixel(j, i);//j행 i열 픽셀을 얻어온다

                    R = pixel.R - pixel.R % 2;//LSB를 0으로 만든다.
                    G = pixel.G - pixel.G % 2;//LSB를 0으로 만든다.
                    B = pixel.B - pixel.B % 2;//LSB를 0으로 만든다.

                    for (int n = 0; n < 3; n++)//3번 반복한다.
                    {
                        if (pixelElementIndex % 8 == 0)//픽셀 인덱스가 8의 배수이고
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//더이상 채울게 없다면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)//숨기는 상태일때
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//픽셀을 변경한다.
                                }
                                
                                return bmp;//함수를 종료하고 비트맵을 반환한다.
                            }

                            if (charIndex >= text.Length)//더이상 숨길게 없다면
                            {
                                state = State.Filling_With_Zeros;//0으로 채운다.
                            }
                            else//더 숨길게 있다면
                            {
                                charValue = text[charIndex++];//다음걸 숨긴다.
                            }
                        }

                        switch (pixelElementIndex % 3)//1비트씩 삽입
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//R,G,B 값을 지정한 후 픽셀 설정
                                } break;
                        }

                        pixelElementIndex++;//다음 픽셀로 간다

                        if (state == State.Filling_With_Zeros)//만약 더이상 채울게 없다면
                        {
                            zeros++;//데이터를 넣을 자리를 0으로 채운다.
                        }
                    }
                }
            }

            return bmp;//비트맵 반환
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)//비트맵의 높이만큼
            {
                for (int j = 0; j < bmp.Width; j++)//비트맵의 너비만큼
                {
                    Color pixel = bmp.GetPixel(j, i);//픽셀 하나를 가져온다.
                    for (int n = 0; n < 3; n++)//3번 반복한다.
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;//R에서 1비트 추출
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;//G에서 1비트 추출
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;//B에서 1비트 추출
                                } break;
                        }

                        colorUnitIndex++;//다음 픽셀로 간다.

                        if (colorUnitIndex % 8 == 0)//추출한 비트가 8개면
                        {
                            charValue = reverseBits(charValue);//1바이트를 추출한 것이다.

                            if (charValue == 0)//0이 8개 연속되어있으면
                            {
                                return extractedText;//지금까지 추출한 텍스트 반환
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString();//아니라면 지금까지 추출한 텍스트에 덧붙이기
                        }
                    }
                }
            }

            return extractedText;//텍스트 반환
        }

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
