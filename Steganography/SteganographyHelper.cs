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

        public static Bitmap embedText(string text, Bitmap bmp) // Hiding된 상태인지를 알기위한 코드
        {
            State state = State.Hiding;

            int charIndex = 0; //조건이 맞을경우 문자열 index값

            int charValue = 0; // text의 문자의 아스키코드값

            long pixelElementIndex = 0; //픽셀의 개수를 세는 값

            int zeros = 0;  //8번씩 text를 보고 초기화

            int R = 0, G = 0, B = 0; // 픽셀에서 RGB값 초기화

            for (int i = 0; i < bmp.Height; i++)  // for(이중)을 통한 픽셀 검사
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); //color 픽셀의 i,j를 이용한 좌표값을 넣는다.

                    R = pixel.R - pixel.R % 2; //r값에서 나머지인 0또는 1을 뺀다 LSB설정
                    G = pixel.G - pixel.G % 2; //g값에서 나머지인 0또는 1을 뺀다 LSB설정
                    B = pixel.B - pixel.B % 2; //b값에서 나머지인 0또는 1을 뺀다 LSB설정

                    for (int n = 0; n < 3; n++) //pixel은 3byte
                    {
                        if (pixelElementIndex % 8 == 0) // 8을 주기로 
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)// 해당 조건이 맞을경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // 마지막 픽셀
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//RGB값을 현재로 조정해준다.
                                }

                                return bmp;//Steganography완료 후 빠져나온다
                            }

                            if (charIndex >= text.Length) // 더이상 문자가 끝날 경우
                            {
                                state = State.Filling_With_Zeros; // 변환해준다
                            }
                            else //모두 아닐경우
                            {
                                charValue = text[charIndex++]; //다음문자로 이동
                            }
                        }

                        switch (pixelElementIndex % 3) // RGB를 구분해서
                        {
                            case 0: //R일경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2; //2로 나눈 나머지를 추가
                                        charValue /= 2; // 마지막 비트를 제거하기위해 2로 나눈다.
                                    }
                                } break;
                            case 1: //G일경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; // R과 같이

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: //B일경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2; //R과 같이 

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // i,j위치를 이용해서 RGB값을 저장한다
                                } break;
                        }

                        pixelElementIndex++; //인덱스 증가

                        if (state == State.Filling_With_Zeros) // 8이 되기를 위해 1씩 증가시킨다
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp; //이미지 변환
        }

        public static string extractText(Bitmap bmp) //text 추출
        {
            int colorUnitIndex = 0; //RGB 인덱스 초기화
            int charValue = 0; //문자의 아스키코드 값

            string extractedText = String.Empty; //이미지에서 추출한 text

            for (int i = 0; i < bmp.Height; i++) //for문을 이용해서 반복 세로
            {
                for (int j = 0; j < bmp.Width; j++) //for문을 이용해서 반복 가로
                {
                    Color pixel = bmp.GetPixel(j, i); //i,j를 이용해서 픽셀에 저장
                    for (int n = 0; n < 3; n++) //RGB반복
                    {
                        switch (colorUnitIndex % 3) //RGB를 구분
                        {
                            case 0://R일경우
                                {
                                    charValue = charValue * 2 + pixel.R % 2;//LSB를 아스키코드값으로 문자 값을 준다.
                                } break;
                            case 1://G일경우
                                {
                                    charValue = charValue * 2 + pixel.G % 2;//LSB를 아스키코드값으로 문자 값을 준다.
                                } break;
                            case 2://B일경우
                                {
                                    charValue = charValue * 2 + pixel.B % 2;//LSB를 아스키코드값으로 문자 값을 준다.
                                } break;
                        }

                        colorUnitIndex++; //인덱스 증가

                        if (colorUnitIndex % 8 == 0) //8로 나누어질 경우
                        {
                            charValue = reverseBits(charValue); //reverse 시켜서 저장한다.

                            if (charValue == 0) // 해당 조건이면
                            {
                                return extractedText; //나온다
                            }
                            char c = (char)charValue; // char 강제형변환

                            extractedText += c.ToString(); //결과를 얻는다
                        }
                    }
                }
            }

            return extractedText; //text출력 완료후 반환
        }

        public static int reverseBits(int n) //reverse함수
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
