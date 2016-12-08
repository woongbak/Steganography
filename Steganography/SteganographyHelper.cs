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

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;

            int charIndex = 0; //값들을 초기화. 캐릭터인덱스,값,픽셀요소인덱스,RGB등..

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); //높이X넓이 만큼 각 픽셀들을 얻는 코드인데 진행방향은 넓이쪽이다. 즉 높이는 넓이가 0부터 그림의 넓이까지 간뒤 변경된다.

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) //한픽셀이 RGB로 구성되어있으므로 3번 반복. RGB에 메시지비트삽입이 끝나면 다시 RGB픽셀을 얻을것임
                    {
                        if (pixelElementIndex % 8 == 0) //한 글자당 8비트의 메시지를 삽입한다. 만약 7비트면 한비트는 0으로 패딩함
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //밑에서 메시지삽입이 다 끝나고 zeros가 8까지되면 즉 픽셀요소(RGB) 8개에 0이 들어가게 되면 트루가된다.
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //switch문에서 case2를 거치지 못하게되면 픽셀을 변경된 RGB로 적용할수 없다 즉 R또는 G까지만 변경하다가 끝을 알리는 8개의 제로비트가 오게된 경우이다.
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length) //text에는 숨기고자할 메시지가 저장. 만약 charIndex값이 메시지보다 크면
                            {
                                state = State.Filling_With_Zeros; //state를 State.Filling_With_Zeros상태로 바꾼다. 즉 모든 메시지를 입력하면 바뀌게 된다.
                            }
                            else
                            {
                                charValue = text[charIndex++]; //그렇지않으면 charValue에 입력한 메시지를 아스키코드값으로 가져온다.
                            }
                        }

                        switch (pixelElementIndex % 3) //각 픽셀의 RGB값에 메시지를 한비트씩 저장하는 과정
                        {
                            case 0: //R의경우 
                                {
                                    if (state == State.Hiding) //메시지 삽입이 완료후에는 거치지 않고 break가 일어나 스위치문을 탈출함.
                                    {
                                        R += charValue % 2; //2로나눈 나머지를 더한다. 
                                        charValue /= 2; //그후 2로 나눈다.  정리하자면 한글자가 있으면 즉 그 글자의 LSB부터 넣고, 한비트를 줄이는 원리
                                    }
                                }
                                break;
                            case 1: //G의경우
                                {
                                    if (state == State.Hiding) //위와 같다.
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 2: //B의경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //RGB값의 조정이 모두 다되면 B가 젤마지막이니 픽셀에 값을 저장해준다.
                                }
                                break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros) //모든 메시지 삽입이 끝나면 위에서 state의 상태가 바뀐다. 그리고 zeros를 증가시킨다. (8개까지 증가할것이다)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++) //이미지의 왼쪽위 ( 0,0 )부터 시작하여 픽셀을 추출해낸다.
                {
                    Color pixel = bmp.GetPixel(j, i); //pixel에 그림의 한 픽셀을 저장
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) //RGB중 어느것인지 확인후
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //맨마지막비트를 추출하고(%연산) 이진수에서 십진수로 변경하는 연산(*2연산)까지 한다.
                                }
                                break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                }
                                break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                }
                                break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue); //넣을때는 LSB부터 넣었다. 그러나 추출할 때는 거꾸로 넣은것을 그대로 추출하였기에 자리를 뒤집어야한다(2진수 에서)

                            if (charValue == 0) //추출해낸 메시지가 0이면 즉 추출된 메시지 8비트가 모두 0인경우가 되어야한다.
                            {
                                return extractedText;
                            }
                            char c = (char)charValue; //추출해낸 메시지는 아직 숫자이므로 char형으로 변환시켜준다.

                            extractedText += c.ToString();// 추출해낸 메시지들을 붙인다.
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)
        {
            int result = 0;

            for (int i = 0; i < 8; i++) //총비트가 8개이므로 8번반복하는것임
            {
                result = result * 2 + n % 2; //2진수로 나열했을 때 그 수를 뒤집는 연산.

                n /= 2;
            }

            return result;
        }
    }
}
