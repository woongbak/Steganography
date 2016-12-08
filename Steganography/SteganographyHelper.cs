using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State       //enum: 열거타입
        {
            Hiding,             //문자 숨김 가능 상태
            Filling_With_Zeros  //문자 숨김 완료 상태
        };

        //숨기기 위한 메소드 - 이미지에 텍스트 숨기기
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding; //문자 숨김 가능 상태로 시작

            int charIndex = 0;          //문자열 사이의 위치
                                        //숨겨지기 위한 문자의 위치

            int charValue = 0;          //문자열 중 문자 하나의 ASCII code 값

            long pixelElementIndex = 0; //문자 1개 = 1byte = 8bit

            int zeros = 0;              //1bit == 0

            int R = 0, G = 0, B = 0;    //RGB: Red, Green, Blue - 0~255

            //bmp의 높이 -> i
            for (int i = 0; i < bmp.Height; i++)
            {
                //bmp의 넓이 -> j
                for (int j = 0; j < bmp.Width; j++)
                {
                    //하나의 픽셀을 읽음
                    Color pixel = bmp.GetPixel(j, i);

                    //읽어온 픽셀 초기화 과정
                    //1픽셀에 해당하는 RGB 각각의 LSB를 0으로 초기화
                    //1픽셀 = 1byte = 8bit = GRBGRBGR = GRBGR000
                        //방법(j(width), i(height))에 해당되는 픽셀의 각 RGB 값에서 각각의 값을 2로 나눈 나머지를 뺌
                        //ex) if R=153; R = 153 - 1(=153%2)
                    //가장 마지막 비트만 변경되기 때문에 눈으로 봤을때 색감(그림)의 큰 변화가 보이지 않기 때문
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    //n은 1 pixel에 해당하는 RGB 각각의 LSB 부분을 나타냄
                    //GRBGRBGR = GRBGRnnn = GRBGR210
                    for (int n = 0; n < 3; n++)
                    {
                        //1pixel을 
                        if (pixelElementIndex % 8 == 0)
                        {
                            //first if: 문자 저장이 완료된 것을 확인하고, 확인된 지점을 파악하기 위함
                            //문자 저장이 모두 완료되면 state = Filling_with_zeros
                            //여기에 문자 저장이 모두 완료된 지점임을 나타내기 위해
                            //3개의 pixel에서 각각의 RGB의 LSB들(총 9개 bit 중 8bit)이 위 픽셀 초기화 과정에 의해 0으로 변경됨 -> zeros = 8
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    //픽셀 (j,i)에 수정된 RGB의 각 값들을 재할당
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            //second if: 텍스트를 숨기는 부분
                            //인덱스 값이 텍스트(숨겨질)의 길이보다 길거나 같으면
                                //인덱스 값은 현재 숨겨진 텍스트의 길이를 의미하는 것으로
                                //현재 숨겨진 텍스트의 길이가 숨겨질 문자열의 총 길이보다 길거나 같다는 것은, 모든 텍스트가 숨겨졌다는 것을 의미
                                //그러면 state는 Filling_with_Zeros 가 됨
                            //그렇지 않으면, charValue에 text의 글자 하나의 값을 저장하고
                                //다음 차례에, 다음 문자를 저장할 수 있도록 또는 텍스트의 끝을 확인할 수 있도록 charIndex를 1을 미리 올림
                            if (charIndex >= text.Length)
                            {
                                //State의 상태 변환
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                //text의 글자(하나)의 값을 저장
                                charValue = text[charIndex++];
                            }
                        }

                        //state의 상태가 hiding인 동안
                            //0 또는 1(charValue%2)을 각각 RGB의 LSB에 더함
                            //8bit 중 LSB에 해당하는 1비트만 변화
                                //숨겨지는 문자는 ASCII code 2진수 기법으로 저장되며, 순거가 거꾸로 되어 저장됨
                                //ex) 97 = a = 2진수 01100001 => 10000110 으로 1bit씩 저장됨
                        //state의 상태에 관계없이 (pixelElementIndex % 3) == 2 인 경우에는
                        //픽셀 값이 변경된 픽셀 값으로 변경됨
                        switch (pixelElementIndex % 3)
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
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

                                    //수정된 RGB 값을 해당 픽셀에 재할당 - state의 상태에 관계 없음
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        pixelElementIndex++;

                        //state가 Filling_With_Zeros인 동안 zeros의 값이 계속 오름
                        //zeros는 최대 8(8bit)까지 오를 수 있음
                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }


        //추출하기 위한 메소드 - 숨겨진 텍스트 추출
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;    //처음엔 빈 공간(값) - 숨겨진 텍스트가 없음을 의미함

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    //하나의 픽셀을 읽음
                    Color pixel = bmp.GetPixel(j, i);

                    //1픽셀에 해당하는 RGB 각각의 LSB를 0으로 초기화
                    //1픽셀 = 1byte = 8bit = GRBGRBGR = GRBGR000
                    for (int n = 0; n < 3; n++)
                    {
                        //1개의 픽셀로부터 저장된 문자를 추출하기 위해 각각의 RGB의 LSB를 추출하는 과정을 3번 반복하면,
                        //8bit를 얻어낼 수 있고, 이를 통해 거꾸로 된 하나의 2진수 형태의 문자를 만들 수 있음
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
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

                        //3개의 픽셀로부터 RGB 각각의 LSB를 총 8개 추출하여, 1byte의 숫자(거꾸로 된 하나의 2진수 형태의 문자를 말함)를 추출해낸 경우
                        if (colorUnitIndex % 8 == 0)
                        {
                            //reverseBits 함수를 이용하여 거꾸로 저장된 문자를 다시 거꾸로 하여, 올바르게 읽어옴
                            //charValue = reverseBites(10000110); -> charValue = "01100001" = 2진수 -> 10진수 97: 'a'를 나타냄
                            charValue = reverseBits(charValue);

                            //복구할 텍스트가 없는 경우 또는 텍스트를 모두 복구하여 8개의 zeros(0)에 해당되는 비트에 다다른 경우 바로 추출
                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                
                            //charValue 값이 0이 아니면 복구된 텍스트 c에 저장
                            char c = (char)charValue;

                            //복구된 텍스트(c)가 extractedText에 추가 저장됨
                            extractedText += c.ToString();
                        }
                    }
                }
            }

            //추출된 모든 텍스트 반환
            return extractedText;
        }

        //비트 복구 = 텍스트 추출
        public static int reverseBits(int n)
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                //n%2 = 0 또는 1
                //2진수의 덧셈 - 아래 숫자 모두 2진수
                    //1. 0+0 = 0
                    //2. 0+1 = 1
                    //3. 1+1 = 10
                //2진수의 곱셈 - 아래 숫자 모두 2진수
                    //1. 0*2 = 0
                    //2. 1*2 = 10
                    //3. 10*2 = 100
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }
    }
}
