using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State//열거형으로 선언하였으므로 값의 중복이 일어나지 않게 해줌
        {
            Hiding,//텍스트를 R,G,B에 저장
            Filling_With_Zeros//숨길 텍스트에서 마지막 문자를 저장하고 나머지 R,G값은 0으로 저장
        };
        //이미지에 텍스트를 숨기기 위한 메소드
        public static Bitmap embedText(string text, Bitmap bmp)//Hidden text와 이미지의 비트맵을 매개변수로 받음
        {
            State state = State.Hiding;//state는 Hiding을 가리킴

            int charIndex = 0;//숨길 텍스트를 char 단위로 나타내기 위한 인덱스

            int charValue = 0;//숨길 텍스트를 char 단위로 나타낸 값

            long pixelElementIndex = 0;//숨길 텍스트를 char단위로 R,G,B에 저장하기 위한 변수

            int zeros = 0;//숨길 텍스트에서 마지막 문자를 카운트하는 변수

            int R = 0, G = 0, B = 0;//픽셀의 R,G,B값을 0으로 초기화
            //이미지의 BMP의 배열형태
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);//하나의 픽셀값을 저장

                    R = pixel.R - pixel.R % 2;//해당 픽셀의 R의 LSB를 0으로 저장
                    G = pixel.G - pixel.G % 2;//해당 픽셀의 G의 LSB를 0으로 저장
                    B = pixel.B - pixel.B % 2;//해당 픽셀의 B의 LSB를 0으로 저장

                    for (int n = 0; n < 3; n++)//해당 픽셀의 R,G,B세팅
                    {
                        if (pixelElementIndex % 8 == 0)//숨길 문자를 다 저장하여 다음으로 숨길 문자를 지정
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//다음으로 숨길 문자가 없는 경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)//해당 픽셀에 저장해야할 R,G값이 남아있는 경우
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//마지막으로 숨긴 텍스트의 일부가 저장된 R,G로 해당 픽셀 세팅
                                }

                                return bmp;//스테가노그래피가 적용된 이미지의 비트맵을 반환
                            }

                            if (charIndex >= text.Length)//숨기려는 텍스트를 다 저장한 경우
                            {
                                state = State.Filling_With_Zeros;//state는 Filling_With_Zeros을 가리킴
                            }
                            else//숨기려는 텍스트가 아직 남은 경우
                            {
                                charValue = text[charIndex++];//숨길 문자를 저장하고 charIndex는 1증가
                            }
                        }

                        switch (pixelElementIndex % 3)//해당 픽셀의 R,G,B값에 돌아가면서 문자 저장
                        {
                            case 0://픽셀의 첫번째 원소 R
                                {
                                    if (state == State.Hiding)//state는 Hiding을 가리킬 경우
                                    {
                                        R += charValue % 2;//숨길 문자의 LSB를 해당 픽셀의 R의 LSB에 저장
                                        charValue /= 2;//숨길 문자를 비트단위로 오른쪽 시프트
                                    }
                                } break;
                            case 1://픽셀의 두번째 원소 G
                                {
                                    if (state == State.Hiding)//state는 Hiding을 가리킬 경우
                                    {
                                        G += charValue % 2;//숨길 문자의 LSB를 해당 픽셀의 G의 LSB에 저장

                                        charValue /= 2;//숨길 문자를 비트단위로 오른쪽 시프트
                                    }
                                } break;
                            case 2://픽셀의 세번째 원소 B
                                {
                                    if (state == State.Hiding)//state는 Hiding을 가리킬 경우
                                    {
                                        B += charValue % 2;//숨길 문자의 LSB를 해당 픽셀의 B의 LSB에 저장

                                        charValue /= 2;//숨길 문자를 비트단위로 오른쪽 시프트
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//숨긴 텍스트의 일부가 저장된 R,G,B로 해당 픽셀 세팅
                                } break;
                        }

                        pixelElementIndex++;//pixelElementIndex가 1증가

                        if (state == State.Filling_With_Zeros)//state가 Filling_With_Zeros을 가리킬 경우
                        {
                            zeros++;//zeros가 1증가
                        }
                    }
                }
            }

            return bmp;//스테가노그래피가 적용된 이미지의 비트맵을 반환
        }
        //이미지에서 텍스트를 추출하기 위한 메소드
        public static string extractText(Bitmap bmp)//이미지의 비트맵을 매개변수로 받아옴
        {
            int colorUnitIndex = 0;//숨겨진 텍스트를 char단위로 R,G,B에서 추출하기 위한 변수
            int charValue = 0;//숨겨진 텍스트를 char 단위로 나타낸 값

            string extractedText = String.Empty;//추출된 텍스트를 저장하기 위한 변수
            //이미지의 BMP의 배열형태
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);//하나의 픽셀값을 저장
                    for (int n = 0; n < 3; n++)//해당 픽셀의 R,G,B세팅
                    {
                        switch (colorUnitIndex % 3)//해당 픽셀의 R,G,B값에 돌아가면서 문자 추출
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;//해당 픽셀의 R의 LSB값을 추출하여 charValue에 저장
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;//해당 픽셀의 G의 LSB값을 추출하여 charValue에 저장
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;//해당 픽셀의 B의 LSB값을 추출하여 charValue에 저장
                                } break;
                        }

                        colorUnitIndex++;//colorUnitIndex값을 1증가

                        if (colorUnitIndex % 8 == 0)//8번 돌면 문자가 추출됨
                        {
                            charValue = reverseBits(charValue);//거꾸로 저장되었던 비트열을 뒤집기 위한 메소드 호출

                            if (charValue == 0)//추출될 값이 더 이상 없는 경우
                            {
                                return extractedText;//추출된 텍스트값
                            }
                            char c = (char)charValue;//추출된 값을 문자형 char로 표현

                            extractedText += c.ToString();//추출된 문자를 문자열로 저장해 나감
                        }
                    }
                }
            }

            return extractedText;//추출된 텍스트값
        }

        public static int reverseBits(int n)//거꾸로 저장되었던 비트열을 뒤집기 위한 메소드
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
