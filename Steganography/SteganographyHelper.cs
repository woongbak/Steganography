using System;                               
using System.Drawing;                           //비트맵 사용하기 위함

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding,                                         // Hiding = 0
            Filling_With_Zeros                              // Filling_With_Zeros = 1
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;                      //state = 0 초기화

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)               //bmp의 높이만큼 반복
            {  
                for (int j = 0; j < bmp.Width; j++)                 //bmp의 너비만큼반복
                {
                    Color pixel = bmp.GetPixel(j, i);                   //pixel에 bmp(j,i)의 값을 넣는다

                    R = pixel.R - pixel.R % 2;                                 //받은 픽셀의 R의 마지막비트의 값을 비운다                 즉, RGB의 마지막비트에데이터를 삽입한다. 
                    G = pixel.G - pixel.G % 2;                                 //받은 픽셀의 G의 마지막비트의 값을 비운다
                    B = pixel.B - pixel.B % 2;                                 //받은 픽셀의 B의 마지막비트의 값을 비운다

                    for (int n = 0; n < 3; n++)                           //3번 반복(R,G,B)한다
                    {
                        if (pixelElementIndex % 8 == 0)                     //PixelElemnetIndex 가 0이거나 8의 배수이면 진입       여기서 한 문자(Ascii코드이면서 1byte)를 삽입한다는걸 알 수있다.      
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)        //state ==1, zeros==8이면 진입 더이상 넣을 문자가 없고 zeros가 8이란건 혹시나 남아있을지 모르는 비트를 마저 넣거나 손실되는 경우를 방지하기위해 한바이트를 더 돌려 확인하기위한 장치        
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)                /*만약 PixelElementIndex-1 을 3모듈로연산한것이 2이하면 진입  즉, pixelElementIndix가 24의배수일때는 건너뛴다. 왜 24의 배수일때 건너뛰느냐 하면 24의배수일때는 모든 비트들이 잘리지않고 딱 들어가는 때이므로 switch문의 case 2까지 접근하여 알아서 setpixel을 해준다
                                                                              반면에 24의 배수가 아닌 8,16,32...일때에는 3의 배수가 아니여서 마지막픽셀이 switch문의 2에 진입하지 못하고 setpixel의 은혜를 받지못하여 여기서 따로 setpixel로 현재픽셀에 삽입된 비트들을 설정해주는것이다. */
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));            //변경된 rgb값들을 구성요소로 bmp의 j,i(x,y) 에넣는다
                                }

                                return bmp;                                                 //문자가 전부 삽입되고 모든 비트가 삽입되어 완전히 끝났다고 인식될때 (굳이 bmp끝까지 안가도 더이상 넣을데이터가없을때)bmp를 반환한다. 반복문을 쓸대없이 반복하지않고 여기서 끝낸다! 
                            }

                            if (charIndex >= text.Length)               //만약 charIndex가 text(사용자가 삽입할 값)의 길이보다 커졌다면진입 즉, 문자열 다 넣으면 state를 1로만든다
                            {
                                state = State.Filling_With_Zeros;           //state가 1이 된다.
                            }
                            else                                        //state가 1도 아니고(끝이아직안났고) charIndex가 텍스트의 길이보다 크지않을경우엔 
                            {   
                                charValue = text[charIndex++];          //charValue에 text의 charindex에 해당하는 ascii값을 넣고 charIndex 1증가시킴 (여기가 사용자가 입력한 문자를 넣기위해 준비하는부분)
                            }
                        }

                        switch (pixelElementIndex % 3)                  //pixelElemnetIndex를 3으로 나눈 나머지값으로 (r,g,b 를 구분하기위해)
                        {
                            case 0:                                     //0이면 진입         (R을 변경)
                                {
                                    if (state == State.Hiding)          //만약 state가 0이면 
                                    {
                                        R += charValue % 2;             //R에다가 charValue(ascii)를 2로나눈 나머지를 추가한다. 즉 여기선 ascii의 마지막비트를 R에 넣는다.
                                        charValue /= 2;                 //그리고 charValue를 2로 나누고 charValue에 다시넣는다. 즉 마지막비트를 넣었으니 2로나눠서 쉬프트 >>하는것과같은효과를 내기위해 비트를 밀어준다
                                    }
                                } break;                                //switch문을 끝낸다
                            case 1:                                     //나머지가 1이면 진입 (G를 변경)
                                {
                                    if (state == State.Hiding)          //state가 0이면 진입
                                    {
                                        G += charValue % 2;             //G에다가 charValue를 2로나눈 나머지를 추가한다.  charValue의 마지막비트를 G에 넣는다
                                        charValue /= 2;                 // 그리고 charValue 를 2로 나눈다.                마지막비트를 삽입했으니 다시 왼쪽으로 쉬프트해준다.
                                    }
                                } break;                                //switch문을 끝낸다
                            case 2:                                     //나머지가 2이면 진입  (B를 변경)
                                {
                                    if (state == State.Hiding)          //만약 state가 0이면 진입
                                    {
                                        B += charValue % 2;             //B에 charValue를 2 로나눈 나머지를 추가한다.     charValue의 마지막비트를 B에 넣는다.
                                        charValue /= 2;                 //charvalue를 2로나눈다.                          왼쪽쉬프트
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));        //나머지가 2면 bmp의 해당 픽셀에 j,i(x,y)에 red,green,blue를 전달한다
                                } break;                                 //switch문을 끝낸다
                        }

                        pixelElementIndex++;                                //pixelElementIndex 1증가

                        if (state == State.Filling_With_Zeros)              //만약 state가 1이면
                        {
                            zeros++;                                        //zeros가 1증가
                        }
                    }
                }
            }

            return bmp;                                             //모든 픽셀의 반복이 끝나면 bmp반환
        }

        public static string extractText(Bitmap bmp)                //bmp를 받아서 문자를 추출
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;                    //일단 문자를받을 텍스트를 생성한다.

            for (int i = 0; i < bmp.Height; i++)                    //bmp의 높이만큼반복
            {
                for (int j = 0; j < bmp.Width; j++)                 //bmp의 너비만큼반복
                {
                    Color pixel = bmp.GetPixel(j, i);               //분석하기위해 생성한pixel에 현재가리키는 pixel의 값 집어넣는다
                    for (int n = 0; n < 3; n++)                      //rgb반복하기위한 부분
                    {
                        switch (colorUnitIndex % 3)                     //colorUnitIndex를 3으로 나눈 나머지로 switch문 실행
                        {
                            case 0:                                         //만약 0이면 
                                {
                                    charValue = charValue * 2 + pixel.R % 2;            //charValue에 2를 곱한다(오른쪽으로 shift)그리고 pixel의 Red의 마지막에 있는 비트를 charValue에 집어넣는다.
                                } break;
                            case 1:                                          //만약 1이면
                                {
                                    charValue = charValue * 2 + pixel.G % 2;            //charValue에 2를 곱한다(오른쪽으로 shift)그리고 pixel의 Green의 마지막에 있는 비트를 charValue에 집어넣는다.
                                } break;
                            case 2:                                          //만약 2이면
                                {
                                    charValue = charValue * 2 + pixel.B % 2;            //charValue에 2를 곱한다(오른쪽으로 shift)그리고 pixel의 Blue의 마지막에 있는 비트를 charValue에 집어넣는다.
                                } break;
                        }

                        colorUnitIndex++;                                   //위치를 파악하기위해 colorUnitIndex를 1상승

                        if (colorUnitIndex % 8 == 0)                        //8번반복할때마다 (1byte크기로 데이터를 나눠 넣었으므로) 진입
                        {
                            charValue = reverseBits(charValue);              //charValue를 거꾸로 다시집어넣는다 (왜냐하면 넣을때는 마지막비트가 맨뒤로 갔지만 여기서 해석할땐 마지막비트를 먼저읽엇으므로 LIFO처럼)

                            if (charValue == 0)                              //만약 charValue가 비었으면(굳이 끝까지 가지않더라도 한 바이트가 전부 0이면)
                            {
                                return extractedText;                        //추출한 문자를 리턴한다 (일찍 끝내기)
                            }
                            char c = (char)charValue;                        // c에다가 ascii코드를 문자로 변환해서 삽입

                            extractedText += c.ToString();                  //extractedText에 c(추출된 ascii문자)를 추가시킴
                        }
                    }
                }
            }

            return extractedText;                                           //반복문이 끝까지 가면 반환
        }

        public static int reverseBits(int n)                            //비트의 순서를 바꾸는  함수
        {
            int result = 0; 

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;                            //result를 2로나눠 나머지가있으면 마지막bit에 1을 집어넣고 왼쪽으로 shitf하면서 순서를바꾼다

                n /= 2;
            }

            return result;
        }
    }
}
