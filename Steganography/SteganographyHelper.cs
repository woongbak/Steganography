using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding, //메시지를 숨기는 것
            Filling_With_Zeros //메시지를 숨기고 0으로 채움, 숨김의 끝을 알림
        };

        public static Bitmap embedText(string text, Bitmap bmp) //이미지 숨기기 위한 함수(이미지 안에 데이터 숨김)
        {
            State state = State.Hiding; //Hiding 상태로 초기화

            int charIndex = 0; //문자열 인덱스(숨길 텍스트의)

            int charValue = 0; //실제 문자열(숨길 텍스트의)

            long pixelElementIndex = 0; //8비트로 문자열 저장위한 변수

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++) //이미지의 픽셀들 참조
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); //이미지 각각 픽셀 저장

                    R = pixel.R - pixel.R % 2; 
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    // RGB의 LSB를 0으로 설정함

                    for (int n = 0; n < 3; n++) //R,G,B 각각 LSB 에 넣어줌
                    {
                        if (pixelElementIndex % 8 == 0) //8비트까지 허용하고 그다음은 처음으로 초기화
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                                //숨길 문자열을 전부 저장한 상태(0을 모두 저장) 라면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) //G, R이 마지막 비트의 끝인 경우
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));  //LSB가 0인 RGB 설정
                                }

                                return bmp; // Hiding 상태 끝나면 8비트를 0으로 바꿈.
                            }

                            if (charIndex >= text.Length) //문자열 다 숨겼을때 (문자를 저장한 상태)
                            {
                                state = State.Filling_With_Zeros; 
                            }
                            else //숨김 문자열이 남아있다면
                            {
                                charValue = text[charIndex++]; // 인덱스값의 문자열을 저장, 인덱스 1 증가
                            }
                        }

                        switch (pixelElementIndex % 3) //R,G,B
                            
                        {
                            case 0: //R
                                {
                                    if (state == State.Hiding) //인코딩 되있는 상태라면(텍스트가 아직 안 숨겨짐)
                                    {
                                        R += charValue % 2; //0인 LSB 에 1비트를 더함(숨김)
                                        charValue /= 2; //그만큼 비트 밀어줘야함
                                    }
                                } break;
                            case 1: //G
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: //B
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    //R,G,B에 charValue의 LSB 비트값을 더해주고 2를 나눈 값 저장.

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //수정된 R,G,B 반영
                                } break;
                        }

                        //  R,G,B 에 차례대로 1비트씩 저장해서 charValue 값을 0이나 1로 저장한다.
                         
                          

                        pixelElementIndex++; // 

                        if (state == State.Filling_With_Zeros) //문자열을 다 읽은 상태라면 
                        {
                            zeros++; //8비트를 0으로 세팅 
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp) //이미지를 받아 문자열을 추출
        {
            int colorUnitIndex = 0; //픽셀의 RGB에 인덱스를 부여
            int charValue = 0; //문자값 저장

            string extractedText = String.Empty; //추출된 텍스트 저장

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); //픽셀값 가져옴
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) //R,G,B 복구
                            //이진수의 자리값을 원래데로
                            //R,G,B의 LSB 값 charValue에 저장(ASCII 문자열을 만듬)
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

                        if (colorUnitIndex % 8 == 0) //복구가 끝났으면(8비트 모두 추출)
                        {
                            charValue = reverseBits(charValue); //비트의 순서를 반대로 하기 위한 함수

                            if (charValue == 0) //추출한 문자가 0 (마지막 8비트)
                            {
                                return extractedText;
                            } 
                            char c = (char)charValue; //int -> char

                            extractedText += c.ToString(); //추출된 문자열 저장 (텍스트에)
                        }
                    }
                }
            }

            return extractedText;  //저장된 텍스트 출력
        }

        public static int reverseBits(int n) //비트순서 반대로
        {
            int result = 0;

            for (int i = 0; i < 8; i++) //8비트
            {
                result = result * 2 + n % 2;
                //이진수의 자리값 원래데로
                n /= 2;
            }

            return result;
        }
    }
}
