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
        public static Bitmap embedText(string text, Bitmap bmp)//Hide기능
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)//이미지 파일의 높이를 0부터 끝까지 살핀다.
            {
                for (int j = 0; j < bmp.Width; j++)//이미지 파일의 너비를 0부터 끝까지 살핀다.
                {
                    Color pixel = bmp.GetPixel(j, i);//픽셀을 받는다.

                    R = pixel.R - pixel.R % 2;//RGB 각각 빨강,초록,파랑을 원래의 값에서 2로나눈 나머지를 빼고 각각 RGB에 저장한다.
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)//pixelElementIndex를 8로나눈나머지가 0이면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//state가 Filling_With_zeros와 같고 zeros가 8이면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)//pixelElementIndex를 8로나눈 나머지가 0임과 동시에 1을 빼고 3으로 나눈나머지가 2보다 작으면
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//추출될 이미지파일의 픽셀에 RGB값을 지정.
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length)//charIndex가 텍스트의 text의 length보다 크면
                            {
                                state = State.Filling_With_Zeros;//state를 Filling_With_Zeros로 저장.
                            }
                            else
                            {
                                charValue = text[charIndex++];//그렇지않으면 charValue를 text의 배열중 charIndex+1한 위치의 있는 값으로 지정
                            }
                        }

                        switch (pixelElementIndex % 3)//pixelElementIndex을 3으로 나눈 나머지가 각각 0이면 R 1이면 G 2이면 B를 조작
                        {
                            case 0://0이면
                                {
                                    if (state == State.Hiding)//state가 Hiding이면
                                    {
                                        R += charValue % 2;//R에 charValue를 2로 나눈 나머지를 저장.
                                        charValue /= 2; //charValue를 2로 나눈다.
                                    }
                                } break;
                            case 1://1이면
                                {
                                    if (state == State.Hiding)//state가 Hiding이면
                                    {
                                        G += charValue % 2;//G에 charValue를 2로 나눈 나머지를 저장.

                                        charValue /= 2;//charValue를 2로 나눈다.
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)//state가 Hiding이면
                                    {
                                        B += charValue % 2;//B에 charValue를 2로 나눈 나머지를 저장.

                                        charValue /= 2;//charValue를 2로 나눈다.
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//조작한 후 Pixel에 저장
                                } break;
                        }

                        pixelElementIndex++;//pixelElementIndex를 1더한다.

                        if (state == State.Filling_With_Zeros)//모든 연산이 끝난 후 state가 Filling_With_Zeros이면
                        {
                            zeros++;//zeros에 1을 더한다.
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp)//Extract기능
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;//extractedText를 Empty로 저장

            for (int i = 0; i < bmp.Height; i++)//Embedtext와 마찬가지로 높이와 너비 모두 살핀다
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);//pixel을 지정하고
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)//colorUnitIndex를 3으로 나눈 나머지를 조사한다
                        {
                            case 0://Embed에서 나눈 나머지가 0이면 R이므로
                                {
                                    charValue = charValue * 2 + pixel.R % 2;//Embed에서 charValue를 2로 나눈값과 R을 나눈 나머지를 뺏으므로 역연산으로해준다
                                } break;
                            case 1://Embed에서 나눈 나머지가 1이면 B이므로
                                {
                                    charValue = charValue * 2 + pixel.G % 2;//역연산
                                } break;
                            case 2://Embed에서 나눈 나머지가 2이면 B이므로
                                {
                                    charValue = charValue * 2 + pixel.B % 2;//역연산
                                } break;
                        }

                        colorUnitIndex++;//colorUnitIndex를 1증가시킨다.

                        if (colorUnitIndex % 8 == 0)//colorUnitIndex를 나눈나머지가 0이면
                        {
                            charValue = reverseBits(charValue);//charValue를 reverseBits함수를 이용해 반환해준다.

                            if (charValue == 0)//반환받은값이 0이면
                            {
                                return extractedText;//추출된 텍스트를 반환한다.
                            }
                            char c = (char)charValue;//charValue의 값을 char형으로 받아 c에 저장

                            extractedText += c.ToString();//extractedText에 char형으로 반환받은 텍스트를 저장.
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)//extractText함수에 사용되는 함수
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;//result를 2곱한값과 입력받은 n을 2로나눈나머지를 더하고

                n /= 2;//n을 2로 나눈다.
            }

            return result;//연산을 마친 result를 변환
        }
    }
}
