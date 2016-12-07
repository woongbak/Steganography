using System;
using System.Drawing; // Steganography 네임스페이스 선언.

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State // enum으로 State 정의
        {
            Hiding, // 0
            Filling_With_Zeros // 1
        };

        public static Bitmap embedText(string text, Bitmap bmp) // text 와 bmp 파일을 인자로 받아오는 embedText.
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            /* 이미지의 전체 픽셀을 돌기위한 loop */
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); // j,i 픽셀을 받아온다.

                    /* RGB의 마지막 비트를 0으로 세팅 */
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    /* RGB가 세개이므로 3번 반복하도록 하는 loop */
                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0) // 
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 텍스트를 모두 받아오고 , 0을 8번 저장 했으면.
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // R G B를 j,i 픽셀에 세팅.
                                }

                                return bmp; // bmp 리턴.
                            }

                            if (charIndex >= text.Length) // 문자열을 모두 숨겼으면.
                            {
                                state = State.Filling_With_Zeros; // state 를 1로 변경한다.
                            }
                            else // 문자열이 남아있으면.
                            {
                                charValue = text[charIndex++]; // charIndex를 아스키로 변경한 값을 charValue로 저장하고 증가시킨다.
                            }
                        }

                        switch (pixelElementIndex % 3) /* RGB이므로 3개의 case를 두는 switch */
                        {
                            case 0: // RED면.
                                {
                                    if (state == State.Hiding) // state가 0이면 
                                    {
                                        R += charValue % 2; // R의 마지막 비트에 문자열 저장.
                                        charValue /= 2; // charValue 를 2로 나누고 저장한다. 다음 비트로 시프트.
                                    }
                                }
                                break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; // G의 마지막 비트에 문자열 저장.

                                        charValue /= 2; // charValue 를 2로 나누고 저장한다. 시프트
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2; // B의 마지막 비트에 문자열 저장

                                        charValue /= 2; // charValue 를 2로 나누고 저장한다. 시프트
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // RGB를 픽셀에 세팅한다.
                                }
                                break;
                        }

                        pixelElementIndex++; // pixelElementIndex를 1 증가시킨다.

                        if (state == State.Filling_With_Zeros) // state가 1일경우.
                        {
                            zeros++; // zeros를 1 증가시킨다.
                        }
                    }
                }
            }

            return bmp; // bmp 리턴.
        }

        public static string extractText(Bitmap bmp) // 숨긴 문자열 추출하는 extractText함수.
        {
            int colorUnitIndex = 0; // 픽셀의 비트 인덱스.
            int charValue = 0; // 문자저장 변수.

            string extractedText = String.Empty; // 문자열을 저장할 string.

            /* 이미지 전체를 돌기위한 loop */
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀을 받아온다.
                    for (int n = 0; n < 3; n++) // RGB 세개이므로 3번 도는 루프. 
                    {
                        switch (colorUnitIndex % 3) // 색을 구분하기 위한 인덱스를 파라미터로 받는다.
                        {
                            case 0: // R
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // charValue에 2를 곱하고 픽셀의 R의 비트를 charValue에 넣는다. 
                                }
                                break;
                            case 1: // G
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // charValue에 2를 곱하고 픽셀의 R의 비트를 charValue에 넣는다.
                                }
                                break;
                            case 2: // B
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // charValue에 2를 곱하고 픽셀의 R의 비트를 charValue에 넣는다.
                                }
                                break;
                        }

                        colorUnitIndex++; // 인덱스를 1 증가시킨다.

                        if (colorUnitIndex % 8 == 0) // 한 문자열 8bit 가 끝났으면.
                        {
                            charValue = reverseBits(charValue);  // 추출이므로 거꾸로 읽어서 charValue에 넣는다.

                            if (charValue == 0) // charValue가 0이면.
                            {
                                return extractedText; // 추출 문자를 리턴.
                            }
                            char c = (char)charValue; // charValue를 아스키로 변환하여 c에 넣는다.

                            extractedText += c.ToString(); // c를 string으로 변환하여 추출된 문자열에 추가한다.
                        }
                    }
                }
            }

            return extractedText; // 추출된 텍스트 반환.
        }

        public static int reverseBits(int n) // 비트를 거꾸로 바꿔주는 함수.
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


