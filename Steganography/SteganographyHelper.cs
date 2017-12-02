using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State //State라는 enum타입 정의 
        {
            Hiding, //0
            Filling_With_Zeros //1
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding; //상태를 0으로 설정.

            int charIndex = 0; //charIndex를 0으로 지정.

            int charValue = 0;//charValue를 0으로 지정.

            long pixelElementIndex = 0; //pixelElementIndex를 0으로 지정.

            int zeros = 0; //zeros를 0으로 지정.

            int R = 0, G = 0, B = 0; // RGB값을 각각 0으로 지정.

            for (int i = 0; i < bmp.Height; i++)//세로
            {
                for (int j = 0; j < bmp.Width; j++)//가로
                {
                    Color pixel = bmp.GetPixel(j, i); //color구조체 변수인 pixel을 선언하고 인자에 해당되는 픽셀을 가져옴.

                    R = pixel.R - pixel.R % 2;  //원래 픽셀값에서 2로나눈 나머지를 빼준뒤 저장. ex)3이면 1을 빼고,2이면 0을뺀다.
                    G = pixel.G - pixel.G % 2;  //원래 픽셀값에서 2로나눈 나머지를 빼준뒤 저장.
                    B = pixel.B - pixel.B % 2;  //원래 픽셀값에서 2로나눈 나머지를 빼준뒤 저장.

                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0) //pixelElementIndex의 8로 나눈 나머지가 0일때,
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//state값이 1이고,zeros값이 8일때,
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)//pixelElementIndex-1의 3의모듈러값이 2보다 작을때,
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//인자에 해당되는 픽셀의 RGB값을 바꾸어준다.
                                }

                                return bmp; //bmp반환.
                            }

                            if (charIndex >= text.Length) //charlndex값이 text의 길이 이상이라면,
                            {
                                state = State.Filling_With_Zeros;//상태를 1로 바꾼다.
                            }
                            else
                            {
                                charValue = text[charIndex++]; //아니면 charValue값을text[charIndex++]로 바꾼다.
                            }
                        }

                        switch (pixelElementIndex % 3)//pixelElementIndex를 3으로 나눈 나머지값을 인자로 하는 switch문.
                        {
                            case 0: //나머지 값이 0일때
                                {
                                    if (state == State.Hiding)//상태가 0일때
                                    {
                                        R += charValue % 2;//R값에 charValue를 2로 나눈 나머지 값을 더해준다.
                                        charValue /= 2;//charValue를 2로 나누어준다.
                                    }
                                } break;//switch 문을 나간다.
                            case 1://나머지 값이 1일때
                                {
                                    if (state == State.Hiding)//상태가 0일때
                                    {
                                        G += charValue % 2;//G값에 charValue를 2로 나눈 나머지 값을 더해준다.

                                        charValue /= 2;//charValue를 2로 나누어준다.
                                    }
                                } break;//switch문을 나간다.
                            case 2://나머지 값이 2일때
                                {
                                    if (state == State.Hiding)//상태가 0일때
                                    {
                                        B += charValue % 2;//R값에 charValue를 2로 나눈 나머지 값을 더해준다.

                                        charValue /= 2;//charValue를 2로 나누어준다.
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//색을 바뀐값으로 바꾸어준다.
                                } break;//switch문을 나간다.
                        }

                        pixelElementIndex++; //pixelElementIndex값을 1 증가 시킨다.

                        if (state == State.Filling_With_Zeros)//상태가 1이라면
                        {
                            zeros++;//zeros값을 증가시킨다.
                        }
                    }
                }
            }

            return bmp;//bmp반환.
        }

        public static string extractText(Bitmap bmp)//Text추출.
        {
            int colorUnitIndex = 0;//colorUnitIndex값을 0으로 선언.
            int charValue = 0;//charValue값을 0으로 선언.

            string extractedText = String.Empty; //빈 문자열임을 나타낸다.

            for (int i = 0; i < bmp.Height; i++)//세로
            {
                for (int j = 0; j < bmp.Width; j++)//가로
                {
                    Color pixel = bmp.GetPixel(j, i);//pixel변수를 bmp변수로 가져온 pixel로 선언.
                    for (int n = 0; n < 3; n++) //n이 0부터2까지
                    {
                        switch (colorUnitIndex % 3)//colorUnitIndex를 3으로 나눈 값을 인자로 하는 switch문
                        {
                            case 0://0일때
                                {
                                    charValue = charValue * 2 + pixel.R % 2;//charValue에다 *2한 값과 pixel의 R값의 모듈러2 값을 넣어준다.
                                } break;//switch문을 나간다.
                            case 1://1일때
                                {
                                    charValue = charValue * 2 + pixel.G % 2;//charValue에다 *2한 값과 pixel의 G값의 모듈러2 값을 넣어준다.
                                } break;//switch문을 나간다.
                            case 2://2일때
                                {
                                    charValue = charValue * 2 + pixel.B % 2;//charValue에다 *2한 값과 pixel의 B값의 모듈러2 값을 넣어준다.
                                } break;//switch문을 나간다.
                        }

                        colorUnitIndex++;//coloUnitIndex값을 1 증가 시킨다.

                        if (colorUnitIndex % 8 == 0)//colorUnitIndex의 8모듈러값이 0이면
                        {
                            charValue = reverseBits(charValue);//reverseBits에 넣은 값을 넣어준다.

                            if (charValue == 0)//charValue값이 0이면
                            {
                                return extractedText;//extractedText값을 반환.
                            }
                            char c = (char)charValue;//charValue값을 문자열로 바꾼후 c에저장.

                            extractedText += c.ToString();//extractedText에 문자열로 저장한c값을 저장.
                        }
                    }
                }
            }

            return extractedText;//extractedText를 반환.
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
