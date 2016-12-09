using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper //steganogrphy , steganographyHelper 선언
    {
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding; //현재 상태를 hiding으로 설정

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0; //정수형 변수들 선언 

            for (int i = 0; i < bmp.Height; i++) //이미지의 높이 반복분
            {
                for (int j = 0; j < bmp.Width; j++) //이미지 너비 반복문 
                {
                    Color pixel = bmp.GetPixel(j, i); //너비,높이로 픽셀값(넓이) 구해서 get 

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2; //RGB값의 마지막 비트인 LSB를 0으로 설정 

                    for (int n = 0; n < 3; n++) //R,G,B 각각으로, 총 3번의 반복문을 수행 
                    {
                        if (pixelElementIndex % 8 == 0) //픽셀요소(8bit)를 입력한 경우
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //state가, filling_with_zeros고, zeros가 8 인경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // pixelElementIndex에서 1을뺀 수의 3으로 나눈 나머지가 2보다 작은 경우
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //픽셀에 세팅
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length) //charindex가 text.length보다 큰 경우 (세팅이 완료된 경우)
                            {
                                state = State.Filling_With_Zeros; //filling_with_zeros상태변경함 0으로 채워진다.
                            }
                            else
                            {
                                charValue = text[charIndex++]; //아닌경우, charvalue에 charindex 가 증가한 값을 넣음으로써 , 더 실행할 수 있도록 한다.
                            }
                        }

                        switch (pixelElementIndex % 3) // 3= R,G,B
                        {
                            case 0: //R
                                {
                                    if (state == State.Hiding) //숨겨진 상태라면
                                    {
                                        R += charValue % 2; //R의 LSB charValue를 2로 나눈 나머지를 더함-마지막비트
                                        charValue /= 2; //다음비트로 이동
                                    }
                                }
                                break;
                            case 1: //G
                                {
                                    if (state == State.Hiding) //숨겨진 상태라면
                                    {
                                        G += charValue % 2; // G의 LSB charValue를 2로 나눈 나머지를 더함 - 마지막비트

                                        charValue /= 2; //다음비트로 이동
                                    }
                                }
                                break;
                            case 2: //B
                                {
                                    if (state == State.Hiding) //숨겨진 상태라면 
                                    {
                                        B += charValue % 2; // B의 LSB charValue를 2로 나눈 나머지를 더함 - 마지막비트

                                        charValue /= 2;  //다음비트로 이동
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //구해진 RGB값 세팅
                                }
                                break;
                        }

                        pixelElementIndex++; //pixelelementindex값 증가-다음으로 이동  

                        if (state == State.Filling_With_Zeros) //만약 데이터를 모두 숨겨서 filling_with_zeros상태라면 zeros값 증가
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp) //이미지에서 text를 추출하기 위한 함수 
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; //변수선언 

            for (int i = 0; i < bmp.Height; i++) //높이 및 너비를 구해서, 세팅값에 저장
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) //3 = R,G,B를 의미
                        {
                            case 0: //R
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //숨기는 것과 반대-charValue는  charValue2배 한 후 R을 2로 나눈 나머지를 더한다.
                                    break;
                            case 1: //G
                                {
                                    charValue = charValue * 2 + pixel.G % 2; //숨기는 것과 반대-charValue는  charValue2배 한 후 G을 2로 나눈 나머지를 더한다.
                                }
                                break;
                            case 2: //B
                                {
                                    charValue = charValue * 2 + pixel.B % 2; //숨기는 것과 반대-charValue는  charValue2배 한 후 B을 2로 나눈 나머지를 더한다.
                                }
                                break;
                        }

                        colorUnitIndex++; // colorunitindex를 증가 

                        if (colorUnitIndex % 8 == 0) //한바이트를 다 읽었을 때 
                        {
                            charValue = reverseBits(charValue); // 숨겨진 픽셀 비트값은 역순이므로, reverse사용

                            if (charValue == 0) //charvalue값이 0이여서 더이상 추출할 것이 없을 경우, extractedtext값 반환
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;  //추출된 문자는 변수c에 저장하여 tostring에 저장

                            extractedText += c.ToString(); //tostring을 누적해서 extractedtext값에 저장
                        }
                    }
                }
            }

            return extractedText; //extractedtext 리턴 
        }

        public static int reverseBits(int n) //비트를 역순으로 출력하기위한 함수
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2; //한바이트 단위로 역순출력 수행 

                n /= 2;
            }

            return result; //역순으로 변환한 결과 반환
        }
    }
}