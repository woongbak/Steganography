using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State //현재 모드 알려줄 플레그로 사용할 열거형 정의
        {
            Hiding,                 //숨김모드
            Filling_With_Zeros      //0을 채우는 모드
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        //텍스트를 숨기는 함수
        {
            State state = State.Hiding;
            //상태 플레그 선언 후 숨김 모드로 세팅

            int charIndex = 0;
            //숨길 문자열의 index 저장할 변수, 0으로 초기화

            int charValue = 0;
            //숨길 문자열의 문자 저장할 변수, 0으로 초기화

            long pixelElementIndex = 0;
            //숨긴 LSB 비트 수 카운트에 사용할 변수, 0으로 초기화

            int zeros = 0;
            //숨긴 데이터가 끝난 후 8bits를 0으로 세팅할 때
            //0으로 세팅한 비트 수 카운트에 사용할 변수, 0으로 초기화

            int R = 0, G = 0, B = 0;
            //픽셀에서 R, G, B 값 가져와 저장할 변수, 0으로 초기화

            for (int i = 0; i < bmp.Height; i++)//픽셀 높이 인덱스 조정할 반복문
            {
                for (int j = 0; j < bmp.Width; j++)//픽셀 너비 인덱스 조정할 반복문
                {
                    Color pixel = bmp.GetPixel(j, i);
                    //데이터를 숨기려는 이미지의 j,i 위치의 픽셀 가져와 저장

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    //가져온 픽셀의 R, G, B 채널 값 저장 후, LSB 0으로 세팅

                    for (int n = 0; n < 3; n++)
                    //각 픽셀에 R, G, B 채널의 LSB 값 변경하기 위한 반복문
                    {
                        if (pixelElementIndex % 8 == 0)
                        //pixelElementIndex % 8 이 0 이면 8bit를 변경한 것이므로
                        //문자 하나를 숨기는 것이 끝났는지 검사하는 조건문
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            //상태 플레그가 0을 채우는 모드인지 검사하고
                            //데이터 숨김이 끝났음을 알리기 위해 8bit 0 세팅이 끝났는지 검사하는 조건문
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                //한 픽셀의 R, G, B 채널의 LSB를 모두
                                //변경하기 전에 숨기려는 문자가 끝났는지 검사
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                    //j,i 위치 픽셀에 변경한 R, G, B 채널 값 저장
                                }

                                return bmp;
                                //데이터 숨긴 이미지 반환
                            }

                            if (charIndex >= text.Length)
                            //숨긴 문자열의 index가 문자열 길이와 같거나 큰지 검사
                            {
                                state = State.Filling_With_Zeros;
                                //상태 플레그를 0 채우는 모드로 변경
                            }
                            else
                            {
                                charValue = text[charIndex++];
                                //다음 숨길 문자 저장, 숨길 문자열의 index 1 증가
                            }
                        }

                        switch (pixelElementIndex % 3)
                        //R, G, B 중 어떤 채널을 변경할지 정하는 스위치문
                        {
                            case 0:
                            //R 채널 변경해야 할 경우
                                {
                                    if (state == State.Hiding)
                                    //상태 플레그가 숨김 모드인지 검사
                                    {
                                        R += charValue % 2; //R 채널 LSB에 숨길 문자 bit 하나씩 저장
                                        charValue /= 2;     //10진수 2진수로 바꾸기 위해 2로 나눈 몫을 저장
                                    }
                                } break;
                            case 1:
                            //G 채널 변경해야 할 경우
                                {
                                    if (state == State.Hiding)
                                    //상태 플레그가 숨김 모드인지 검사9
                                    {
                                        G += charValue % 2; //G 채널 LSB에 숨길 문자 bit 하나씩 저장
                                        charValue /= 2;     //10진수 2진수로 바꾸기 위해 2로 나눈 몫을 저장
                                    }
                                } break;
                            case 2:
                            //B 채널 변경해야 할 경우
                                {
                                    if (state == State.Hiding)
                                    //상태 플레그가 숨김 모드인지 검사
                                    {
                                        B += charValue % 2; //B 채널 LSB에 숨길 문자 bit 하나씩 저장
                                        charValue /= 2;     //10진수 2진수로 바꾸기 위해 2로 나눈 몫을 저장
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                    //R, G, B 채널의 LSB 값을 모두 변경한 후 해당 위치 픽셀에 저장
                                } break;
                        }

                        pixelElementIndex++;
                        //변경한 LSB 비트 개수 증가

                        if (state == State.Filling_With_Zeros)
                        //플레그가 0을 채우는 모드인지 아닌지를 검사하는 조건문
                        {
                            zeros++;
                            //0으로 채운 LSB 개수 증가
                        }
                    }
                }
            }

            return bmp;
            //데이터 숨긴 이미지 반환
        }

        public static string extractText(Bitmap bmp)
        //텍스트를 추출하는 함수
        {
            int colorUnitIndex = 0;
            //읽어온 LSB 비트 수 카운트에 사용할 변수, 0으로 초기화

            int charValue = 0;
            //추출한 문자 값 저장할 변수, 0으로 초기화

            string extractedText = String.Empty;
            //추출한 문자 저장할 배열, 빈 배열로 초기화

            for (int i = 0; i < bmp.Height; i++)//픽셀 높이 인덱스 조정할 반복문
            {
                for (int j = 0; j < bmp.Width; j++)//픽셀 너비 인덱스 조정할 반복문
                {
                    Color pixel = bmp.GetPixel(j, i);
                    //데이터를 숨기려는 이미지의 j,i 위치의 픽셀 가져와 저장

                    for (int n = 0; n < 3; n++)
                    //각 픽셀에 R, G, B 채널의 LSB 값 읽어오기 위한 반복문
                    {
                        switch (colorUnitIndex % 3)
                        //R, G, B 중 어떤 채널에서 읽어올지 정하는 스위치문
                        {
                            case 0:
                            //R 채널 변경해야 할 경우
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                    //2진수 bit 값을 10진수로 바꾸기 위해 2곱하고 color 채널의 LSB 값 더하기
                                } break;
                            case 1:
                            //G 채널 변경해야 할 경우
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                    //2진수 bit 값을 10진수로 바꾸기 위해 2곱하고 color 채널의 LSB 값 더하기
                                }
                                break;
                            case 2:
                            //B 채널 변경해야 할 경우
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                    //2진수 bit 값을 10진수로 바꾸기 위해 2곱하고 color 채널의 LSB 값 더하기
                                }
                                break;
                        }

                        colorUnitIndex++;
                        //읽어온 LSB 개수 증가

                        if (colorUnitIndex % 8 == 0)
                        //colorUnitIndex % 8 값이 0이면 8bit의 값을 읽어온 것이므로
                        //문자 하나의 값을 모두 추출하였는지 검사
                        {
                            charValue = reverseBits(charValue);
                            //문자를 숨길 때 숨길 문자의 LSB 부터 MSB 순서로 저장하였으므로
                            //추출해온 bit 값은 원래 값과 반대여서 순서를 바꿔주는 기능

                            if (charValue == 0)
                            //문자를 숨길 때 끝까지 숨긴 다음 8bit를 0으로 세팅하므로
                            //읽어온 문자의 값이 0이면 추출을 종료
                            {
                                return extractedText;
                                //추출한 텍스트 반환
                            }
                            char c = (char)charValue;
                            //읽어온 값을 char형으로 형 변환하여 char형 변수 c에 저장

                            extractedText += c.ToString();
                            //c에 저장된 문자를 반환할 배열에 추가
                        }
                    }
                }
            }

            return extractedText;
            //추출한 텍스트 반환
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
