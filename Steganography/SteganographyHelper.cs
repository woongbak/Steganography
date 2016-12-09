using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State   //state가 숨기는 hiding과 0으로 채우는 Filling_With_Zeros 두가지가 있다
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp) //메시지를 숨기는 함수 :숨기려는 메시지를 text로 받고, 숨기려는 이미지를 bmp로 받는다 
        {
            State state = State.Hiding;    //state를 hiding으로 설정

            int charIndex = 0;            //숨길 text의 index를 가리키는 변수

            int charValue = 0;            //숨길 text의 값을 저장하는 변수

            long pixelElementIndex = 0;   //픽셀을 세는 변수

            int zeros = 0;

            int R = 0, G = 0, B = 0;     //픽셀의 R,G,B 값을 저장할 변수

            for (int i = 0; i < bmp.Height; i++)      
            {
                for (int j = 0; j < bmp.Width; j++)  
                {
                    Color pixel = bmp.GetPixel(j, i);  //이미지의 넓이와 높이만큼 반복문을 돌면서 필셀 값을 가져온다

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;         //RGB의 각 LSB 비트를 0으로 만든다
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)   //(RGB:3회) 반복문 3회 시행
                    {
                        if (pixelElementIndex % 8 == 0)  //가져온 한 문자(8bit)를 다 읽었을 경우 
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)  //state가 Filling_With_Zeros이고 zeros가 8인경우(RGB의 B까지 0으로 채워진 경우)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)  //마지막이 B가 아닐 경우(switch문에서 두번째 case)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//j,i 위치로 픽셀을 세팅
                                }

                                return bmp;  //bmp 반환
                            }

                            if (charIndex >= text.Length)  //가져온 스트링을 문자 길이만큼 다 숨겻을 경우 
                            {
                                state = State.Filling_With_Zeros;  //나머지를 0으로 채운다
                            }
                            else
                            {
                                charValue = text[charIndex++]; //아직 숨길 메시지가 남았을 경우 읽어올 다음 인덱스를 가져온다
                            }
                        }

                        switch (pixelElementIndex % 3) //각 RGB마다 연산
                        {
                            case 0:   //R
                                {
                                    if (state == State.Hiding)    //state가 huiding이라면
                                    {
                                        R += charValue % 2;  //R의 LSB를 저장한다
                                        charValue /= 2; //다음비트로 이동한다
                                    }
                                } break;
                            case 1:    //G
                                {
                                    if (state == State.Hiding)    //숨기는 중이라면
                                    {
                                        G += charValue % 2;  //G의 LSB를 저장한다
                                        charValue /= 2;  //다음 비트로 이동한다
                                    }
                                } break;
                            case 2:   //B
                                {
                                    if (state == State.Hiding)  //숨기는 중
                                    {
                                        B += charValue % 2;  //B의 LSB 저장

                                        charValue /= 2;    //다음 비트로 이동한다
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));   //j,i 위치에 RGB 값을 세팅한다
                                } break;
                        }

                        pixelElementIndex++;   //읽어올 픽셀인덱스 증가

                        if (state == State.Filling_With_Zeros)  //state가 0으로 채우는 상태
                        {
                            zeros++;    //zero 증가
                        }
                    }
                }
            }

            return bmp; //bmp 반환
        }

        public static string extractText(Bitmap bmp)   //히든 메시지를 추출하는 함수
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;   //추출된 메시지를 저장하는 변수

            for (int i = 0; i < bmp.Height; i++)  
            {
                for (int j = 0; j < bmp.Width; j++)    //input 이미지의 높이와 넓이 만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i);   //j,i 위치의 픽셀을 가져온다
                    for (int n = 0; n < 3; n++)   //각 픽셀당 RGB에 대해 3회 반복
                    {
                        switch (colorUnitIndex % 3)  //R,G,B에 대해 switch연산
                        {
                            case 0:  //R
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //메시지를 숨길때와 반대로 각 LSB 값을 더한다
                                } break;
                            case 1:  //G
                                {
                                    charValue = charValue * 2 + pixel.G % 2; 
                                } break;
                            case 2:  //B
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;//인덱스 증가

                        if (colorUnitIndex % 8 == 0)  //한 문자를 다읽었을 경우
                        {
                            charValue = reverseBits(charValue);  //위에서 나온 charValue를 역순으로 저장

                            if (charValue == 0)   //0일경우 (더 읽어올 것이 없을 경우)
                            {
                                return extractedText;   //추출된 메시지를 반환
                            }
                            char c = (char)charValue;  //char형으로 캐스팅

                            extractedText += c.ToString();  //char로 변환된 c를 추출메시지에 추가하다
                        }
                    }
                }
            }

            return extractedText;   //추출된 메시지를 반환한다.
        }

        public static int reverseBits(int n)   //추출을 위해 charValue 역순을 다시 바꿔주는 함수
        {
            int result = 0;

            for (int i = 0; i < 8; i++)  //8비트에 대해 반복문 실행
            {
                result = result * 2 + n % 2;  //left shift 하고 LSB와 더한다

                n /= 2;  //다음으로 이동한다
            }

            return result;
        }
    }
}
