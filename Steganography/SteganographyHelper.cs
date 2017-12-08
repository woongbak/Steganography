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

        public static Bitmap embedText(string text, Bitmap bmp)         //숨김
        {
            State state = State.Hiding;

            int charIndex = 0;              //숨기는 문자의 인덱스 저장

            int charValue = 0;              //숨기는 문자를 정수로 변환시킨후 저장할 변수

            long pixelElementIndex = 0;         //R/G/B의 인덱스를 가지는 변수

            int zeros = 0;              //시행 후 더해진 0의 개수

            int R = 0, G = 0, B = 0;                //rgb변수

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);             //            i,j 좌표에서 지정된 픽셀의 색을 가져온다

                    //             pixel.X%2 ==0아님 1 == 픽셀당 LSB를 변화 == LSB=0
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) // (R1,G1,B1,R2,G2,B2,R3,G3,) 3ㅇ므로
                    {
                        if (pixelElementIndex % 8 == 0) /// 문자 다숨김 (하나) 숨김위치가 끝났다는건 8비트째를 0으로 변환하므로
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 0의개수가 8개, 스테이트가  fillingwith zero 상태일떄
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length)///문자의 인덱스가 문장의 마지막 문자일경우,
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else//아니면 그다음 문자로 넘어감
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)      //RGB각 경우마다 스위치하기 위해 RGBRGBRG == 8개 
                        {
                            case 0:             // R의 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;         // charVALUE의 lsb를 R에 저장
                                        charValue /= 2;     ///  2로 나눔
                                    }
                                } break;
                            case 1:     //G의 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2:// B의 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));// 여기가 마지막이므로, 변경된 RGB를 저장 비트맵에
                                } break;
                        }

                        pixelElementIndex++;  /// 인덱스 증가

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;   // 0 의 개수 증가
                        }
                    }
                }
            }

            return bmp;
        }   ///끝

        public static string extractText(Bitmap bmp)   // 추출
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;    //숨겨진 텍스트 저장용

            for (int i = 0; i < bmp.Height; i++)                //이거 포함 2중포문=== 검사방향 == 가로 -> 세로
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);           //현재 픽셀의 정보 받아옴 (J,I)
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)    //RGB루프
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;        //숨김과 반대, 일단 문자를 정수로 변환한 값에 2배하고, lsb를 더해준다
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

                        colorUnitIndex++;//인덱스 증가

                        if (colorUnitIndex % 8 == 0)   //1문자를 다 조사했다면
                       {
                            charValue = reverseBits(charValue);

                            if (charValue == 0)///문자열 X
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString();      //문자형으로 변환된 값을 추출을 위한 텍스트 변수에 저장
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)//추출시 CHARVAL을 역으로 하기 위한 클래스..
        {
            int result = 0;
            
            for (int i = 0; i < 8; i++)// 1바이트 == 8비트
            {
                result = result * 2 + n % 2;//

                n /= 2;
            }

            return result;
        }
    }
}
