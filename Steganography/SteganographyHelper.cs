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
        /// <summary>
        /// 이미지속에 텍스트를 숨기는 함수
        /// </summary>
        /// <param name="숨길 문장"></param>
        /// <param name="숨길 이미지"></param>
        /// <returns></returns>
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
                    //픽셀단위로 데이터를 가져옵니다.
                    Color pixel = bmp.GetPixel(j, i);

                    //각 RGB채널의 하위 1bit를 제거합니다.(0으로 만듬)
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    //1pixel 에 3채널이 존재하므로 3번 돌려준다.
                    for (int n = 0; n < 3; n++)
                    {
                        //char은 8bit -> pixelElementIndex % 8 == 0 이면 새로운 문자를 받아오거나
                        if (pixelElementIndex % 8 == 0)
                        {
                            /*
                             * State.Filling_With_Zeros -> 텍스트를 다 채움
                             * zeros == 8 -> text를 다 채운 상태에서 3pixel 지나면
                             * 비트맵에 저장 후 비트맵을 반환하면서 함수종료
                             */
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }
                            //텍스트를 이미지에 다 저장하면 state 를 Filling_With_Zeros 바꿔준다.
                            if (charIndex >= text.Length)
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                charValue = text[charIndex++];
                            }
                        }
                        /*
                         * pixelElementIndex == 0 이면 R 채널에 문자 저장.
                         * 1 이면 G, 2이면 B채널에 저장한다.
                         * state 가 hiding 이면 해당 채널에 문자열의 하위 1bit를 넣고 문자열값을 넣은만큼 1bit 줄인다.
                         * pixelElementIndex == 2 면 B 채널까지 다 채웠으므로 비트맵에 다시 저장해준다.
                         */
                        switch (pixelElementIndex % 3)
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
        /// <summary>
        /// 이미지에서 숨겨진 문장을 추출한다.
        /// </summary>
        /// <param name="숨겨진 문장이 존재하는 이미지"></param>
        /// <returns></returns>
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    //각 color 채널의 하위 1bit를 더한다.
                    //charValue * 2를 해줘서 비트로 볼때 왼쪽으로 쉬프트 해준다.
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
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
                            //저장한 순서처럼 받아왔으므로 낮은 비트가 현재는 높은 비트로 되있기 떄문에 역전시켜준다. 
                            charValue = reverseBits(charValue);
                            /* 저장할 떄 마지막에 3pixel,즉 문자 1개만큼 0을 넣어줬다.
                             * 그 8개의 0을 받으면서 charValue * 2를 해주면 오버플로우로 결국 0이되어 버린다.
                             */
                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString();
                        }
                    }
                }
            }

            return extractedText;
        }
        //추출된 문자는 bit가 거꾸로 되있기에 역전시켜주는 함수가 필요.
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
