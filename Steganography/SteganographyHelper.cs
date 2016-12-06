using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State //상태 플래그, 숨기기 상태, 0으로 채워야할 상태(다 숨기고난후)
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp)//입력한 스트링과 이미지를 변수로 받음.
        {
            State state = State.Hiding; // state를 hiding으로 설정

            int charIndex = 0; // 숨겨줄 스트링 index 나타내는 변수

            int charValue = 0; // 숨겨질 문자 저장할 변수

            long pixelElementIndex = 0; //픽셀 데이터 인덱스, 픽셀데이터 카운트를 위해 선언

            int zeros = 0;// 숨길 스트링이 다 끝난후 8비트를 0으로 채워주는데, 이 횟수를 카운트 할 변수

            int R = 0, G = 0, B = 0; // 픽셀당 RGB값을 나타내기위한 변수

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); //bmp 파일넓이와 높이만큼 for문을 루프하여 픽셀값을 가져오는것

                    R = pixel.R - pixel.R % 2; //픽셀의 각 채널값의 마지막비트를 0으로 만들어 주는 과정
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) // RGB 3개의 채널에 대해 코드 수행
                    {
                        if (pixelElementIndex % 8 == 0)//한개의 문자 1byte=8bit를 다 입력한 경우 마다 체크
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 입력받은 텍스트를 다 입력하고, state가 변경된후, zeros가 8번 증가된 경우                            {
                                if ((pixelElementIndex - 1) % 3 < 2) //switch 문 case 2의 bmp.SetPixel을 통과하지 못한경우 
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 8비트의 0을 이미지에 적용
                                }

                                return bmp;// bmp 파일 리턴
                            }

                            if (charIndex >= text.Length) // 입력받은 스트링의 길이를 다 입력한 경우
                            {
                                state = State.Filling_With_Zeros; // 0으로 채우는 상태로 변경
                            }
                            else
                            {
                                charValue = text[charIndex++]; // 아닌 경우 새로 입력할 문자를 char value에 입력
                            }
                       }

                        switch (pixelElementIndex % 3)  //RGB 채널에 돌아가며 데이터 숨기기
                        {
                            case 0:
                                {
                                    if (state == State.Hiding) // 숨기기 상태일때(숨길 데이터가 남아있을때) 
                                    {
                                        R += charValue % 2; // 문자의 해당 비트부터 저장
                                        charValue /= 2; // 2로 나눔으로써 문자의 다음 비트로 이동
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

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //해당 픽셀의 변경된 RGB값을 픽셀에 적용
                                } break;
                        }

                        pixelElementIndex++;// 다음 채널로 이동
                         /* 동작원리 charValue 가 97 (==a) 인경우
                          * 97 % 2 == 1 -> 97 / 2 == 48 pixelElementindex == 0 -> 1
                          * 48 % 2 == 0 -> 48 / 2 == 24 pixelElementindex == 1 -> 2
                          * 24 % 2 == 0 -> 24 / 2 == 12 pixelElementindex == 2 -> 3
                          * 12 % 2 == 0 -> 12 / 2 == 6  pixelElementindex == 3 -> 4
                          * 6  % 2 == 0 -> 6  / 2 == 3  pixelElementindex == 4 -> 5
                          * 3  % 2 == 1 -> 3  / 2 == 1  pixelElementindex == 5 -> 6
                          * 1  % 2 == 1 -> 1  / 2 == 0  pixelElementindex == 6 -> 7
                          * 0  % 2 == 0 -> 0  / 2 == 0  pixelElementindex == 7 -> 8 => 위 if 문에서 다음 문자열로 이동
                          * 저장된 것을 그대로 읽으면 10000110 -> 뒤짚으면 01100001인데 아스키 코드에서 맨앞에 비트는 문자 구분에는 영향을 끼치지 않으므로/
                          * 97 = 1100001 -> 즉, 문자 비트열이 역방향으로 들어간 것을 알 수 있다.
                          */

                        if (state == State.Filling_With_Zeros) // 모든 텍스트를 숨기고 난후 state가 filling_with_Zeros로 변경된 경우
                        {
                            zeros++;//zeros를 하나씩 증가
                        }
                    }
                }


            return bmp;// bmp 파일의 해상도(width와 height)를 다 사용한 경우 bmp 파일 리턴

        }
        public static string extractText(Bitmap bmp) // 비트맵 형식 /이미지에서 숨겨진 데이터 추출
        {
            int colorUnitIndex = 0; // 픽셀 비트단위 인덱스
            int charValue = 0; // 숨겨진 문자 저장하는 변수

            string extractedText = String.Empty;//추출된 스트링을 저장할 변수

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)// 이미지의 높이와 넓이만큼 픽셀을 대상으로 수행
                {
                    Color pixel = bmp.GetPixel(j, i);// 픽셀 데이터를 받아 클래스 변수에 저장
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)// R,G,B 채널당 수행
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;//숨기는과정의 역과정을 통해 각 채널의 마지막 비트값을 더해 변수에 넣어주는 과정
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

                        colorUnitIndex++; //인덱스 카운트를 하나 증가

                        if (colorUnitIndex % 8 == 0) // 8번의 수행, 즉 8비트 1byte 하나의 문자에 대한 수행이 끝난경우
                        {
                            charValue = reverseBits(charValue);// 역방향으로 들어가있기 때문에 문자비트를 리버스 시켜 제대로된 문자로 표현

                            if (charValue == 0)
                            {
                                return extractedText;//계산된 비트열이 0, 즉 데이터가 끝났다면 추출된 텍스트를 리턴
                            }
                            char c = (char)charValue; // 변수를 캐릭터형으로 변환

                            extractedText += c.ToString(); // 변환된 캐릭터형 변수를 반환해줄 스트링에 추가 
                        }
                    }
                }
            }

            return extractedText;// 이미지의 모든 픽셀을 대상으로 수행이 끝난경우 텍스트 리턴
        }

        public static int reverseBits(int n) //입력받은 변수를 차례대로 뒤집어서 표현 ex) 10101010 -> 01010101 
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
