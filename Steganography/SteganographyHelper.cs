using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper       // steganographyHelper 라는 클래스를 지정
    {
        public enum State           // state 안의 요소들 Hiding 과 Filling_With_Zeros 를 설정 
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp)     // 주어진 텍스트를 숨기는 method  
        {
            State state = State.Hiding;                             // 상태를 Hiding 으로 설정하고

            int charIndex = 0;      

            int charValue = 0;          

            long pixelElementIndex = 0; 

            int zeros = 0;                      

            int R = 0, G = 0, B = 0;                // 필요한 변수들 선언 및 초기화

            for (int i = 0; i < bmp.Height; i++)        
            {
                for (int j = 0; j < bmp.Width; j++)     //  2차원 그림의 1*1 단위 비트맵값 하나에 대하여...
                {
                    Color pixel = bmp.GetPixel(j, i);   //  R/G/B 값을 받아온다.

                    R = pixel.R - pixel.R % 2;      
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;          // 모든 픽셀의 값 RGB 의 LSB 를 0으로 만들어주는 과정

                    for (int n = 0; n < 3; n++)         // 한픽셀당 밑에 과정들 3번씩 반복
                    {
                        if (pixelElementIndex % 8 == 0)     // pixelElementIndex가 8의 배수일때마다  => 이유: char 형 변수가 크기가 1바이트 이기 때문에 (2^8-1 개 가지수)
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)        //  현 상태가 Filling_With_Zeros 상태이지만 zero 가 8이 되었다면(state == State.Filling_With_Zeros 가 8번 채워졌다면),
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)        // pixel 이 3의 배수번째가 아닐때 (3k+1 , 3k+2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));        // 위에서 설정한 RGB 값을 새로운 비트값으로 설정 
                                }

                                return bmp;         // 비트값 돌려줌
                            }

                            if (charIndex >= text.Length)
                            {
                                state = State.Filling_With_Zeros;       // charIndex 가 텍스트 길이보다 길다면 state 를 Filling_With_Zeros 로 변화시킴
                            }
                            else
                            {
                                charValue = text[charIndex++];          // 아니라면 text[charIndex]의 아스키값을 charValue 로 받음, 이후 charIndex 1 증가시킴
                            }
                        }

                        switch (pixelElementIndex % 3)                        
                        {
                            case 0:                              // 그 사이 3의 배수번째 pixel 이라면 
                                {
                                    if (state == State.Hiding)  
                                    {
                                        R += charValue % 2;     // R 값의 LSB 변화 시키는 항, charValue 가 홀수이면 변화시킴   
                                        charValue /= 2;         // 이후 charValue 를 2로 나눔
                                    }
                                } break;
                            
                            case 1:                             //  3으로 나눈 나머지가 1 인 pixel 이라면
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;     // G 값의 LSB 변화 시키는 항, charValue 가 홀수이면 변화시킴 

                                        charValue /= 2;         // charValue 를 2로 나눈 몫
                                    }
                                } break;

                            case 2:                             // 3으로 나눈 나머지가 2 이라면
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;     // B 값의 LSB 변화 시키는 항, charValue 가 홀수이면 변화시킴

                                        charValue /= 2;         // charValue 를 2로 나눈 몫
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));   // 3으로 나눈 나머지가 2일때 한픽셀당 3번 반복한 결과들로 픽셀값을 재설정 해줌
                                } break;
                        }

                        pixelElementIndex++;                                    // 한픽셀당 3번 수앻됨  

                        if (state == State.Filling_With_Zeros)                  //  Filling_With_Zeros 상태면
                        {
                            zeros++;                                            // zeros 변수만 증가시킴.
                        }
                    }
                }
            }

            return bmp;         // 비트값이 모두 재 설정되었다면 리턴해준다
        }

        public static string extractText(Bitmap bmp)        // 안에있는 텍스트를 추출하는 method
        {
            int colorUnitIndex = 0;                         // 색깔 단위 인덱스
            int charValue = 0;                              // 문자의 아스키값

            string extractedText = String.Empty;            // string.empty 필드, 추출시 텍스트를 담아놓는 변수로 현재 빈문자열로 초기화했음. 

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)             // 1*1 단위 비트맵으로 나눈 것에 대하여
                {
                    Color pixel = bmp.GetPixel(j, i);           // 비트맵값을 받음 
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)             // 위의 embed Text 의 switch 문과 반되되는 과정
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;     
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;  //
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;    
                                } break;
                        }

                        colorUnitIndex++;           // 위의 pixelElementIndex 와 같은 의미로 사용

                        if (colorUnitIndex % 8 == 0)    // colorUnitIndex 이 8의 배수이면 
                        {
                            charValue = reverseBits(charValue); // charValue 복귀

                            if (charValue == 0)                 
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;           // 복귀된 아스키값을 character 형태로 받아 저장

                            extractedText += c.ToString();      
                        }
                    }
                }
            }

            return extractedText;                   // 이렇게 모여진 string 반환
        }

        public static int reverseBits(int n)        
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;        // char value 값 복귀 과정

                n /= 2;
            }

            return result;
        }
    }
}
