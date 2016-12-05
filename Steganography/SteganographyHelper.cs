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

        public static Bitmap embedText(string text, Bitmap bmp) //숨길 메세지와 이미지를 인자로 받는 함수
        {
            State state = State.Hiding;  //하이딩 상태로 초기화해줌

            int charIndex = 0; //문자열 인덱스

            int charValue = 0; //문자열을 하나씩 아스키형으로 바꿔주기 위한 변수

            long pixelElementIndex = 0; //8비트 문자저장을 위해 8단위로 끊을 변수

            int zeros = 0; //문자저장을 다하고 LSB가 0인 상태로 한 글자를 넣어줄 때 세는 변수

            int R = 0, G = 0, B = 0; //RGB값저장을 위한 변수 

            for (int i = 0; i < bmp.Height; i++) //이미지의 높이만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++) //이미지의 넓이만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i); //각 픽셀값을 pixel에 저장

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2; //RGB값을 얻어 LSB를 0으로 set해줌

                    for (int n = 0; n < 3; n++) //RGB의 각 LSB에 넣어주기 위해 3번 반복
                    {
                        if (pixelElementIndex % 8 == 0) //픽셀인덱스의 끝에 다다르면 다시 처음으로 초기화
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //문자를 다 저장해서 상태가 filling_with_zeros로 세팅되고 LSB가 0인채로 8번을 저장한 후일 때
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) //문자저장을 다하고 스위치구문의 B경우를 거치지 못하는 경우
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //LSB가 0인 RGB 세팅을 여기서 해줌
                                }

                                return bmp; //이미지를 반환
                            }

                            if (charIndex >= text.Length) //문자를 다 변환해 저장한 상태이면
                            {
                                state = State.Filling_With_Zeros; //상태를 바꿔줌
                            }
                            else //아니면
                            {
                                charValue = text[charIndex++]; //인덱스값의 문자를 아스키 코드로 변환해 저장하고 인덱스를 1증가
                            }
                        }

                        switch (pixelElementIndex % 3) //RGB의 세가지 경우를 나눠줌
                        {
                            case 0: //R의 경우
                                {
                                    if (state == State.Hiding) //인코딩상태이면
                                    {
                                        R += charValue % 2; //R의 값에 charValue의 LSB비트를 더해주고

                                        charValue /= 2; //2를 나눠저장->2진수 구하는 과정과 같음
                                    }
                                } break;
                            case 1: //G의 경우
                                {
                                    if (state == State.Hiding) //인코딩상태이면
                                    {
                                        G += charValue % 2; //R의 값에 charValue의 LSB비트를 더해주고

                                        charValue /= 2; //2를 나눠저장->2진수 구하는 과정과 같음
                                    }
                                } break;
                            case 2: //B의 경우
                                {
                                    if (state == State.Hiding) //인코딩상태이면
                                    {
                                        B += charValue % 2; //R의 값에 charValue의 LSB비트를 더해주고

                                        charValue /= 2; //2를 나눠저장->2진수 구하는 과정과 같음
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //현재위치의 픽셀값을 변경한 RGB값으로 변경
                                } break;
                        }

                        pixelElementIndex++; 

                        if (state == State.Filling_With_Zeros) //문자를 다 읽은 경우
                        {
                            zeros++; //zeros를 1증가
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp) //이미지를 인자로 받아 문자를 추출
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; //텍스트를 널로 초기화

            for (int i = 0; i < bmp.Height; i++) //이미지의 높이만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++) //이미지의 폭만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i); //현재 픽셀의 값을 대입
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) //RGB의 세가지 경우를 나눠줌
                        {
                            case 0: //R의 경우
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //이진수의 자리값을 맞춰주기위해 원래의 값에 2를 곱하고 R의 LSB를 더해준다
                                } break;
                            case 1: //G의 경우
                                {
                                    charValue = charValue * 2 + pixel.G % 2; //이진수의 자리값을 맞춰주기위해 원래의 값에 2를 곱하고 G의 LSB를 더해준다
                                } break;
                            case 2: //B의 경우
                                {
                                    charValue = charValue * 2 + pixel.B % 2; //이진수의 자리값을 맞춰주기위해 원래의 값에 2를 곱하고 B의 LSB를 더해준다
                                } break;
                        }

                        colorUnitIndex++; 

                        if (colorUnitIndex % 8 == 0) //한글자를 쓰면(8비트)
                        {
                            charValue = reverseBits(charValue); //입력했던 문자의 거꾸로 저장된 charValue를 역으로 바꿔줌

                            if (charValue == 0) //추출한 문자가 0일 때
                            {
                                return extractedText; //추출한 문자를 반환
                            }
                            char c = (char)charValue; //charValue를 캐릭터형으로 바꿔줌

                            extractedText += c.ToString(); //추출한 문자를 담는 변수에 추출한 문자를 담은 c를 string으로 변환해 이어줌
                        }
                    }
                }
            }

            return extractedText; //추출한 문자열을 담은 변수를 반환해줌
        }

        public static int reverseBits(int n) 
        {
            int result = 0;

            for (int i = 0; i < 8; i++) //8비트만큼 반복
            {
                result = result * 2 + n % 2; //이진수의 자리수를 맞추기 위해 result값에 2를 곱해 자리수를 맞춘 값에 n의 LSB비트를 더해줌

                n /= 2; //n을 2로 나눔
            }

            return result; //결과 값을 반환
        }
    }
}
