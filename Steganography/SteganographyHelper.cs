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

        public static Bitmap embedText(string text, Bitmap bmp) //숨기기 위한 메소드
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0; //픽셀 index 값 선언

            int zeros = 0;

            int R = 0, G = 0, B = 0; //RGB Color 값 선언

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); //이미지 읽어오기
                    // 각 RGB 채널의 하위 1비트를 변조(remove)
                    R = pixel.R - pixel.R % 2; 
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    // 한 픽셀당 세개의 채널존재 (n = 3)
                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {   
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //받은 파일의 RGB 값을 변조
                                }

                                return bmp;
                                // Filling_With_Zeros, 즉 텍스트를 전부 받은 후 
                                // 3 픽셀이 지나면 return bmp (저장후 bitmap 반환)
                            }

                            if (charIndex >= text.Length) //입력받은 텍스트 저장 후 Filling_With_Zeros 초기화
                            {//입력받은 텍스트 저장 후 Filling_With_Zeros 초기화
                                    state = State.Filling_With_Zeros;
                            }
                            else //텍스트를 다 입력받은게 아닌경우 index 값을 더 할당
                            {
                                charValue = text[charIndex++];
                            }
                        }
                        //pixel element index 값에 따른 RGB clolor 값 case 분류 
                        switch (pixelElementIndex % 3)
                        {
                            case 0: //0일 경우 R 채널에 저장
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 1: //1일 경우 G 채널에 저장
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: //2일경우 B채널에 저장
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

            return bmp; //비트맵에 저장 (return)
        }

        public static string extractText(Bitmap bmp) //추출하기 위한 메소드
        {
            int colorUnitIndex = 0;
            int charValue = 0; //삽입할 문자 value 값 

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++) 
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); //픽셀값을 받아오기
                    for (int n = 0; n < 3; n++)
                    {   
                        //각 case 에 따라 R,G,B 컬러 필셀값을 위와 반대로 복구
                        switch (colorUnitIndex % 3) //char value에 2를 곱해주어 추출할 값을 위해 비트를 밀어냄
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

                        //유닛 인덱스에 0을 8개 받았을 경우 if 문으로 들어간다 
                        if (colorUnitIndex % 8 == 0) 
                        {   
                            charValue = reverseBits(charValue);
                            //받은 bit값을 reverse 시켜준다 (반대로 받아왔으므로)
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
        //추출된 문자를 reverse - 추출 후의 result가 반대로 되어 있으므로 
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
