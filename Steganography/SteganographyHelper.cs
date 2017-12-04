using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        //Hiding과 Filling_with_Zeros 에 0과 1값을 넣는다.
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        };

        //숨기기 위한 메소드
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            //사진 비트맵의 높이만큼 for문을 돌린다.
            for (int i = 0; i < bmp.Height; i++)
            {
                //사진 비트맵의 너비만큼 for문을 돌린다. 
                for (int j = 0; j < bmp.Width; j++)
                {
                    //pixel이라는 Color 클래스의 객체를 만들어서 각 픽셀에 대한 값을 저장
                    Color pixel = bmp.GetPixel(j, i);

                    R = pixel.R - pixel.R % 2;  //픽셀의 R값을 0으로 초기화한다.
                    G = pixel.G - pixel.G % 2;  //픽셀의 G값을 0으로 초기화한다.
                    B = pixel.B - pixel.B % 2;  //픽셀의 B값을 0으로 초기화한다.

                    
                    //픽셀이 0~255까지의 자리 숫자인데 100의 자리 숫자를 바꾸면 색깔이 많이 변하기 때문에 10의 자리와 1의 자리만 바꿔주는 작업
                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0) //R1, G1, B1, R2, G2, B2, R3, G3 의 8비트를 숨긴다.
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)    //10의 자리, 1의 자리이면 1의자리 숫자가 0으로 초기화 된 값을 입력
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp; //변경한 bmp값을 return 해준다.
                            }

                            //charIndex가 String인 txt의 길이보다 크면
                            if (charIndex >= text.Length)
                            {
                                state = State.Filling_With_Zeros;   //state를 Hiding에서 Filling_With_Zeros로 변경
                            }
                            //작으면
                            else
                            {
                                charValue = text[charIndex++];  //charValue에 text[charIndex]값을 숨긴다. charIndex가 꽉 찰때까지 숨기는 것!
                            }
                        }

                        //pixelElementIndex값을 3으로 나눈 나머지에 대해서 각각 수행을 한다.
                        switch (pixelElementIndex % 3)
                        {
                            //최하위 비트가 0일때
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2; //레드값에 대해서 변화를 줌
                                        charValue /= 2;
                                    }
                                } break;
                            //최하위 비트가 1일때
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; //그린값에 대해서 변화를 줌
                                        charValue /= 2;
                                    }
                                } break;
                            //최하위 비트가 2일때
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2; //블루 값에 대해서 변화를 줌

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    //변화를 준 RGB값을 사진 픽셀에 대입한다.
                                } break;
                        }
                        //변화를 줄때마다 pixelElementIndex 값 증가
                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;     //변화된 사진을 돌려준다.
        }

        //추출하기 위한 메소드
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;
                
            //사진의 높이의 픽셀만큼 반복
            for (int i = 0; i < bmp.Height; i++)
            {
                //사진의 너비의 픽셀만큼 반복
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);   // 사진 픽셀값을 받는다.
                    for (int n = 0; n < 3; n++)
                    {
                        //각각 RGB값에 대해서 숨긴 원리를 반대로 적용하여 실행한다.
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

                        if (colorUnitIndex % 8 == 0)    //8비트에 대해서 작업이 끝날때마다 수행
                        {
                            charValue = reverseBits(charValue); //charValue에 대해서 거꾸로 연산해준다.

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

            return extractedText;   //발견한 text문자 리턴
        }

        //리버싱 작업을 해주는 메소드
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
