using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State           // 상태를 체크 하기 위해
        {
            Hiding,
            Filling_With_Zeros
        };
        //숨김원리
        //BMP - bitmap 형식으로 하나의 픽셀을 배열형태로 정보를 가지고 있다.
        //전체적으로 루프를 돌며, RGB 값을 얻는다.
        //각 RGB 값의 LSB를 0으로 셋팅, 0으로 셋팅한 LSB 부분을 텍스트를 숨기는 곳으로 활용한다.
        //Text 문자를 정수로 변환 후 변환된 값을 픽셀에 인코딩.


        public static Bitmap embedText(string text, Bitmap bmp) // Embed 함수
        {
            State state = State.Hiding; //숨기기 위해, State를 Hiding으로 설정

            int charIndex = 0; //숨기기 위한 스트링 Index

            int charValue = 0; // Text - int형 변환

            long pixelElementIndex = 0;//Pixel Index

            int zeros = 0; //모든 스트링 Embed 후, 8Bit를 0으로 셋팅 하기 위한 변수

            int R = 0, G = 0, B = 0; //RGB

            for (int i = 0; i < bmp.Height; i++) //반복 : bmp의 높이
            {
                for (int j = 0; j < bmp.Width; j++) //반복 : bmp 넓이
                {
                    Color pixel = bmp.GetPixel(j, i); //bmp 포맷으로 부터 픽셀 값을 가져온다.

                    R = pixel.R - pixel.R % 2; //R : 마지막 비트 값을 0으로 셋팅
                    G = pixel.G - pixel.G % 2; //G : 마지막 비트 값을 0으로 셋팅
                    B = pixel.B - pixel.B % 2; //B : 마지막 비트 값을 0으로 셋팅
                                               //각 RGB 값의 LSB를 0으로 셋팅하는 과정.

                    for (int n = 0; n < 3; n++) //RGB 3가지 수행
                    {
                        if (pixelElementIndex % 8 == 0) //8bits, 해당 한 문자 처리 했을 경우 조건.
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //모든 문자 숨긴 후 -> State.Filling_With_Zeros / zeros = 8 조건의 경우.
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // case 0,1로
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //setpixel
                                }

                                return bmp; //bmp 리턴
                            }

                            if (charIndex >= text.Length) //텍스트 길이에 도달했을경우, 다 숨겼을 경우
                            {
                                state = State.Filling_With_Zeros; //State_Filling_with_Zeros로 셋팅
                            }
                            else
                            {
                                charValue = text[charIndex++]; //텍스트에 대한 인덱스 증가.
                            }
                        }

                        switch (pixelElementIndex % 3)                           // pixelElementIndex Mod 3을 통해, R/G/B 3가지 경우로 나눔.
                        {                                                        // R,G,B 채널의 0으로 셋팅된 LSB bit를 이용하여 Text 값을 변환한 값인 Charvalue를 Embed 시키는 과정
                            case 0:                             //R의 경우       
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;     // 0으로 셋팅된 R채널 LSB에 0 아니면 1 셋팅
                                        charValue /= 2;         // 문자의 1bit 입력 후, 다음 채널에서 활용하기 위해 1bit 이동.
                                    }
                                }
                                break;
                            case 1:                              //G의 경우(R의 경우와 같음.)
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 2:                                //B의 경우(R의 경우와 같음.)
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //한 픽셀, RGB 처리 완료, 따라서 반영.
                                }
                                break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros) //모든 Text를 숨긴 상황, State가 Filling_with_Zeros로 설정된 이후에 8bit 0으로 만들어 주기 위한 zeros 증가.
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp; //bmp 리턴
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0; //픽셀 인덱스
            int charValue = 0; //숨겨진 Text 저장 변수

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)            //높이
            {
                for (int j = 0; j < bmp.Width; j++)         //넓이
                {
                    Color pixel = bmp.GetPixel(j, i);       //픽셀 가져오기.

                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)         //R,G,B 값의 LSB 추출 (숨김과정역으로)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                }
                                break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                }
                                break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                }
                                break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)    //8bit 추출된 경우, reverseBits 함수를 이용하여, 뒤집어져있던 비트를 바꿔줌.
                        {
                            charValue = reverseBits(charValue);

                            if (charValue == 0)         //이미지 삽입 종료
                            {
                                return extractedText;   //텍스트 리턴.
                            }
                            char c = (char)charValue; //형변환

                            extractedText += c.ToString(); //변환된 값 스트링에 누적.
                        }
                    }
                }
            }

            return extractedText; //텍스트 리턴
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
