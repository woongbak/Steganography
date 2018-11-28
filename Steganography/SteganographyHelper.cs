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
            State state = State.Hiding; //state상태를 Hiding으로 설정시켜준다.

            int charIndex = 0;  //charIndex값을 0으로 초기화시켜준다.

            int charValue = 0;  // charValue값을 0으로 초기화시켜준다.

            long pixelElementIndex = 0; // pixelElementIndex값을 0으로 초기화시켜준다.

            int zeros = 0;          // zeros값을 0으로 초기화시켜준다.

            int R = 0, G = 0, B = 0;    // RGB값을 각각 0으로 초기화시켜준다.

            for (int i = 0; i < bmp.Height; i++)        //bmp.Height만큼 i를 반복시켜 준다.
            {
                for (int j = 0; j < bmp.Width; j++)     //bmp.Width만큼 j를 반복시켜준다.
                {
                    Color pixel = bmp.GetPixel(j, i);   // Color pixel이라는 변수에 지금 해당(바꾸고싶은)하는 Pixel을 매칭시켜준다.

                    R = pixel.R - pixel.R % 2;          // RGB 값에서 R값을 0으로 설정한다.
                    G = pixel.G - pixel.G % 2;          // RGB 값에서 G값을 0으로 설정한다.
                    B = pixel.B - pixel.B % 2;          // RGB 값에서 B값을 0으로 설정한다.

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)     // 숨긴데이터정보가 8비트 마다 있기에 pixelElementIndex값을 8로 나누어 값을 확인해준다.
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)    // charIndex값이 숨길 데이터의 길이보다 커지고, zeros가 8일경우(왜냐하면 숨길데이터의 정보를 8bit마다 끊어주기 때문)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)    // pixelElementIndex값을 3으로 나눈 값이 2보다 작을경우
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // pixel 에 변경한 R,G,B값을 넣어준다.
                                }

                                return bmp; //bmp값을 리턴시켜준다.
                            }

                            if (charIndex >= text.Length) // charIndex값이 숨길 데이터의 길이보다 커질경우
                            {
                                state = State.Filling_With_Zeros;   // state상태를 Filling_with_Zero로 설정해준다.
                            }
                            else
                            {
                                charValue = text[charIndex++];      // charValue값에 숨기고싶은 데이터의 index정보를 한개넣은뒤, 숨기고싶은index를 1증가시켜준다.
                            }
                        }

                        switch (pixelElementIndex % 3)      //pixelElementIndex값을 구분하기 위해 3으로 나눠준다.
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)  //charIndex값이 숨길 데이터의 길이보다 크지 않을 경우
                                    {
                                        R += charValue % 2; //R값에 숨길데이터의 정보를 넣어준다.
                                        charValue /= 2; // charValue를 2로 나눠준다.
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding) //charIndex값이 숨길 데이터의 길이보다 크지 않을 경우
                                    {
                                        G += charValue % 2; //G값에 숨길데이터의 정보를 넣어준다.

                                        charValue /= 2; // charValue를 2로 나눠준다.
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding) //charIndex값이 숨길 데이터의 길이보다 크지 않을 경우
                                    {
                                        B += charValue % 2; //B값에 숨길데이터의 정보를 넣어준다.

                                        charValue /= 2; // charValue를 2로 나눠준다.
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // pixel 에 변경한 R,G,B값을 넣어준다.
                                } break; // 빠져나간다.
                        }

                        pixelElementIndex++; // 다음픽셀로 이동시킨다.

                        if (state == State.Filling_With_Zeros) //  charIndex값이 숨길 데이터의 길이보다 커질경우
                        {
                            zeros++;    //zeros값을 증가시켜준다
                        }
                    }
                }
            }

            return bmp;     // bmp값을 리턴시켜준다.
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;             //ColorUnitIndex값을 0으로 초기화시켜준다.
            int charValue = 0;                  //charValue값을 0으로 초기화시켜준다.

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)        // i값을 bmp.Height만큼 반복시켜준다.
            {
                for (int j = 0; j < bmp.Width; j++)     // j값을 bmp.Width만큼 반복시켜준다.
                {
                    Color pixel = bmp.GetPixel(j, i);      // 각 픽셀중, 지금 데이터정보를 추출할 픽셀을 지정해준다.
                    for (int n = 0; n < 3; n++)         //RGB값에 저장되어있는 데이터값을 추출하기 위해 3번 반복해준다.
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;        //R값에 있는 정보를 추출하여 준다.
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;        //G값에 있는 정보를 추출하여 준다.
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;        //B값에 있는 정보를 추출하여 준다.
                                } break;
                        }

                        colorUnitIndex++;           // colorUnitIndex값을 1증가시켜준다.

                        if (colorUnitIndex % 8 == 0)    // 수업시간때 설명해주셧듯이 8개비트의 따라 데이터정보의 끝인지를 확인해주기 위해서 colorUnitIndex의 값을 8로 나누어 나머지값을 확인하여 준다.
                        {
                            charValue = reverseBits(charValue); // charValue값을 모조리 반대로 바꿔준다.

                            if (charValue == 0)
                            {
                                return extractedText;       // charValue가 0일경우 extractedText를 반환시켜준다.
                            }
                            char c = (char)charValue;   // c에 숨겨진 데이터(문자)정보를 넣어준다.

                            extractedText += c.ToString();  // 추출한데이터(문자)를 extractedText추가시켜준다.
                        }
                    }
                }
            }

            return extractedText;   // 추출한 데이터를 리턴시켜준다.
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
