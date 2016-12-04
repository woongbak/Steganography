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

        public static Bitmap embedText(string text, Bitmap bmp) //숨기기 위한 메소드
        {
            State state = State.Hiding;

            int charIndex = 0; //숨길 텍스트의 문자를 하나하나 옮겨가기 위한 인덱스를 저장하는 변수

            int charValue = 0; //숨길 텍스트의 문자 하나씩을 저장하기 위한 변수

            long pixelElementIndex = 0; //그림 픽셀의 RGB 하나하나에 인덱스를 부여하여 세기 위한 변수

            int zeros = 0; //데이터 끝의 상태일 때 세팅한 0의 개수를 저장하기 위한 변수

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++) 
            { 
                for (int j = 0; j < bmp.Width; j++) //이미지의 각 픽셀을 반복문으로 방문
                {
                    Color pixel = bmp.GetPixel(j, i);

                    R = pixel.R - pixel.R % 2; //픽셀 R 값의 LSB를 0으로 세팅(2로 나눈 나머지(0또는 1)를 빼서)
                    G = pixel.G - pixel.G % 2; //픽셀 G 값의 LSB를 0으로 세팅
                    B = pixel.B - pixel.B % 2; //픽셀 B 값의 LSB를 0으로 세팅

                    for (int n = 0; n < 3; n++) //3회짜리 반복문(RGB 한번씩 방문)
                    {
                        if (pixelElementIndex % 8 == 0) //인덱스가 0(텍스트의 문자 하나 읽어옴) 또는 8의 배수(하나의 문자 처리 완료)인 경우
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //숨긴 데이터가 끝나고 마지막 8비트를 0으로 세팅하는 것이 완료된 경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) //0으로 세팅한 마지막 RGB중 하나가 B가 아니어서 픽셀에 세팅이 못 되었다면
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 픽셀에 값 세팅
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length) //텍스트에서 읽어들인 부분이 텍스트의 길이보다 길거나 같은 경우(다 읽은 경우)
                            {
                                state = State.Filling_With_Zeros; //state를 숨김(Hiding) 상태에서 데이터 끝 상태로 바꿈(숨긴 데이터가 끝나면 8비트를 0으로 세팅)
                            }
                            else //아닌 경우
                            {
                                charValue = text[charIndex++]; //숨길 텍스트의 한 문자를 정수값으로 저장
                            }
                        }

                        switch (pixelElementIndex % 3) //픽셀 인덱스를 3으로 나눈 나머지 값이 
                        {
                            case 0: //0인 경우, R
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2; //문자의 정수값 중 마지막 비트를 픽셀에 인코딩 
                                        charValue /= 2; //문자값의 자리수를 한 단계씩 낮춤(2진수여서 2로 나눔)
                                    }
                                } break;
                            case 1: //1인 경우, G
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; //문자의 정수값 중 마지막 비트를 픽셀에 인코딩

                                        charValue /= 2; //문자값의 자리수를 한 단계씩 낮춤(2진수여서 2로 나눔)
                                    }
                                } break;
                            case 2: //2인 경우, B
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2; //문자의 정수값 중 마지막 비트를 픽셀에 인코딩

                                        charValue /= 2; //문자값의 자리수를 한 단계씩 낮춤(2진수여서 2로 나눔)
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //문자값이 LSB에 저장된 RGB를 픽셀에 저장
                                } break;
                        }

                        pixelElementIndex++; //R G B당 하나하나 인덱스가 증가

                        if (state == State.Filling_With_Zeros) //state가 데이터 끝 상태여서 0으로 세팅중인 경우
                        {
                            zeros++; //세팅한 0 값의 개수 카운트
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp) //추출하기 위한 메소드
        {
            int colorUnitIndex = 0; //그림 픽셀의 RGB 하나하나에 인덱스를 부여하여 세기 위한 변수
            int charValue = 0; //문자값을 저장하기 위한 변수

            string extractedText = String.Empty; //추출된 텍스트를 저장하기 위한 변수

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++) //이미지의 각 픽셀을 반복문으로 방문
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) //3회짜리 반복문(RGB 한번씩 방문)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //문자값의 자리수를 한 단계씩 높여서 픽셀 중 R 값의 LSB 비트를 삽입
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2; //문자값의 자리수를 한 단계씩 높여서 픽셀 중 G 값의 LSB 비트를 삽입
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2; //문자값의 자리수를 한 단계씩 높여서 픽셀 중 B 값의 LSB 비트를 삽입
                                } break;
                        }

                        colorUnitIndex++; //R G B당 하나하나 인덱스가 증가

                        if (colorUnitIndex % 8 == 0) //하나의 문자(8비트)를 다 읽은 경우
                        {
                            charValue = reverseBits(charValue);

                            if (charValue == 0) //데이터 끝에 삽입된 0을 읽은 경우
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString(); //추출된 문자를 텍스트 변수에 삽입
                        }
                    }
                }
            }

            return extractedText;
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
