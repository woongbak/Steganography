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

        public static Bitmap embedText(string text, Bitmap bmp)//이미지에 문자를 숨기는 메소드
        {
            State state = State.Hiding;

            int charIndex = 0;//숨김문자 인덱스

            int charValue = 0;//숨김문자 값.

            long pixelElementIndex = 0;//픽셀의 RGB인덱스

            int zeros = 0; //8bit count

            int R = 0, G = 0, B = 0;//픽셀의 Red,Green,Blue

            for (int i = 0; i < bmp.Height; i++)//이미지 height 
            {
                for (int j = 0; j < bmp.Width; j++)//이미지의 width 결국 => height * width만큼 탐색.
                {
                    Color pixel = bmp.GetPixel(j, i); //현재 픽셀을 의미하며 (height,width)의 픽셀

                    R = pixel.R - pixel.R % 2;//현재의 Rpixel의 2로 나눈 나머지 만큼을(0 or 1)을 뺀다.
                    G = pixel.G - pixel.G % 2;//현재의 Gpixel의 2로 나눈 나머지 만큼을(0 or 1)을 뺀다.
                    B = pixel.B - pixel.B % 2;//현재의 Bpixel의 2로 나눈 나머지 만큼을(0 or 1)을 뺀다.

                    for (int n = 0; n < 3; n++)//각각의 R,G,B를 위해 3번 탐색
                    {
                        if (pixelElementIndex % 8 == 0)//pixelElementIndex가 8로 나눠지면 즉,나머지가 0이면 다음 if문에 들어감
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//0이 8개인지 확인
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)//이미지의 마지막 픽셀 컬러를 정해준다.
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;//다 처리된 이미지를 반환
                            }

                            if (charIndex >= text.Length)//만약 텍스트 길이가 숨기려는 문자의 index 보다 작다면
                            {
                                state = State.Filling_With_Zeros;//텍스트의 끝에 0 추가상태
                            }
                            else
                            {
                                charValue = text[charIndex++];//아니라면 다음문자로 이동
                            }
                        }

                        switch (pixelElementIndex % 3)//RGB를 3개의 case로 구분
                        {
                            case 0://Red
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;//숨김 문자의 값을 2로나눈 나머지를 더함
                                        charValue /= 2;//숨김 문자값을 2로 나누어 저장
                                    }
                                }
                                break;
                            case 1://GReen
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;//숨김 문자의 값을 2로나눈 나머지를 더함

                                        charValue /= 2;//숨김 문자값을 2로 나누어 저장
                                    }
                                }
                                break;
                            case 2://Blue
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;//숨김 문자의 값을 2로나눈 나머지를 더함

                                        charValue /= 2;//숨김 문자값을 2로 나누어 저장
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//설정해준 값으로 pixel설정
                                }
                                break;
                        }

                        pixelElementIndex++;//RGB pixel Element Index 1 높임

                        if (state == State.Filling_With_Zeros)//8이 될대까지
                        {
                            zeros++;//1씩 올려준다.
                        }
                    }
                }
            }

            return bmp;//처리된 이미지 반환
        }

        public static string extractText(Bitmap bmp)//이미지로 부터 문자를 추출하기 위한 메소드 
        {
            int colorUnitIndex = 0;//colorUnitindex를 0설정
            int charValue = 0;//문자 값을 0으로 설정

            string extractedText = String.Empty;//이미지에서 추출한 텍스트

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)//탐색한다. height*width 만큼
                {
                    Color pixel = bmp.GetPixel(j, i);//현재 pixel을 저장
                    for (int n = 0; n < 3; n++)//RGB 이기에 3번 탐색
                    {
                        switch (colorUnitIndex % 3)//Unitindex를 3으로 나누어 나머지로 구분
                        {
                            case 0://red
                                {
                                    charValue = charValue * 2 + pixel.R % 2;//픽셀의 LSB를 문자 비트 값에 더함, 2를 곱하므로 맨 오른쪽에 더할 수 있음
                                }
                                break;
                            case 1://green
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                }
                                break;
                            case 2://blue
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                }
                                break;
                        }

                        colorUnitIndex++;//인덱스를 1씩추가

                        if (colorUnitIndex % 8 == 0)//인덱스가 8로 나눠지면
                        {
                            charValue = reverseBits(charValue);//문자 값을 뒤집에서 저장(reverse bit)

                            if (charValue == 0)//만약 charValue가 0이면
                            {
                                return extractedText;//extractText반환 (0)
                            }
                            char c = (char)charValue;//문자값을 char로 변환

                            extractedText += c.ToString();//extractText에 연속 값추가.
                        }
                    }
                }
            }

            return extractedText;//추출 메세지 반환
        }

        public static int reverseBits(int n)//비트를 뒤집어서 반환하는 메소드
        {
            int result = 0;

            for (int i = 0; i < 8; i++)//8번의 실행동안 비트를 뒤집으며
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;//뒤집힌 비트를 반환한다.
        }
    }
}
