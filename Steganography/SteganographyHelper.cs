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

        public static Bitmap embedText(string text, Bitmap bmp) //hide 파트
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0; //전체 픽셀을 가지고 온다

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); //각 픽셀값을 추출한다.

                    R = pixel.R - pixel.R % 2; // 2로 나눠서 뺸 나머지 값으로 현 RGB 값을 LSB 값을 0으로 만든다.
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) //RGB에 택스트를 숨길수 있게 3번씩 반복
                    {
                        if (pixelElementIndex % 8 == 0) //한 비트를 모두 RGB에 셋팅하고 
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //문자열의 크기보다 1 더 이동했다면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // RGB 값을 0으로 세팅
                                }

                                return bmp; //bmp값 리턴ㄴ
                            }

                            if (charIndex >= text.Length)
                            {
                                state = State.Filling_With_Zeros; //state를 Filling_With_Zeros로 바꿈.
                            }
                            else
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {
                            case 0: 
                                {
                                    if (state == State.Hiding)//문자의 마지막 비트에 R 비트를 저장하고 문자의 마지막비트 삭제
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 1: 
                                {
                                    if (state == State.Hiding)// 위와 동일 G비트 저장
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)// 위와 동일 B비트 저장
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //RGB 를 다채웠으니 픽셀에 문자숨긴 RGB값 세팅
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros) //state 값이 Filling_with_zeros 이면
                        {
                            zeros++; //zeros 값 추가.
                        }
                    }
                }
            }

            return bmp;//위작업이 끝나면 bmp return.
        }
        
        public static string extractText(Bitmap bmp) //추출 파트 
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++) // 전체픽셀 가져오기
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);   //픽셀에서 데이터 추출

                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0: // charValue에서 한비트 이동후 R비트 추가
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                }
                                break;
                            case 1: // 위와 동일 G비트 추가
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                }
                                break;
                            case 2: // 위와 동일  B비트 추가
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                }
                                break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)    // 8비트를 완성한 경우
                        {
                            charValue = reverseBits(charValue); // 추출한 문자의 비트를 리버스

                            if (charValue == 0) // 추출한문자의 값이 0일경우
                            {
                                return extractedText;   // extractedText을 리턴
                            }
                            char c = (char)charValue; 

                            extractedText += c.ToString();  // extractedText에서 추출한 문자 추가
                        }
                    }
                }
            }

            return extractedText;   //끝나면 extractedText 리턴
        }

        public static int reverseBits(int n)    // n: 리버스 할 int형 변수
        {
            int result = 0;

            for (int i = 0; i < 8; i++) // 8비트데이터를 리버스
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }
    }
}
