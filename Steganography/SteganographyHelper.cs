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


        //이미지에 원하는 텍스트를 숨기기 위한 숨김 함수
        //text : 숨기려고 하는 문자열, bmp : 문자열을 숨기기 위해서 이용할 사진파일

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;

            int charIndex = 0;
            //text의 문자열의 인덱스를 나타내는 변수이다.

            int charValue = 0;
            //text의 해당 문자를 정수로 저장하기 위한 변수이다.

            long pixelElementIndex = 0;
            //RGB를 판단하기 위한 변수이다.

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)//사진의 높이까지 i를 증가시킨다.
            {
                for (int j = 0; j < bmp.Width; j++)//사진의 넓이까지 j를 증가시킨다.
                {
                    Color pixel = bmp.GetPixel(j, i);
                    //현재 i, j가 가리키고 있는 이미지의 (i, j)좌표의 픽셀을 불러온다.

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    //해당하는 좌표의 픽셀의 LSB 값을 모두 0으로 바꾸어준다.
                    //픽셀은 8비트이기 때문에 마지막 비트의 값만 추출하기 위해서 value % 2를 해준 모습이다.

                    for (int n = 0; n < 3; n++)
                    //R, G, B를 모두 돌아봐야 하므로 3번 for문을 돌린다.
                    {
                        if (pixelElementIndex % 8 == 0)
                        //가져온 픽셀의 인덱스가 0이거나 8의 배수일때 실행
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                    //해당 픽셀 R, G, B를 0으로 세팅.
                                }

                                return bmp;
                                //텍스트가 숨겨진 이미지를 반환.
                            }

                            if (charIndex >= text.Length)
                            //문자열을 가르키는 index가 받은 text의 길이보다 크거나 같아졌을 때
                            {
                                state = State.Filling_With_Zeros;
                                //text의 모든 문자를 숨겨준다.
                            }
                            else
                            {
                                charValue = text[charIndex++];
                                //text의 해당 인덱스의 문자를 정수로 charValue에 저장하고 인덱스를 1증가시켜준다.
                            }
                        }

                        switch (pixelElementIndex % 3)
                        //Pixel 인덱스를 3으로 나누어서 R, G, B를 판단.
                        {
                            case 0://R일때
                                {
                                    if (state == State.Hiding)
                                    //상태가 숨김상태일 경우
                                    {
                                        R += charValue % 2;
                                        //R의 마지막 비트에, 해당 문자의 값을 2로 나눈 나머지를 더한다.
                                        charValue /= 2;
                                        //해당 문자의 값을 2로 나눈다.
                                    }
                                } break;
                            case 1://G일때
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;
                                        //G의 마지막 비트에, 해당 문자의 값을 2로 나눈 나머지를 더한다.
                                        charValue /= 2;
                                        //해당 문자의 값을 2로 나눈다.
                                    }
                                } break;
                            case 2://B일때
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;
                                        //B의 마지막 비트에, 해당 문자의 값을 2로 나눈 나머지를 더한다.
                                        charValue /= 2;
                                        //해당 문자의 값을 2로 나눈다.
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                    //열어놓은 이미지의 RGB를 현재 저장되어있는 R, G, B값으로 설정
                                } break;
                        }

                        pixelElementIndex++;
                        //Pixel의 인덱스 1증가.

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
            //텍스트가 숨겨진 이미지 파일 반환
        }

        //텍스트를 이미지에서 추출하기 위한 함수
        //bmp : 텍스트를 추출하기 위한 이미지

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            //사진의 인덱스(R,G,B)를 나타내는 변수.
            int charValue = 0;
            //찾아낸 문자의 정수 값을 저장하기 위한 변수.

            string extractedText = String.Empty;
            //비어있는 문자열 변수 선언

            for (int i = 0; i < bmp.Height; i++)//사진의 높이까지 i를 증가시킨다.
            {
                for (int j = 0; j < bmp.Width; j++)//사진의 넓이까지 j를 증가시킨다.
                {
                    Color pixel = bmp.GetPixel(j, i);
                    //현재 i, j가 가리키고 있는 이미지의 (i, j)좌표의 픽셀을 불러온다.

                    for (int n = 0; n < 3; n++)
                    //R, G, B를 모두 돌아봐야 하므로 3번 for문을 돌린다.
                    {
                        switch (colorUnitIndex % 3)
                        //R, G, B를 판단한다.
                        {
                            case 0://R일때
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                    // 추출한 부분의 R 영역의 LSB 값과 charValue에 2를 곱한 값을 더한 후 저장
                                } break;
                            case 1://G일때
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                    // 추출한 부분의 G 영역의 LSB 값과 charValue에 2를 곱한 값을 더한 후 저장
                                } break;
                            case 2://B일때
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                    // 추출한 부분의 B 영역의 LSB 값과 charValue에 2를 곱한 값을 더한 후 저장
                                } break;
                        }

                        colorUnitIndex++;
                        //그림 픽셀 인덱스를 1 증가시킨다.

                        if (colorUnitIndex % 8 == 0)
                        //사진 인덱스가 0이나 8의 배수일때
                        {
                            charValue = reverseBits(charValue);

                            if (charValue == 0)
                            //charValue이 0이되면 끝을 의미하므로 문자열 반환
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;
                            //문자로 강제변환

                            extractedText += c.ToString();
                            //문자열에 해당 문자 삽입.
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
