using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State //상태
        {
            Hiding, // 숨김
            Filling_With_Zeros // 0으로 채움
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding; //현재 상태를 숨김 상태로 변경

            int charIndex = 0; //변수 charIndex 선언과 0으로 지정

            int charValue = 0;//변수 charValue 선언과 0으로 지정

            long pixelElementIndex = 0;//변수 pixeIElementIndex 선언과 0으로 지정

            int zeros = 0; //변수 zeros 선언과 0으로 지정

            int R = 0, G = 0, B = 0; //변수 R,G, B 선언과 0으로 지정

            for (int i = 0; i < bmp.Height; i++) // 높이를 처음부터 끝까지 반복
            {
                for (int j = 0; j < bmp.Width; j++) // 너비를 처음부터 끝까지 반복
                {
                    Color pixel = bmp.GetPixel(j, i); //pixel값 지정

                    R = pixel.R - pixel.R % 2; // (원래 R값/2)의 나머지만큼 빼줌
                    G = pixel.G - pixel.G % 2; // (원래 G값/2)의 나머지만큼 빼줌
                    B = pixel.B - pixel.B % 2; // (원래 B값/2)의 나머지만큼 빼줌

                    for (int n = 0; n < 3; n++) // 각 R,G,B 값에 비트 저장
                    {
                        if (pixelElementIndex % 8 == 0) // 8로 나눈 너마지가 0일때
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {// state와 0으로 채운 상태가 8일때
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {//픽셀값-1을 3으로 나눈 나머지가 1 또는 0일때
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }//픽셀 지정 rgb 비트 값으로

                                return bmp;
                            }

                            if (charIndex >= text.Length) 
                            {//텍스트 길이보다 charindex가 큰 경우일때
                                state = State.Filling_With_Zeros; // state값 변경
                            }
                            else
                            {//텍스트길이가 charindex보다 작은경우일때
                                charValue = text[charIndex++]; // 텍스트를 charvalue에 입력 
                            }
                        }

                        switch (pixelElementIndex % 3)//픽셀 인덱스를 3으로 나눈 나머지에 대한 경우
                        {
                            case 0: // 0일때
                                {
                                    if (state == State.Hiding)  //숨김 상태일 때
                                    {
                                        R += charValue % 2; //r값에 charValue/2의 나머지값을 더함
                                        charValue /= 2; // charValue를 2로 나눈 값을 charValue에 저장
                                    }
                                } break;
                            case 1:// 1일때
                                {
                                    if (state == State.Hiding) // 숨김 상태일 때
                                    {
                                        G += charValue % 2; //위와 동일

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: // 2일때
                                {
                                    if (state == State.Hiding)// 숨김 상태일 때
                                    {
                                        B += charValue % 2;// 위와 동일

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//픽셀 지정 rgb 비트 값으로
                                } break;
                        }

                        pixelElementIndex++; // 픽셀인덱스 증가

                        if (state == State.Filling_With_Zeros) 
                        {// state가 0으로 채워진 상태일때
                            zeros++; //제로 1증가
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp)
        { // 이미지에서 text추출하는 함수
            int colorUnitIndex = 0;
            int charValue = 0;
            //변수 선언 후 0 저장
            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)// 높이 처음부터 끝까지 반복
            {
                for (int j = 0; j < bmp.Width; j++)//너비 처음부터 끝까지 반복
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀 값을 받아옴
                    for (int n = 0; n < 3; n++) //R,G,B값을 받기위해 3번 반복
                    {
                        switch (colorUnitIndex % 3) //colorUnitIndex를 3으로 나눈 나머지
                        {//원래 값을 복구하는 식
                            case 0: 
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //charValue는 charValue 두배에 픽셀R값을 2로나눈 나머지를 더한 값.
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // 위와 동일
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // 위와 동일
                                } break;
                        }

                        colorUnitIndex++; // 1증가

                        if (colorUnitIndex % 8 == 0) //  복구된 값을 8로 나눈 나머지가 0일때
                        {
                            charValue = reverseBits(charValue);// 복구 과정에서 역순으로 지정된 비트수를 원래대로 돌려주기 위해 
                                                               // 비트를 뒤집음 
                            if (charValue == 0)
                            {
                                return extractedText; // 복구가 끝나면 리텅
                            }
                            char c = (char)charValue;// int타입을 char로 변환

                            extractedText += c.ToString(); // extractedText(추출된 문자)에 문자 저장
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n) // 위에 사용된 비트를 역순으로 만들어주는 함수
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
