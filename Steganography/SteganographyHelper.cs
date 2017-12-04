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

        public static Bitmap embedText(string text, Bitmap bmp)//이미지에 문자를 숨기는 함수 
        {
            State state = State.Hiding;



            int charIndex = 0; // 숨겨질 문자의 인덱스
            int charValue = 0;//인티저로 변환 된 문자 값

            long pixelElementIndex = 0;//처리중인 픽셀의 RGB 인덱스

            int zeros = 0; //8비트 count

            int R = 0, G = 0, B = 0;//픽셀에 있는 요소들

            for (int i = 0; i < bmp.Height; i++)//이미지의 height만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++)//이미지의 width만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i);//이미지의 (height, width), 현재 처리중인 픽셀

                    R = pixel.R - pixel.R % 2;//2나눈 만큼 빼줘서 LSB를 0으로 맞춤, 예를들어 R이 3이었다면 2로나눈 나머지인 1을 3에서 빼줌= 00000010
                    G = pixel.G - pixel.G % 2;//2나눈 만큼 빼줘서 LSB를 0으로 맞춤
                    B = pixel.B - pixel.B % 2;//2나눈 만큼 빼줘서 LSB를 0으로 맞춤

                    for (int n = 0; n < 3; n++)//3번반복,(R,G,B)
                    {
                        if (pixelElementIndex % 8 == 0)//pixelElementIndex가 8의 배수면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//0이 8개인지 확인
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)//이미지의 마지막픽셀
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//컬러정해주고 
                                }

                                return bmp;//텍스트를 다 숨기고 반환
                            }

                            if (charIndex >= text.Length)//텍스트의 길이와 비교해서 모든 문자가 숨겨졌다면
                            {
                                state = State.Filling_With_Zeros;//텍스트의 끝이므로 0추가
                            }
                            else//아니라면
                            {
                                charValue = text[charIndex++];//다음문자로 이동
                            }
                        }

                        switch (pixelElementIndex % 3)//switch문으로 3으로 나눠서 RGB구분 
                        {
                            case 0://레드
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;//숨길 문자의 이진값을 2로나눈 나머지 추가
                                        charValue /= 2;//숨길 문자를 2로 나눠서 맨 마지막 비트 없앰
                                    }
                                } break;
                            case 1://그린
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;//숨길 문자의 이진값을 2로나눈 나머지 추가

                                        charValue /= 2;//숨길 문자를 2로 나눠서 맨 마지막 비트 없앰
                                    }
                                } break;
                            case 2://블루
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;//숨길 문자를 2로 나눠서 맨 마지막 비트 없앰

                                        charValue /= 2;//숨길 문자를 2로 나눠서 맨 마지막 비트 없앰
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//설정해준 값으로 컬러 
                                } break;
                        }

                        pixelElementIndex++;//RGB인덱스를 1 높여줌

                        if (state == State.Filling_With_Zeros)//8이 될때까지 1을 더함
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;//이미지 반환
        }

        public static string extractText(Bitmap bmp)//텍스트를 다시 추출하는 함수
        {
            int colorUnitIndex = 0;//픽셀의 RGB인덱스
            int charValue = 0;

            string extractedText = String.Empty;//이미지에서 추출한 텍스트 

            for (int i = 0; i < bmp.Height; i++)//아까처럼 이미지의 높이만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++)//이미지의 폭만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i);//현재 픽셀 저장 
                    for (int n = 0; n < 3; n++)//RGB 3번반복
                    {
                        switch (colorUnitIndex % 3)//3으로 나눠서 RGB구분
                        {
                            case 0://레드
                                {
                                    charValue = charValue * 2 + pixel.R % 2;//팍셀의 LSB를 문자 비트에 더해줌
                                } break;
                            case 1://그린
                                {
                                    charValue = charValue * 2 + pixel.G % 2;//2를 곱해줘서 맨 오른쪽에 더해줄 수 있게 함 
                                } break;
                            case 2://블루
                                {
                                    charValue = charValue * 2 + pixel.B % 2;//마찬가지
                                } break;
                        }

                        colorUnitIndex++;//인덱스에 1 더해줌

                        if (colorUnitIndex % 8 == 0)//8비트라면 
                        {
                            charValue = reverseBits(charValue);//비트를 뒤집어서 저장

                            if (charValue == 0)//0이라면
                            {
                                return extractedText;//리턴(0이8개면 끝)
                            }
                            char c = (char)charValue;//문자를 char로 변환

                            extractedText += c.ToString();//문자를 결과에 추가
                        }
                    }
                }
            }

            return extractedText;//다하면 반환
        }

        public static int reverseBits(int n)//비트 뒤집는 함수
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
