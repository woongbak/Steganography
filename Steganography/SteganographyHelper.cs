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

        public static Bitmap embedText(string text, Bitmap bmp){
            State state = State.Hiding;
            int charIndex = 0;
            int charValue = 0;
            long pixelElementIndex = 0;
            int zeros = 0;
            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++) {  //이미지의 세로
                for (int j = 0; j < bmp.Width; j++) { //이미지의 가로
                    Color pixel = bmp.GetPixel(j, i); //(j,i)좌표의 픽셀 값을 얻는다.

                    //한 픽셀의 RGB의 LSB를 0으로 초기화 한다.
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    //for문을 3번 도는 이유는 R,G,B를 참조하기 위해서다.
                    //for문을 돌 때마다 pixelElementIndex값이 하나씩 증가하여 R, G, B를 참조한다.
                    for (int n = 0; n < 3; n++) {
                        //1byte가 8bit이므로 pixelElementIndex가 8번 돌았으면
                        //text의 다음 메세지 값을 받는다. 
                        if (pixelElementIndex % 8 == 0) {
                            if (state == State.Filling_With_Zeros && zeros == 8) {
                                if ((pixelElementIndex - 1) % 3 < 2) {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            //메세지를 모두 LSB에 저장하였다면 숨김이 끝났다는 것을 알려주기 위해
                            //state값을 Filling_With_Zeros로 설정해준다.
                            if (charIndex >= text.Length) {
                                state = State.Filling_With_Zeros;
                            }
                            else { //charValue에다가 메세지의 값을 넣는다. 
                                charValue = text[charIndex++];
                            }
                        }

                        /*
                        R, G, B의 LSB에 charValue의 1bit씩 넣는다.
                        주의할 점은 RGB에 값이 들어갈 때 거꾸로 들어간다.
                        예를 들어, charValue=10110101(2)일 때
                        10101101(2)로 들어가게 된다.
                        추출할 때는 반대로 해줘야 한다.
                        */
                        switch (pixelElementIndex % 3) {
                            case 0:
                                if (state == State.Hiding) {
                                    R += charValue % 2;
                                    charValue /= 2;
                                 }
                                 break;
                            case 1:
                                if (state == State.Hiding){
                                    G += charValue % 2;
                                    charValue /= 2;
                                }
                                break;
                            case 2:
                                if (state == State.Hiding){
                                    B += charValue % 2;
                                    charValue /= 2;
                                }
                                //기존 RGB원본에 데이터가 숨겨진 RGB픽셀을 덮어 씌운다.
                                bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros){
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

            for (int i = 0; i < bmp.Height; i++) { //이미지의 세로
                for (int j = 0; j < bmp.Width; j++) {  //이미지의 가로
                    Color pixel = bmp.GetPixel(j, i);  //(j,i)좌표의 픽셀 값을 얻는다.

                    for (int n = 0; n < 3; n++) {
                        /*
                        R, G, B의 LSB로부터 값을 받아온다.
                        다음 값을 받아올 때마다 x2씩 해준다. 왜냐하면 2진수이기 때문이다.
                        주의할 점은 RGB에 Text가 거꾸로 들어가져 있다는 것이다.
                        */
                        switch (colorUnitIndex % 3) {
                            case 0:
                                charValue = charValue * 2 + pixel.R % 2;
                                break;
                            case 1:
                                charValue = charValue * 2 + pixel.G % 2;
                                break;
                            case 2:
                                charValue = charValue * 2 + pixel.B % 2;
                                break;
                        }

                        colorUnitIndex++;

                        /*
                         여기서 8bit가 됬을 경우 bit를 역순으로 취해줘서 얻고자 하는 데이터 값을 얻어야
                         한다. 왜냐하면 Encode 때 데이터가 거꾸로 들어갔기 때문이다.
                        */
                        if (colorUnitIndex % 8 == 0) {
                            charValue = reverseBits(charValue);

                            if (charValue == 0) {
                                return extractedText;
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString();
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
