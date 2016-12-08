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

        public static Bitmap embedText(string text, Bitmap bmp) // 이미지를 숨기기 위한 함수
        {
            State state = State.Hiding;

            int charIndex = 0; // 숨겨지는 문자의 index값

            int charValue = 0; // 문자의 값을 int형으로 변환

            long pixelElementIndex = 0;  // 픽셀(RGB)의 인덱스를 가진다.

            int zeros = 0; 

            int R = 0, G = 0, B = 0; // 픽셀 값을 넣기 위한 변수 선언 

            for (int i = 0; i < bmp.Height; i++)  //이중 FOR문을 이용하여 모든 픽셀에 대한 참조를 한다.
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);  // 픽셀 값을 저장

                    R = pixel.R - pixel.R % 2; // mod 2 연산값을 빼주어 최하위비트를 0으로 세팅한다.
                    G = pixel.G - pixel.G % 2; // mod 2 연산값을 빼주어 최하위비트를 0으로 세팅한다.
                    B = pixel.B - pixel.B % 2; // mod 2 연산값을 빼주어 최하위비트를 0으로 세팅한다.

                    for (int n = 0; n < 3; n++) // R,G,B에 각각 비트를 저장하기 위해 3번 반복된다.
                    {
                        if (pixelElementIndex % 8 == 0) //  8비트까지 값을 숨길수 있도록 한다.
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 숨길 문자열을 다 저장하고, 0을 8번 저장 다했을 경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)  
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 마지막 비트 연산
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length) // 문자열을 다 숨겼을 경우
                            {
                                state = State.Filling_With_Zeros;   // 0을 추가한다.
                            }
                            else // 숨길 문자열이 남은 경우
                            {
                                charValue = text[charIndex++];   // 다음 문자로 이동하여 처리한다.
                            }
                        }

                        switch (pixelElementIndex % 3)  // mod 3연산을 case문에 넣어 R,G,B에 차례대로 1비트씩 저장, charValue값을 2로 나눈 나머지 0 또는 1을 저장한다. 
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

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

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

            for (int i = 0; i < bmp.Height; i++) //이중 FOR문을 이용하여 모든 픽셀에 대한 참조를 한다.
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)  // R,G,B을 차례대로 참조한다.
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // mo2 연산을 -한걸 암호화시 했기 때문에  복호화 시 더해준다.
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

                        if (colorUnitIndex % 8 == 0) // 한 문자에 복구가 끝난경우
                        {
                            charValue = reverseBits(charValue); // 복구할 경우 비트의 순서가 반대로 되어있기때문에 다시 순서를 맞추기 위해 reverse 해준다.
																
                            if (charValue == 0)
                            {
                                return extractedText;  // 값의 복구가 끝이 나면 리턴한다.
                            }
                            char c = (char)charValue; // int형 타입으로 저장된 문자를 다시 char형으로 형변환한다. 

                            extractedText += c.ToString(); // extractedText에 추출한 문자를 저장한다. 
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)  //비트 reverse를 위한 함수
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
