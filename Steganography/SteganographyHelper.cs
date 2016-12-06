using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding, //숨기는 상태
            Filling_With_Zeros //0으로 채우는 상태
        };

        public static Bitmap embedText(string text, Bitmap bmp) //데이터를 숨기기 위한 메소드
        {
            State state = State.Hiding; //state를 숨기는 상태로 셋팅

            int charIndex = 0; //문자열 인덱스

            int charValue = 0; //현재 문자열의 아스키값 저장하는 변수

            long pixelElementIndex = 0; //한문자를 8개의 LSB에 숨기기위한 인덱스

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지의 크기만큼 이중 for문
                {
                    Color pixel = bmp.GetPixel(j, i); //현재 위치의 픽셀 값을 얻음

                    R = pixel.R - pixel.R % 2; // R의 LSB비트를 0으로 셋팅

                    G = pixel.G - pixel.G % 2; // G의 LSB비트를 0으로 셋팅
                    B = pixel.B - pixel.B % 2; // B의 LSB비트를 0으로 셋팅

                    for (int n = 0; n < 3; n++) // R,G,B를 위한 반복문
                    {
                        if (pixelElementIndex % 8 == 0) //인덱스가 8의 배수일경우(한문자 다 숨겼을 경우)
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //state가 filling_with_zeros 상태이고 zeros 가 8이면 (문자열 저장이 다 끝나고 끝을 표시하기 위해 8개의 LSB값을 0으로 셋팅했을 경우)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) //마지막이 B가 아니어서 픽셀이 셋팅되지 못한 경우,
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //픽셀 셋팅
                                }

                                return bmp; //리턴
                            }

                            if (charIndex >= text.Length) //텍스트를 다 숨겼을 경우
                            {
                                state = State.Filling_With_Zeros; //Filling_with_Zeros 상태로 전환
                            }
                            else //아직 덜 숨겼을 경우
                            {
                                charValue = text[charIndex++]; //charValue에 다음 텍스트의 아스키값 저장
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {
                            case 0: //R의 경우
                                {
                                    if (state == State.Hiding) //Hiding 중이면
                                    {
                                        R += charValue % 2; //charValue(문자열 아스키 저장한 변수) 값의 LSB를 R에 저장
                                        charValue /= 2; // charValue 2로 나누고 저장
                                    }
                                } break;
                            case 1: //G의 경우
                                {
                                    if (state == State.Hiding) //Hiding 중이면
                                    {
                                        G += charValue % 2; //charValue의 LSB를 G에 저장

                                        charValue /= 2; //charValue 2로 나누고 저장
                                    }
                                } break;
                            case 2: //B의 경우
                                {
                                    if (state == State.Hiding) //Hiding 중이면
                                    {
                                        B += charValue % 2; //charValue의 LSB를 G에 저장

                                        charValue /= 2; //charValue 2로 나누고 저장
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //위에서 셋팅한 R,G,B값을 픽셀에 저장
                                } break;
                        }

                        pixelElementIndex++; //인덱스 1증가시킴

                        if (state == State.Filling_With_Zeros) //텍스트 다 저장하고 0으로 채우는 중이면
                        {
                            zeros++; //zeros 증가시킴 
                        }
                    }
                }
            }

            return bmp; //리턴
        }

        public static string extractText(Bitmap bmp) //텍스트를 추출하는 함수
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; //추출된 텍스트 저장하기 위한 변수

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++) //이미지 픽셀을 순차적으로 돔
                {
                    Color pixel = bmp.GetPixel(j, i); //해당하는 픽셀의 값 얻어옴
                    for (int n = 0; n < 3; n++) //R,G,B 3번 반복
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0: //R이면
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // 현재 char Value에 2를 곱하고(1만큼 좌 shift), R의 LSB 더함
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2; //1만큼 좌 shift 후 G의 LSB 더함
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2; //1만큼 좌 shift 후 B의 LSB 더함
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0) //인덱스가 8인경우(하나의 문자 아스키값 다 추출했을 경우)
                        {
                            charValue = reverseBits(charValue); //현재 charValue는 역순으로 되어있으므로 charValue를 역순으로 돌려 원래 아스키 값 만듬

                            if (charValue == 0) //charValue가 0이면 문자열이 더이상 없다는 의미이므로
                            {
                                return extractedText; //리턴
                            }
                            char c = (char)charValue; //charValue의 아스키 값을 문자로 저장

                            extractedText += c.ToString(); //바꾼 문자를 텍스트 변수에 추가
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n) //추출할 때 charValue를 역순으로 뒤집기 위한 메소드
        {
            int result = 0;

            for (int i = 0; i < 8; i++) //8번 반복(8자리이므로)
            {
                result = result * 2 + n % 2; //좌shift하고 charValue의 LSB 더함

                n /= 2; //charValue 2로 나누고 저장
            }

            return result;
        }
    }
}
