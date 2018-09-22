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
            State state = State.Hiding; //이미지에 문자 숨기기

            int charIndex = 0; //변수값 초기화

            int charValue = 0; //변수값 초기화

            long pixelElementIndex = 0; //변수값 초기화

            int zeros = 0; //변수값 초기화

            int R = 0, G = 0, B = 0; //변수값 초기화

            for (int i = 0; i < bmp.Height; i++) //높이
            {
                for (int j = 0; j < bmp.Width; j++) //너비
                {
                    Color pixel = bmp.GetPixel(j, i); //저장된 픽셀 불러오기

                    R = pixel.R - pixel.R % 2; //픽셀에서 LSB 지우기
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) 
                    {
                        if (pixelElementIndex % 8 == 0) //새로운 8비트 처리되었는지 확인
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //8 zeros가 추가되었는지 확인
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) //이미지의 마지막 픽셀 적용
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); 
                                }

                                return bmp; //Text가 숨겨진 BMP 리턴
                            }

                            if (charIndex >= text.Length) //모든 문자들이 숨겨져있는지 확인
                            {
                                state = State.Filling_With_Zeros; //Zeros를 Text의 마지막에 추가
                            }
                            else
                            {
                                charValue = text[charIndex++]; //다음 글자로 커서를 욺김
                            }
                        }

                        switch (pixelElementIndex % 3) // 어떤 픽셀이 LSB에서 비트를 숨길 차례인지 확인
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        //가장 오른쪽의 비트에 있는 문자 지우기
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

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                            //Zeros가 8이 될때까지 값 증가
                        }
                    }
                }
            }

            return bmp; //BMP 리턴
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; //변수값 초기화

            for (int i = 0; i < bmp.Height; i++) //높이
            {
                for (int j = 0; j < bmp.Width; j++) //너비
                {
                    Color pixel = bmp.GetPixel(j, i); //가져오기
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            //픽셀에서 LSB를 가져온 후, 문자의 오른쪽에 +1을 한다  
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

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0) //8비트가 추가되었는지 확인
                        {
                            charValue = reverseBits(charValue); //맞는 문자를 결과값 Text에 추가

                            if (charValue == 0) //0인지 확인
                            {
                                return extractedText;
                            }
                            char c = (char)charValue; //int애서 char로 변환

                            extractedText += c.ToString(); //맞는 문자를 결과값 Text에 추가
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)
        {
            int result = 0; //결과값 초기화

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2; //결과값 연산

                n /= 2;
            }

            return result;
        }
    }
}
