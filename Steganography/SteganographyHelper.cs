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

        public static Bitmap embedText(string text, Bitmap bmp)//bmp파일에 text를 숨기는 메소드
        {
            State state = State.Hiding;//Hiding상태인지 Filling_With_Zeros인지 구별하는 flag

            int charIndex = 0;//text문자열의 Index값


            int charValue = 0;//text문자열의 문자 값

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)//이미지의 세로크기만큼 탐색
            {
                for (int j = 0; j < bmp.Width; j++)//이미지의 가로크기만큼 탐색
                {
                    Color pixel = bmp.GetPixel(j, i);//이미지의 (j,i) 부분의 픽셀값 대입

                    R = pixel.R - pixel.R % 2;//그 픽셀의 R값이 1이면 0으로 변경하여 저장
                    G = pixel.G - pixel.G % 2;//그 픽셀의 G값이 1이면 0으로 변경하여 저장
                    B = pixel.B - pixel.B % 2;//그 픽셀의 B값이 1이면 0으로 변경하여 저장

                    for (int n = 0; n < 3; n++)//R,G,B LSB값 변경위해 한번씩 순회
                    {
                        if (pixelElementIndex % 8 == 0)//(pixelElementIndex가 0이거나 8일때 ->text 문자 한개를 숨겼을 때
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//text 문자를 다 채웠을 때
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)////  세 번째 픽셀의 B 비트를 가리킬 때
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//(j,i) 위치 픽셀에 현재 RGB값 세팅
                                }

                                return bmp;//모든 text를 숨겼음으로 bmp return.
                            }

                            if (charIndex >= text.Length)
                            {// text를 모두 숨겼거나 마지막 문자 일때 state를 hiding에서 Filling_With_Zero로 변환해준다.
                                state = State.Filling_With_Zeros;
                            }
                            else//문자열이 남아있을경우
                            {
                                charValue = text[charIndex++]; //해당 index의 text 값을 charValue에 대입한다.
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {//R,G,B중 어느 비트에 저장할지
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;//문자의 마지막 비트를 R 비트에 저장
                                        charValue /= 2;//charValue에 2로 나눈 값 저장
                                    }
                                }
                                break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;//문자의 마지막 비트를 G 비트에 저장

                                        charValue /= 2;//charValue에 2로 나눈 값 저장
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;//문자의 마지막 비트를 B 비트에 저장

                                        charValue /= 2;//charValue에 2로 나눈 값 저장
                                    }
                                    // RGB의 LSB값을 모두 변경하여 (j,i) 위치 픽셀에 현재 RGB값 세팅
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
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

            return bmp;//BMP 리턴
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;//추출한 문자 저장 변수

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)//이미지의 세로크기만큼 탐색
            {
                for (int j = 0; j < bmp.Width; j++)//이미지의 가로크기만큼 탐색
                {
                    Color pixel = bmp.GetPixel(j, i);//이미지의 (j,i) 부분의 픽셀값 대입
                    for (int n = 0; n < 3; n++)//R,G,B 각 한번씩
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0://charValue를 한 비트 이동시키고 뒤에 R비트를 붙인다
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                }
                                break;
                            case 1://charValue를 한 비트 이동시키고 뒤에 G비트를 붙인다
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                }
                                break;
                            case 2://charValue를 한 비트 이동시키고 뒤에 B비트를 붙인다
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                }
                                break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0) //text 문자 하나(8비트)를 추출한경우
                        {
                            charValue = reverseBits(charValue);//추출한 문자의 비트를 리버스한다, 추출할 때 숨길때의 반대방향으로 해서

                            if (charValue == 0)//추출한 값이 NULL값(0)이면
                            {
                                return extractedText;//extractedText을 리턴한다
                            }
                            char c = (char)charValue;//읽어온 정수를 문자형으로 변환

                            extractedText += c.ToString();//extractedText에 추출한 문자 c를 이어 붙인다
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
