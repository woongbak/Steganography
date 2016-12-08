using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State // 두가지 state = hiding은 숨기는 중이고, filling_with_zeros는 pixel값에 zero를 채우는 중이다.
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp) // Text를 숨기기 위한 method. 
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지의 width와 height만큼 반복.
                {
                    Color pixel = bmp.GetPixel(j, i); // 이미지의 pixel(R,G,B)를 가져온다.

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;  // 각 R,G,B 값에 대해서 나머지 2만큼 뺸다.(LSB를 0으로 바꾼다.)

                    for (int n = 0; n < 3; n++) // R,G,B 값에 대해서 실행해야 하므로 for문이 3번 돈다.
                    {
                        if (pixelElementIndex % 8 == 0) // char는 1byte이므로 8bit를 기준으로 삼음. 
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // pixel이 시작 할때 인지 아니면 중간인지. 
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // pixel의 중간이면 setPixel을 해야됨.
                                }
                                // pixel이 시작할 때 이면 이미 setPixel이 되있으므로 안해도 된다.  

                                return bmp;
                            }

                            if (charIndex >= text.Length) // charIndex가 text의 길이보다 길거나 같으면 상태를 변경.
                            {
                                state = State.Filling_With_Zeros;  // 그 상태는 pixel에 0을 채우는 것이다.
                            }
                            else
                            {
                                charValue = text[charIndex++]; // charIndex가 text의 길이보다 작으면 text[](char)를 charValue에 저장.
                            }
                        }

                        switch (pixelElementIndex % 3) // R,G,B 값에 charValue의 마지막 비트를 넣고 charValue를 오른쪽으로 한칸씩 shift.
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if (state == State.Hiding) // 처음의 charValue값이 들어간다.
                                    {
                                        B += charValue % 2; // charValue의 마지막 비트를 넣는다. 

                                        charValue /= 2; // 나머지 2를 해줘서 오른쪽으로 한칸씩 shift한 것과 동일하다.
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // R,G,B에 대해서 마지막 비트에 charValue를 넣었으므로, 해당 width와 height에 set.
                                }
                                break;
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

        public static string extractText(Bitmap bmp) // 이미지에 숨겨져있는 text를 찾아내는 함수.
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) // R,G,B 값에 대해서 해야하므로 3의 나머지로 실행.
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;  // charValue를 왼쪽으로 한비트 이동시키고 R,G,B의 마지막 비트를 더한다.
                                }
                                break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                }
                                break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                }
                                break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0) // 8비트를 기준으로 추출.
                        {
                            charValue = reverseBits(charValue); // charValue의 마지막 비트를 R,G,B 에 넣었으므로 추출할 때는 reverse를 해줘야 한다.

                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;  // int로 되어있는 charValue를 char 형으로 다시 바꿔줌.

                            extractedText += c.ToString(); // extractText에 char배열(string)으로 저장한다.
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n) // bit를 reverse하는 함수.
        {
            int result = 0;

            for (int i = 0; i < 8; i++) // 8bit가 기준이므로 for문을 8번 반복.
            {
                result = result * 2 + n % 2; // result를 왼쪽으로 한 비트 씩 옮기고 n의 마지막 비트를 넣음.

                n /= 2; // n을 오른쪽으로 한 비트씩 이동.
            }

            return result;
        }
    }
}
