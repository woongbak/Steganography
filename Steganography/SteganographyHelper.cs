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

        // 숨김원리
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;

            int charIndex = 0; 

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            /* RGB 값 추출 
             가장 일반적으로 스테가노그래피 수정은 최하위 비트(LSB)를 고친다. 
             이 비트를 0에서 1로, 1에서 0으로 바뀌고 이 비트가 복원되면 원래의 숨겨진 메시지가 나타난다. 
             RGB로 색을 표현할 때 24비트 색상 팔레트로 표현한다.
             RED, GREEN, BLUE 8비트씩 각각 256가지의 레드, 그린, 블루를 표현한다.
             LSB를 수정하였을 경우 우리 눈으로 차이점을 
             찾아내기 매우 어렵기 때문에 데이터를 숨기기에 
             적합한 곳으로 많이 이용된다. */
            for (int i = 0; i < bmp.Height; i++) // 높이
            {
                for (int j = 0; j < bmp.Width; j++) //너비
                {
                    Color pixel = bmp.GetPixel(j, i); //그림을 좌표로 생각하고 가져옴,  해당 System.Drawing.Bitmap의 지정된 픽셀의 색을 가져옴

                    // RGB 값 추출 
                    R = pixel.R - pixel.R % 2; 
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) //rgb 하나하나씩해서 3번 반복
                    {
                        if (pixelElementIndex % 8 == 0) // 8비트 끝(LSB)에 숨기므로 그 때 일때만 수행
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //숨기는 곳과 0으로 채우는 것이 같고 zeros가 8일때 수행
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) //pixelElementIndex = 1,2,4,5,7,8 (3,6빼고 수행)  
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length) //char 인데스가 입력 글자보다 커지면 hiding 부분을 zero로 채우기(state = State.Filling_With_Zeros)
                            {
                                state = State.Filling_With_Zeros; 
                            }
                            else 
                            {
                                charValue = text[charIndex++];
                            }
                        }


                        switch (pixelElementIndex % 3) // 1,2,3,4,5,6,7,8
                        {
                            case 0: //R 값 수정
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 1: //G 값 수정
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2://B 값 수정
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


        // 추출원리
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++) //그림의 높이
            {
                for (int j = 0; j < bmp.Width; j++) // 그림의 너비
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) // 숨김과 반대로 해줌
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

                            if (charValue == 0) //끝까지 다 추출하면 추출된 글자 반환
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

        public static int reverseBits(int n) //bit을 반대로 
        {
            int result = 0;

            for (int i = 0; i < 8; i++) // 8bit 까지 쭉 돌려줌
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }
    }
}
