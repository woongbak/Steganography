using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding,                     // 데이터 숨길 것이 있음
            Filling_With_Zeros      // 데이터 숨길 것이 없음
        };

        // text = data,  bmp = 숨길 이미지, 데이터 숨기는 함수임
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding; // 데이터 숨길 것이 있음으로 설정

            int charIndex = 0;              // 숨긴 문자 수

            int charValue = 0;              // 숨긴 문자

            long pixelElementIndex = 0;     // 카운트 인덱스

            int zeros = 0;  // 숨길것이 없는 상태에서 픽셀을 순회하면 1씩 증가

            int R = 0, G = 0, B = 0;

            // 이미지의 사이즈 만큼의 모든 픽셀을 이중 for문을 통해서 순회
            for (int i = 0; i < bmp.Height; i++)            // 이미지의 높이 만큼 반복문 실행
            {
                for (int j = 0; j < bmp.Width; j++)         // 이미지의 넓이 만큼 반복문 실행
                {
                    Color pixel = bmp.GetPixel(j, i);       // 픽셀을 가져옴

                    // pixel.R G B의 값을 이용해, 정수형 R, G, B에 새로운 값을 저장
                    // 즉 RGB값을 2진수로 봤을 때 , 마지막 값을 무조건 0으로 맞춰줌
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    // 한 픽셀의 R, G, B값을 반복문으로 확인하여 조건에 만족하면 그 값을 변경
                    for (int n = 0; n < 3; n++)     // 한 픽셀의 R, G, B 값을 반복
                    {
                        if (pixelElementIndex % 8 == 0) // 픽셀요소인덱스가 8번째 마다 이면
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)    // 숨길것이 없는 상태 + 숨길것이 없는상태에서 8번째이면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)    // 픽셀값을 설정하고 비트맵 반환 숨김 종료
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    // 픽셀의 정보를 설정
                                }

                                return bmp; // 위의 모든 조건이 만족하면 bmp 반환
                            }

                            if (charIndex >= text.Length)   // 현재 숨긴 문자가 숨길 문자의 길이보다 크거나 같으면
                            {
                                state = State.Filling_With_Zeros;   // 숨길것이 없는 상태로 상태 설정
                            }
                            else  //  아니라면 charValue에 숨길 문자를 대입
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3) // 픽셀요소인덱스를 3으로 나눈 나머지 값으로 switch문을 실행
                        {
                            case 0: // 나머지가 0이면
                                {
                                    if (state == State.Hiding)  // state가 숨길것이 있는 상태이면
                                    {
                                        R += charValue % 2; // R의 값을 변경
                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 1: // 나머지가 1이면
                                {
                                    if (state == State.Hiding) // state가 숨길것이 있는 상태이면
                                    {
                                        G += charValue % 2; // G의 값을 변경

                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if (state == State.Hiding) // state가 숨길것이 있는 상태이면
                                    {
                                        B += charValue % 2; // B의 값을 변경

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    // 변경된 RGB값으로 픽셀을 설정
                                }
                                break;
                        }

                        pixelElementIndex++;    // count를 해준다.

                        if (state == State.Filling_With_Zeros)  // 픽셀 반복중에 state가 숨길것이 없는 상태이면, zeros +1 해줌
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp)       // 데이터를 추출하는 함수
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)    // 이미지의 높이 만큼 반복 수행
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지의 너비 만큼 반복 수행
                {
                    Color pixel = bmp.GetPixel(j, i);   // 이미지의 픽셀을 얻어옴


                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) // // case마다 숨김에서 한 연산의 반대
                        {
                            case 0: // 나머지가 0이면
                                {
                                    charValue = charValue * 2 + pixel.R % 2;    // R픽셀을 2로 나눈나머지를 charValue*2의 값과 더한 후 charValue에 저장
                                }
                                break;
                            case 1: // 나머지가 1이면
                                {
                                    charValue = charValue * 2 + pixel.G % 2;    // G픽셀을 2로 나눈나머지를 charValue*2의 값과 더한 후 charValue에 저장
                                }
                                break;
                            case 2: // 나머지가 2이면
                                {
                                    charValue = charValue * 2 + pixel.B % 2;    // B픽셀을 2로 나눈나머지를 charValue*2의 값과 더한 후 charValue에 저장
                                }
                                break;
                        }

                        colorUnitIndex++;   //색 인덱스를 카운드 +1

                        if (colorUnitIndex % 8 == 0)    // 색인덱스을 8로 나눈 나머지가 0이면
                        {
                            charValue = reverseBits(charValue); // 비트를 뒤집어준다.

                            if (charValue == 0) // charValue의 값이 0이면
                            {
                                return extractedText;   // extractedText 스트링을 반환
                            }
                            char c = (char)charValue;   //charValue를 캐릭터 형으로 변경 해서 문자 하나를 얻어냄

                            extractedText += c.ToString();  // 그 문자를 extractedText에 추가
                        }
                    }
                }
            }

            return extractedText;   // 모든 반복문이 종료되면 extractedText를 반환
        }

        public static int reverseBits(int n) //비트를 뒤집어주는 함수, 비트를 뒤집어 숨겨진 데이터에 무엇이 저장되어 있는지 확인한다.
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
