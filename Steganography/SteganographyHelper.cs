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
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++) // input된 이미지의 O(높이*넓이) 작동하는 for 문
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); //가져온 이미지의 (j, i) 에 해당하는 pixel값을 struct에 저장

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2; 
                    B = pixel.B - pixel.B % 2;
                    /*
                     * 각 픽셀의 RGB값에서 0 또는 1을 뺀다. 각 RGB값이 짝수면 0, 홀수면 1을 뺀다.
                     * */
                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)
                            /* pixel의 요소의 인덱스가 8의 배수가 될 때마다 진입하는 if문 
                             * Guess : 아마도 MSB를 변조하기 위한 조건문인듯?            */
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            /* Guess : 이 if문 진입시 return을 하는 것을 보아 추측하건데
                                이미지 파일이 끝나거나 또는 더는 변환할 필요가 없을 때 진입하는듯?*/
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length)
                            {
                                /*charIndex를 통하여 불필요한 for문을 줄이는 trigger
                                 *charIndex는 Input된 text의 index                 */
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                //input된 text를 변수에 저장시켜 나감
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {
                            /*위에서 변조했었던 RGB값을 한번 더 변조하는 switch문*/
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

                                    /*변조한 RGB값을 현재 픽셀에 적용시킴
                                     *for loop이 3번 동작하므로 이 함수는 
                                     *마지막에 항상 동작함*/
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            /*불필요한 for loop을 줄이기 위한 trigger2
                             * 이 if문으로 charIndex가 input Value의 Length를 넘었더라도
                             * 8번정도 for loop이 진행됨
                             */
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

            string extractedText = String.Empty; //string을 초기화

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++) //위에서 설명했듯이 O(h*w)동작
                {
                    Color pixel = bmp.GetPixel(j, i); //위와 동일
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) //RGB값을 위에서 인코딩했던 방식과 정확히 반대로 작동시킴
                        {
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

                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue); 
                            /*
                             * ??어떠한 동작을 하는 함수인지 이해하지 못함?? 
                                변환된 charValue를 원래대로 되돌리는 것이라 추측*/
                            
                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString(); //추출된 value를 string에 더해감
                        }
                    }
                }
            }

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
