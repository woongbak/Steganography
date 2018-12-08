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

        public static Bitmap embedText(string text, Bitmap bmp) // bmp파일에 텍스트를 숨기기 위한 메소드.
        {
            State state = State.Hiding;  

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++) //2중 for 문을 이용해 bmp의 넓이와 높이만큼 반복한다.
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀의 값을 하나 가져온다.

                    R = pixel.R - pixel.R % 2;      //각각의 RGB 값의 맨 끝값을 0으로 만들어 준다. 끝값이 1일경우 %2 이기 때문에 1을 빼서 0으로 만들어준다.
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) // RGB 하나에 하나씩 3개 반복.
                    {
                        if (pixelElementIndex % 8 == 0)
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 뒤에 zeros 가 8이면 return bmp로 변형된 파일을 리턴한다.
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length)   //숨길 text 의 길이와 현재 인덱스를 비교한다.
                            {
                                state = State.Filling_With_Zeros;   //인덱스가 더 크면 상태를 바꾼다.
                            }
                            else
                            {
                                charValue = text[charIndex++]; //현재 인덱스가 더 작으면 charValue에 text 내용을 하나 넣는다.
                            }
                        }

                        switch (pixelElementIndex % 3)  //pixelElementIndex를 3으로 나머지 연산해서 0일 경우는 R값에 1일경우는 G값에 2일 경우는 B 값에 해당 내용을 넣는다.
                        {
                            case 0:                 //각각의 케이스에 charValue를 넣어서 text를 숨긴다.
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2; // 끝자리를 0으로 만든 R값에 숨기고자 하는 값의 맨 끝값을 저장한다.
                                        charValue /= 2;     //2로 나누는것은 2진수 형식으로 데이터를 저장하기 위해서이다.(2로 나누면 2진수 형식에서 맨끝값이 사라진다 맨끝값은 바로 윗줄에서 넣었으므로 삭제하는것과 같다.)
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    //숨긴 픽셀을 다시 이미지에 넣는다.
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros) // 숨길 텍스트보다 인덱스가 더 큰경우에 상대를 바꿧으므로 해당 상태이면 zeros++를 한다
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp) //숨긴 텍스트를 다시 찾는 메소드
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++) // 2중for문으로 전체의 픽셀을 돈다.
                {
                    Color pixel = bmp.GetPixel(j, i); //픽셀을 하나 가져온다.
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) //숨길때와 마찬가지로 0이면 R 1이면 G 2이면 B.
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;    //앞의 charValue를 *2 해줌으로써 2진수에서 한자리씩 앞으로 옮기고 맨 마지막에 현재 가지고 있는 픽셀값을 가져온다.
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

                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue); //현재 charValue에는 저장된 문자열의 값이 반대로 들어가있으므로 다시 반대로 바꾸어 제대로 출력시킨다.

                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString(); //결과 Text에 charValue를 추가시킨다.
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n) //2진수로 된 비트를 반대로 바꾸는 함수
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
