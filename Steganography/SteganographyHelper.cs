using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        /* 열거형 정의 */
        public enum State
        {
            Hiding, // 0
            Filling_With_Zeros // 1
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    /* 픽셀 데이터를 얻어옴 */
                    Color pixel = bmp.GetPixel(j, i);

                    /* 얻어온 픽셀의 R,G,B값을 0으로 초기화*/
                    R = pixel.R - pixel.R % 2; 
                    G = pixel.G - pixel.G % 2; 
                    B = pixel.B - pixel.B % 2;

                    /* RGB 저장을 위해 3회 반복 */
                    for (int n = 0; n < 3; n++)
                    {
                        /* 픽셀인덱스 mod 8 값이 0이면 */
                        if (pixelElementIndex % 8 == 0)
                        {
                            /* state가 1이고(즉, 데이터가 전부 0인경우) 0이 8개이면 */
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                /* 픽셀 인덱스-1 mod 3 값이 2보다 작으면*/
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    /* 픽셀 설정 */
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                /* 비트맵 데이터 리턴 */
                                return bmp;
                            }

                            /* 현재 문자 인덱스가 입력된 문자열 길이보다 긴 경우 */
                            if (charIndex >= text.Length)
                            {
                                /* 문자가 존재하지 않음을 가리킴 */
                                state = State.Filling_With_Zeros;
                            }
                            /* 길지 않은 경우*/
                            else
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        /* 각 픽셀의 인덱스를 3으로 나눈 결과값을 이용해 switch (R,G,B값으로 각각 매칭) */
                        switch (pixelElementIndex % 3)
                        {
                            /* R의 값을 대입*/
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            /* G의 값을 대입 */
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            /* B의 값을 대입*/
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    /* bmp 데이터에 픽셀 저장 */
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        /* 다음 픽셀로 이동*/
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

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            /* height 만큼 반복 */
            for (int i = 0; i < bmp.Height; i++)
            {
                /* width 만큼 반복 */
                for (int j = 0; j < bmp.Width; j++)
                {
                    /* 픽셀 얻어옴 */
                    Color pixel = bmp.GetPixel(j, i);
                    /* RGB 분석을 위해 3회 반복 */
                    for (int n = 0; n < 3; n++)
                    {
                        /* 인덱스 mod 3 값을 활용해 switch */
                        switch (colorUnitIndex % 3)
                        {
                            case 0: // R값
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1: // G값
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2: // B값
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        /* 인덱스++ */
                        colorUnitIndex++;

                        /* 8비트 모두 복구한 경우 */
                        if (colorUnitIndex % 8 == 0)
                        {
                            /* 얻어진 값을 뒤집음 */
                            charValue = reverseBits(charValue);

                            /* 얻어진 값이 NULL이면(즉, 문자열의 끝이면) */
                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;

                            /* 문자열에 문자 추가*/
                            extractedText += c.ToString();
                        }
                    }
                }
            }

            /* 얻어진 문자열 리턴 */
            return extractedText;
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
