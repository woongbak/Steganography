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

        public static Bitmap embedText(string text, Bitmap bmp) //bmp파일에 Steganography를 실행할 수 있게 하기 위한 메소드
        {
            State state = State.Hiding;

            int charIndex = 0; //변수들을 선언

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++) //i와 j를 반복문을 이용해서 받은 bmp파일의 높이와 넓이, 즉 세로와 가로길이만큼 증가
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); //한 픽셀의 값을 가져옴

                    R = pixel.R - pixel.R % 2; //RGB의 일의 자리 값을 0으로 만들어주기 위해 2로 나눈 나머지를 빼준다
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) //3번 반복
                    {
                        if (pixelElementIndex % 8 == 0) //pixelElementIndex의 값을 8로 나눴을때 값이 0이라면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //zeros가 8이고 state가 Filling_With_Zeros와 같다면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) //pixelElementIndex - 1 한 값을 3으로 나눴을때 2보다 작다면
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//(j,i)에 위치한 픽셀을 주어진 RGB값으로 설정
                                }

                                return bmp;//변경된 이미지 반환
                            }

                            if (charIndex >= text.Length) //숨길 메세지의 인덱스값보다 입력받은 텍스트의 길이가 크거나 같다면
                            {
                                state = State.Filling_With_Zeros; //state에 Filling_With_Zeros의 값을 넣어준다
                            }
                            else
                            {
                                charValue = text[charIndex++]; //아니라면 인덱스보다 1만큼 큰 위치의 text값을 charValue에 넣어준다
                            }
                        }

                        switch (pixelElementIndex % 3) //이미지의 픽셀의 인덱스 값을 3으로 나눈 나머지에 따라 switch문 실행
                        {
                            case 0://0이라면
                                {
                                    if (state == State.Hiding)//state가 Hiding 상태라면
                                    {
                                        R += charValue % 2; //charValue를 2로 나눈 나머지를 R에 더해주고
                                        charValue /= 2;//charValue에는 2로 나눴을 때의 몫을 넣어준다.
                                    }
                                } break;
                            case 1://1이라면
                                {
                                    if (state == State.Hiding)//state가 Hiding상태라면
                                    {
                                        G += charValue % 2; //G에 charValue를 2로 나눈 나머지를 더해주고

                                        charValue /= 2; //charValue에는 몫을 넣어준다
                                    }
                                } break;
                            case 2://2라면
                                {
                                    if (state == State.Hiding)//state가 Hiding상태라면
                                    {
                                        B += charValue % 2;//B에 charValue를 2로 나눈 나머지를 더해주고

                                        charValue /= 2;//charValue에는 몫을 넣어준다
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//픽셀의 값을 해당 RGB값으로 설정
                                } break;
                        }

                        pixelElementIndex++; //픽셀의 Index 값 증가

                        if (state == State.Filling_With_Zeros) //state가 Filling_With_Zeros라면
                        {
                            zeros++;//zeros값 1 증가
                        }
                    }
                }
            }

            return bmp;//변경된 bmp파일 반환
        }

        public static string extractText(Bitmap bmp)//bmp파일을 Decode하기 위한 메소드
        {
            int colorUnitIndex = 0;//변수 선언
            int charValue = 0;

            string extractedText = String.Empty;//숨겨진 string을 저장하기 위한 빈 문자열 선언

            for (int i = 0; i < bmp.Height; i++)//파일의 높이,즉 세로부분의 크기만큼 반복
            {
                for (int j = 0; j < bmp.Width; j++)//파일의 넓이, 즉 가로부분의 크기만큼 반복
                {
                    Color pixel = bmp.GetPixel(j, i);//어떤 한 위치의 픽셀 값을 가져옴
                    for (int n = 0; n < 3; n++) //3번 반복
                    {
                        switch (colorUnitIndex % 3)//colorUnitIndex를 3으로 나눈 나머지에 따라 switch문 실행
                        {
                            case 0://0이라면
                                {
                                    charValue = charValue * 2 + pixel.R % 2;//charValue에 2를 곱해주고 R의 2의 나머지를 더해준다
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;//charValue에 2를 곱해주고 G의 2의 나머지를 더해준다
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;//charValue에 2를 곱해주고 B의 2의 나머지를 더해준다
                                } break;
                        }

                        colorUnitIndex++;//colorUnitIndex 값 증가

                        if (colorUnitIndex % 8 == 0)//colorUnitIndex의 값을 8로 나눴을 때 나머지가 0이라면
                        {
                            charValue = reverseBits(charValue); //charValue를 뒤집어서 charValue에 저장

                            if (charValue == 0)//charValue가 0이라면
                            {
                                return extractedText; //추출된 메세지 반환
                            }
                            char c = (char)charValue;//c라는 변수에 charValue값을 char타입으로 입력

                            extractedText += c.ToString();//추출된 메세지 변수에 c의 값을 스트링으로 변환한 값을 더해서 넣어준다
                        }
                    }
                }
            }

            return extractedText;//추출된 메세지 반환
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
