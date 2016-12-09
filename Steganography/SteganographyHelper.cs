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
        }; // 열거형 State

        public static Bitmap embedText(string text, Bitmap bmp) // text 숨기는 메소드
        {
            State state = State.Hiding; 

            int charIndex = 0;

            int charValue = 0; //캐릭터를 인트로 받아와 비트적으로 수정 가능

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;
             // 초기화
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); // 이미지의 높이, 넓이 정수배마다의 pixel값을 받아옴

                    R = pixel.R - pixel.R % 2; 
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2; // 픽셀값중 RGB 값의 마지막 비트(작은비트)의 값(pixel.r%2)을 뺴서 마지막비트의 값을 0으로 만든다. 그곳에 text를 숨기기위해 

                    for (int n = 0; n < 3; n++) //R,G,B 3번
                    {
                        if (pixelElementIndex % 8 == 0) // 처음 시작일때, 8배수 마다 실행 문자하나가 8bit다
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //state가 filling with zero 이고 zeros가 8일때 실행 // 이건 받은 문자를 다 쓰고 마지막 글자 0으로 다 채웟을떄
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)// 8의 배수와 3의배수가 동시에 아닐때(24의배수가 아닐때) 들어간다. 문자 다 썼는데 setpixel을 못해서 여기서 해준다. 
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp; //비트맵 파일 리턴 //pixelElementInde가 3의 배수 일 떄 일로 옴 이건 RGB모두 setpixel을 완료 한거다.
                            }

                            if (charIndex >= text.Length) //문자 다썻을때 fillingwithzeros 스테이트로 바꾼다.
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                charValue = text[charIndex++];
                            } //charValue에 텍스트 받은거 차례로 넣는다.
                        }

                        switch (pixelElementIndex % 3) //
                        {
                            case 0: //R부분 고침
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2; //R 값에 문자의 남은 마지막 비트 넣는다.
                                        charValue /= 2; // shift right
                                    }
                                } break;
                            case 1: //G부분 고침
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;//G 값에 문자의 남은 마지막 비트 넣는다.

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: //B부분 고침
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;//B 값에 문자의 남은 마지막 비트 넣는다.

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros) // 마지막 문자 다음거를 //제로로 채우는 스테이트 일떄 제로로 하나씩 채우고 있는것을 나타냄
                        {
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

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++) 
                {
                    Color pixel = bmp.GetPixel(j, i);// 필셀값 가져옴
                    for (int n = 0; n < 3; n++) // R G B 
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //  char vlaue shift left하고 R 의숨겨진 문자 비트 하나씩 추출 해서 넣는다.
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

                        if (colorUnitIndex % 8 == 0) //  한문자 다 받아왔다.
                        {
                            charValue = reverseBits(charValue); // 이걸 reverse해줘야 받아온 문자로 된다.

                            if (charValue == 0)
                            {
                                return extractedText; //빈문자열일 때 받아온 문자 return 
                            }
                            char c = (char)charValue; // 형변환

                            extractedText += c.ToString(); //스트링으로바꿈
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n) // 리버스해
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
