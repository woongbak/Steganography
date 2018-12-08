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
        
        // 입력된 텍스트를 이미지에 숨기는 함수 생성
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            // 전체 BMP의 영역에 대하여
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    // 각각의 픽셀을 얻음
                    Color pixel = bmp.GetPixel(j, i);
                    // 픽셀의 RGB 값이 홀수면 최좌측 비트를 0으로 바꾸고 짝수면 그대로 
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    // 세 번의 반복문
                    for (int n = 0; n < 3; n++)
                    {
                        // index가 8의 배수이면
                        if (pixelElementIndex % 8 == 0)
                        {
                            // state가 Filling_With_Zeros 객체에 머물러있고 zeros라는 변수의 값이 8이면
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                // (index - 1 ) mod 3 ≡ 0, 1인 인덱스에 한하여
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    // 픽셀을 위에서 설정한 RGB 값으로 바꿔준다
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            // 입력된 텍스트의 길이보다 charIndex가 더 크면
                            if (charIndex >= text.Length)
                            {
                                // state는 State의 Filling_With_Zeros 객체를 가리킨다
                                state = State.Filling_With_Zeros;
                            }
                            // 입력된 텍스트의 길이가 더 큰 경우라면
                            else
                            {
                                // charValue에다가 text 배열의 현재 인덱스 값을 넣어주고 charIndex를 ++ 해준다
                                charValue = text[charIndex++];
                            }
                        }

                        // pixelElementIndex mod 3 값이
                        switch (pixelElementIndex % 3)
                        {
                            // 0이라면    
                            case 0:
                                {
                                    // state가 State의 Hiding 객체를 가리키고 있다면
                                    if (state == State.Hiding)
                                    {
                                        // text[charIndex]의 값을 가지고 있는 charValue를 2로 나눈 나머지를 R에다가 더하고 charValue를 2로 나눠준다
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            // 1이라면    
                            case 1:
                                {
                                    // state가 State의 Hiding 객체를 가리키고 있다면
                                    if (state == State.Hiding)
                                    {
                                        // text[charIndex]의 값을 가지고 있는 charValue를 2로 나눈 나머지를 G에다가 더하고 charValue를 2로 나눠준다
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            // 2라면    
                            case 2:
                                {
                                    // state가 State의 Hiding 객체를 가리키고 있다면
                                    if (state == State.Hiding)
                                    {
                                        // text[charIndex]의 값을 가지고 있는 charValue를 2로 나눈 나머지를 B에다가 더하고 charValue를 2로 나눠준다
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }
                                    // 해당 RGB를 가지고 픽셀을 set 해준다
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }
                        // index를 +1 해준다
                        pixelElementIndex++;

                        // state가 State의 Filling_With_Zeros를 가리키고 있다면
                        if (state == State.Filling_With_Zeros)
                        {
                            // zeros변수를 ++ 
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
                    // 제공된 사진의 픽셀을 얻음
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        // colorUnitIndex mod 3의 값이
                        switch (colorUnitIndex % 3)
                        {
                            // 0일 경우
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            // 1일 경우    
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            // 2일 경우
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;

                        // colorUnitIndex mod 8이 0이면
                        if (colorUnitIndex % 8 == 0)
                        {
                            // charValue의 정렬순서를 바꿔주고
                            charValue = reverseBits(charValue);

                            // 더 이상 바꿔줄 bit가 없으면 그대로 출력
                            if (charValue == 0)
                            {                               
                                return extractedText;
                            }
                            // charValue를 char으로 형식 변환시킨뒤 extractedText에 String 형식으로 저장
                            char c = (char)charValue;

                            extractedText += c.ToString();
                        }
                    }
                }
            }

            return extractedText;
        }

        // 입력값의 bit의 정렬순서를 앞에서 뒤로 바꿔주는 함수
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
