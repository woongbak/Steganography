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

        public static Bitmap embedText(string text, Bitmap bmp) // data를 동봉하는 함수
        {
            State state = State.Hiding; // 성탸룰 '감충'으로 설정

            int charIndex = 0;  // 감출 문자의 인덱스 변수

            int charValue = 0;  // 감출 문자의 값

            long pixelElementIndex = 0; //픽셀 요소의 인덱스 변수

            int zeros = 0;  // 문자 숨길 때, 사용할 픽셀 값 저장하는 변수

            int R = 0, G = 0, B = 0; // pixel의 red, green, blue 값

            for (int i = 0; i < bmp.Height; i++)    // 이미지의 높이만큼 반복문을 돌림
            {   
                for (int j = 0; j < bmp.Width; j++) // 이미지의 너비만큼 반복문을 돌림
                {    // 너비와 높이에 해당하는 픽셀을 저장시킨다.
                    Color pixel = bmp.GetPixel(j, i);                    
                    // Color의 R,G,B 값을 이용하여 정수형 R,G,B 값을 얻어 
                    R = pixel.R - pixel.R % 2; 
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    // 각각의 RGB 영역의 LSB 값을 0으로 만들기 위한 부분 
                    for (int n = 0; n < 3; n++)
                    {   // 각각의 픽셀들의 R, G, B값을 반복적으로 확인
                        if (pixelElementIndex % 8 == 0)
                        {   // 픽셀의 인덱스가 8의 나머지가 0이고
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {   //Filling_with_Zeros상태이고, zeros가 8이면
                                if ((pixelElementIndex - 1) % 3 <  2)
                                {   //조건 만족시, 이미지 픽셀을 RGB값으로 설정
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
                                    // 이미지 반환
                                return bmp;
                            }

                            if (charIndex >= text.Length)
                            {   // 가리키는 문자의 인덱스가 문자열의 길이와 같거나 크면
                                state = State.Filling_With_Zeros;
                            }   // state를 State의 Fillong_With_Zeros의 값으로 바꿈.
                            else
                            {   // 가리키는 문자의 인덱스가 문자열의 길이와 작으면
                                charValue = text[charIndex++];
                            }   // 현재 가리키고 있던 문자의 다른 문자값을 저장
                        }

                        switch (pixelElementIndex % 3)
                        {   // 픽셀요소의 인덱스 값을 3으로 나눈 나머지가
                            case 0:
                                {   //R일 경우
                                    if (state == State.Hiding)
                                    {   // 상태가 Hiding 일 경우
                                        R += charValue % 2; // 문자값을 2로 나눈 나머지를 더한 후
                                        charValue /= 2; // 문자값을 2로 나눈 몫 저장
                                    }
                                } break;
                            case 1:
                                {   //G일 경우
                                    if (state == State.Hiding)
                                    {    // 상태가 Hiding 일 경우
                                        G += charValue % 2;
                                            // G에 문자값을 2로 나눈 나머지 값을 더한 후
                                        charValue /= 2; // 문자값을 2로 나눈 몫 저장
                                    }
                                } break;
                            case 2:
                                {   //B일 경우
                                    if (state == State.Hiding)
                                    {    // 상태가 Hiding 일 경우
                                        B += charValue % 2;
                                        // B에 문자값을 2로 나눈 나머지 값을 더한 후
                                        charValue /= 2;// 문자값을 2로 나눈 몫 저장
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                        //픽셀 값을 RGB 값으로 설정함
                                } break;
                        }

                        pixelElementIndex++;
                        // 이미지 픽셀 요소의 인덱스 값을 증가시킴
                        if (state == State.Filling_With_Zeros)
                        {   //Filling_with_Zeros상태이면
                            zeros++;
                        }   //zeros 증가
                    }
                }
            }
            // 이미지 반환
            return bmp;
        }

        public static string extractText(Bitmap bmp)
        {       // data를 추출하는 함수
            int colorUnitIndex = 0; // 색 유닛의 인덱스 변수
            int charValue = 0;  // 문자값 변수

            string extractedText = String.Empty;
                // 문자열 저장 변수
            for (int i = 0; i < bmp.Height; i++)
            {   //이미지 높이만큼 
                for (int j = 0; j < bmp.Width; j++)
                {   // 이미지 너비 만큼 픽셀을 이중 반복문으로 확인
                    Color pixel = bmp.GetPixel(j, i); //(i,j)의 피셀 값을 저장
                    for (int n = 0; n < 3; n++)
                    {   
                        switch (colorUnitIndex % 3)
                        {   // 색 유닛 인덱스 값을 3으로 나눈 나머지가
                            case 0:
                                {       //R일 경우
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;    //문자 값을 1 시프트하고, 숨긴 문자 삽입
                            case 1:
                                {       //G일 경우
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;    //문자 값을 1 시프트하고, 숨긴 문자 삽입
                            case 2:
                                {       //B일 경우
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;    //문자 값을 1 시프트하고, 숨긴 문자 삽입
                        }

                        colorUnitIndex++;
                        // 색 유닛 인덱스 1 증가
                        if (colorUnitIndex % 8 == 0)
                        {   //색 유닛 인덱스가 0이거나 8로 나누어 떨어지면
                            charValue = reverseBits(charValue);
                            // 비트를 뒤집어서 문자 값에 저장
                            if (charValue == 0)
                            {   // 문자값이 0이면 
                                return extractedText;
                            }   // 추출할 메세지 반환
                            char c = (char)charValue;
                            // c에 문자값을 char형으로 변환하여 저장
                            extractedText += c.ToString();
                        }   //추출할 문자열에 C를 더함
                    }
                }
            }

            return extractedText;
        }   // 추출할 문자열 반환

        public static int reverseBits(int n)
        {   // 비트를 뒤집어주는 함수
            int result = 0;
            // 결과 저장하는 정수형 변수
            for (int i = 0; i < 8; i++)
            {   //0~ 8비트까지 검색하여 비트를 뒤집어주기 위한 반복문
                result = result * 2 + n % 2;

                n /= 2;
            }
            // 결과 값 반환
            return result;
        }
    }
}
