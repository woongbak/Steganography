using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        // 열거형 정의로 상태를 표시한다. 
        public enum State
        {
            //숨기는 상태
            Hiding,
            //0으로 채우는 상태
            Filling_With_Zeros
        };

        //text를 숨기기위한 메소드이다.
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            //state를 Hiding으로 바꾼다.
            State state = State.Hiding;

            //숨기려는 문자열 개수를 담는다.
            int charIndex = 0;

            //문자의 값을 정수로 담는다.
            int charValue = 0;

            //한문자를 8개의 LSB에 나눠 넣기위한 인덱스이다.
            long pixelElementIndex = 0;

            int zeros = 0;

            //픽셀 값을 모두 0으로 초기화한다.
            int R = 0, G = 0, B = 0;

            //이미지의 너비와 높이에 따른 이중포문
            for (int i = 0; i < bmp.Height; i++)
            {

                for (int j = 0; j < bmp.Width; j++)
                {
                    //픽셀의 데이터를 얻어온다.
                    Color pixel = bmp.GetPixel(j, i);

                    //R,G,B 각각의 LSB를 0으로 초기화한다.
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    //R,G,B 각각
                    for (int n = 0; n < 3; n++)
                    {
                        //한 문자를 다 숨겼을 경우
                        if (pixelElementIndex % 8 == 0)
                        {
                            //state가 filling_with_zero 상태이고 zeros가 8이라면 
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                //마지막이 B로 끝나지 않는다면
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    //불완전하게 끝난 픽셀 세팅
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            //text를 다 숨겼다면
                            if (charIndex >= text.Length)
                            {
                                //Filling_with_Zeros 상태로 전환한다.
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                //charValue에 text의 다음 문자의 int값을 넣는다.
                                charValue = text[charIndex++];
                            }
                        }

                        //LSB에 bit를 숨기는데 픽셀 구성 요소인 R G B 중 어디에 숨길지
                        switch (pixelElementIndex % 3)
                        {
                            //R인 경우
                            case 0:
                                {
                                    //상태가 Hiding이라면
                                    if (state == State.Hiding)
                                    {
                                        //charValue의 최하위 비트를 R에 넣는다.
                                        R += charValue % 2;

                                        //charValue 값을 우 shitf해준다.
                                        charValue /= 2;
                                    }
                                }
                                break;
                            //G인 경우
                            case 1:
                                {
                                    //상태가 Hiding이라면
                                    if (state == State.Hiding)
                                    {
                                        //charValue의 최하위 비트를 R에 넣는다.
                                        G += charValue % 2;

                                        //charValue 값을 우 shift해준다.
                                        charValue /= 2;
                                    }
                                }
                                break;
                            //B인 경우
                            case 2:
                                {
                                    //상태가 Hiding이라면
                                    if (state == State.Hiding)
                                    {
                                        //charValue의 최하위 비트를 B에 넣는다.
                                        B += charValue % 2;

                                        //charValue 값을 우 shift해준다.
                                        charValue /= 2;
                                    }

                                    //픽셀을 세팅한다. 
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
                                break;
                        }

                        pixelElementIndex++;

                        //텍스트를 다 숨기고 0는 넣는 상태라면
                        if (state == State.Filling_With_Zeros)
                        {
                            //zeros를 1씩 증가시킨다.
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        //text를 추출하는 메소드이다.
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            //추출된 test를 저장하기 위한 변수이다.
            string extractedText = String.Empty;

            //이미지의 너비와 높이에 따른 이중포문
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    //(j,i)에 해당하는 픽셀 값을 가져온다.
                    Color pixel = bmp.GetPixel(j, i);

                    //R,G,B 에 각각
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            //R이라면
                            case 0:
                                {
                                    //charValue에 좌shift 후 R의 LSB를 charValue의 LSB에 넣는다.
                                    charValue = charValue * 2 + pixel.R % 2;
                                }
                                break;
                            //G이라면
                            case 1:
                                {
                                    //charValue에 좌shift 후 G의 LSB를 charValue의 LSB에 넣는다.
                                    charValue = charValue * 2 + pixel.G % 2;
                                }
                                break;
                            //B이라면
                            case 2:
                                {
                                    //charValue에 좌shift 후 B의 LSB를 charValue의 LSB에 넣는다.
                                    charValue = charValue * 2 + pixel.B % 2;
                                }
                                break;
                        }

                        colorUnitIndex++;

                        //하나의 문자 8bit를 다 추출 했다면
                        if (colorUnitIndex % 8 == 0)
                        {
                            //문자의 값을 reverse하여 저장한다.
                            charValue = reverseBits(charValue);

                            //문자열 끝에 도달했다면 return extractedText한다.
                            if (charValue == 0)
                            {
                                return extractedText;
                            }

                            // 정수형인 charValue를 char형으로 바꾸어서 변수c에 넣는다.
                            char c = (char)charValue;

                            // extractedText 문자열에 문자를 붙인다.
                            extractedText += c.ToString();
                        }
                    }
                }
            }

            return extractedText;
        }
        // 추출을 하기위해 charValue를 역순으로 뒤집는 메소드
        public static int reverseBits(int n)
        {
            int result = 0;
            //8bit 이므로 8번 수행한다.
            for (int i = 0; i < 8; i++)
            {
                //result를 좌로 shift하고 charValue의 LSB를 result의 LSB에 넣는다.
                result = result * 2 + n % 2;
                //charValue를 우로 shift한다.
                n /= 2;
            }

            return result;
        }
    }
}