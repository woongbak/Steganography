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
            State state = State.Hiding; //Hiding상태인지 Filling_With_Zeros인지 구별하는 flag

            int charIndex = 0; // 조건이 맞으면 넣어야 할 text 문자열의 Index값 0부터 text.Length까지.

            int charValue = 0; // 넣어야 할 text의 문자의 값(ASCII)

            long pixelElementIndex = 0; // Pixel의 개수를 카운트 하는 값. 만약 128 * 250크기라면 최대 128*250*3까 지 된다.

            int zeros = 0; // 모든 text가 다 넣어졌을 때 부터 카운트 된다. 8번까지 추가로 for문이 돈다.

            int R = 0, G = 0, B = 0; // 해당 픽셀의 RGB값.

            for (int i = 0; i < bmp.Height; i++) // 2중 for문을 통해 모든 픽셀을 확인한다.
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); // Color형 class pixel변수에 각 좌표(Width,Height)값의 픽셀값을 대입한다(bmp class의 method GetPixel사용)

                    R = pixel.R - pixel.R % 2; // 그 픽셀값중 R(red)값에서 2로 mod한 나머지(즉 0 혹은 1)을 뺀다.
                    G = pixel.G - pixel.G % 2; // G(green)값과 B(blue)값도 똑같은 방식으로 진행한다.
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) // pixel값이 3바이트
                    {
                        if (pixelElementIndex % 8 == 0) // pixelElementIndex mod 8 == 0일때 즉 8로 나누어 떨어질때 if문으로 들어간다. 이 숫자가 8인 이유는 1byte = 8bit이기 때문이다. 
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // pixelElementIndex % 3가 0인 경우  
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //원본 파일의 RGB값을 현재의 RGB값으로 설정한다.
                                }

                                return bmp; // 모든 text가 스태가노그래피 되었기에 return.
                            }

                            if (charIndex >= text.Length) // 더 이상 스태가노그래피 할 text가 없거나 마지막 text의 문자 일때 state의 flag를 hiding->Filling_With_Zero로 변환해준다.
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else // pixelElementIndex 가 8로 나누어 떨어질 때 다음 글자 값을 넣을 준비를 한다.
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3) // pixelElementIndex(픽셀 순서) mod 3을 할 때 (이를 modPixel이라고 밑에서 설명한다.), 모든 경우는 state가 hiding상태여야 한다. / 모든 경우 charValue를 2로 나누는 이유는 만약 감출 text가 10101010이면 한글자씩 << 하면서 넣어야 하기 때문.
                        {
                            case 0: // modPixel이 0이면
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2; //R값에  charValue 짝수면 변화x 홀수면 1을 더한다.
                                        charValue /= 2; // charValue를 2로 나눈다.
                                    }
                                } break;
                            case 1: //modPixel이 1인 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; // G값에 charValue 짝수면 변화x 홀수면 1을 더한다.

                                        charValue /= 2; // charValue를 2로 나눈다.
                                    }
                                } break;
                            case 2: // modPixel이 2인 경우
                                { 
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2; //B값에 charValue 짝수면 변화x 홀수면 1을 더한다.

                                        charValue /= 2; // charValue 값을 2로 나눈다.
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // modPixel이 2인 경우 bmp파일의 pixel RGB값을 저장한다.
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros) 
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
            int colorUnitIndex = 0; // colorUnixIndex값을 저장할 변수이다. embedText의 pixelElementIndex와 같은 역할이라고 볼 수 있다.
            int charValue = 0; // decode(extract)된 값을 저장할 변수

            string extractedText = String.Empty; // extract된 문자를 저장할 변수, Empty method로 초기화. 

            for (int i = 0; i < bmp.Height; i++) // 2중 for문을 사용하여 image의 pixel값을 본다.
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); // bmp의 pixel좌표를 통해 값의 RGB값을 Color class pixel변수에 저장한다.
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) // colorUnitIndex mod 3을 한 값에 따라서 계산을 한다. 아래로는 modColor라고 칭하겠다. 이 과정은 embedText의 역이라고 생각할 수 있다.
                        {
                            case 0: // modColor값이 0인 경우. charValue를 두배 한후 R값이 홀수면 1을 더한다.
                                { 
                                    charValue = charValue * 2 + pixel.R % 2; 
                                } break;
                            case 1: // modColor값이 1인 경우. charValue를 두배 한후 G값이 홀수면 1을 더한다.
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2: // modColor값이 0인 경우. charValue를 두배 한후 B값이 홀수면 1을 더한다.
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)  // 8개의 바이트에서 charValue를 추출 했을 경우.
                        {
                            charValue = reverseBits(charValue); //charValue를 뒤집는다. 즉 11110000이였을 경우 00001111로 변환한다. 이는 숨길때 00001111순서대로 숨겼기 때문에 위의 extract과정에서 11110000순으로 값을 구했기 때문이다.

                            if (charValue == 0) // 변환한 값이 NULL값 즉 ASCII코드 변환시 0이기 대문이다.
                            {
                                return extractedText;
                            }
                            char c = (char)charValue; // 변환한 int형 값을 char형으로 변환해준다.

                            extractedText += c.ToString(); // String형의 extractedText에 추가해준다.
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n) // bit의 순서를 reverse한다. 새로운 변수 result에 n을 2로 나눈 나머지를 대입하고 2로 곱해준다.
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
