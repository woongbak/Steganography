using System;
using System.Drawing;

// namespace Steganography 정의
namespace Steganography
{
    
    // class SteganographyHelper 정의
    class SteganographyHelper
    {
        // 2가지의 State 정의
        public enum State
        {
            Hiding, // 숨김 모드
            Filling_With_Zeros
        };

        // embedText 메소드 구현 (text 숨기는 메소드)
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding; // state는 숨김 상태

            int charIndex = 0; // 캐릭터 인덱스 값

            int charValue = 0; // 캐릭터 값

            long pixelElementIndex = 0; // 픽셀 인덱스 값

            int zeros = 0; // 비트 계산용

            int R = 0, G = 0, B = 0; // R, G, B 값

            for (int i = 0; i < bmp.Height; i++) // 이미지 높이 만큼 루프
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지 넓이 만큼 루프
                {
                    Color pixel = bmp.GetPixel(j, i); // 현재 (j, i)에 위치한 픽셀의 R,G,B 정보를 가져옴

                    //  현재 픽셀 R,G,B의 LSB를 0으로 초기화 시켜줌
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;


                    for (int n = 0; n < 3; n++) // 3번 루프를 돎
                    {
                        if (pixelElementIndex % 8 == 0) // 픽셀 인덱스 값의 나머지가 8인 경우 (글자의 비트가 남아있는 경우)
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 숨김 완료 상태이고 zeros가 8인 경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // 만약 변경해야할 RGB 값이 있는 경우
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // (j, i) 픽셀에 R,G,B를 적용시켜줌
                                }

                                return bmp; // bmp 반환
                            }

                            if (charIndex >= text.Length) // 만약 캐릭터 인덱스 값이 이미지 전체보다 큰 경우 (문자열을 다 저장한 경우)
                            {
                                state = State.Filling_With_Zeros; // 숨김 완료 상태로 전환
                            }
                            else // 아니면
                            {
                                charValue = text[charIndex++]; // 문자열에서 문자 하나 가져오고 인덱스 증가시킴
                            }
                        }

                        switch (pixelElementIndex % 3) // 픽셀 인덱스(R,G,B) 값에 따른 switch문
                        {
                            case 0: // 0 (R)인 경우
                                {
                                    if (state == State.Hiding) // 만약 숨김 상태인 경우
                                    {
                                        R += charValue % 2; // R에 문자의 마지막 비트 저장
                                        
                                        charValue /= 2; // 마지막 비트 없애기
                                    }
                                } break;
                            case 1: // 1 (G)인 경우
                                {
                                    if (state == State.Hiding) // 만약 숨김 상태이면
                                    {
                                        G += charValue % 2; // G에 문자의 마지막 비트 저장

                                        charValue /= 2; // 마지막 비트 없애기
                                    }
                                } break;
                            case 2: // 2 (B)인 경우
                                {
                                    if (state == State.Hiding) // 만약 숨김 상태이면
                                    {
                                        B += charValue % 2; // B에 문자의 마지막 비트 저장

                                        charValue /= 2; // 마지막 비트 없애기
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 현재 RGB 값을 pixel에 넣음
                                } break;
                        }

                        pixelElementIndex++; // 픽셀 인덱스에 1을 더함

                        if (state == State.Filling_With_Zeros) // 만약 숨김 완료 상태이면
                        {
                            zeros++; // zeros를 1 증가시킴
                        }
                    }
                }
            }

            return bmp; // bmp 반환
        }

        public static string extractText(Bitmap bmp) // extractText 메소드 구현 (문자열 추출 메소드)
        {
            int colorUnitIndex = 0; // R,G,B 인덱스
            int charValue = 0; // 캐릭터 값

            string extractedText = String.Empty; // 빈 스트링 선언 (추출한 문자열이 들어갈 변수)

            for (int i = 0; i < bmp.Height; i++) // 이미지의 높이만큼 루프를 돎
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지의 넓이만큼 루프를 돎
                {
                    Color pixel = bmp.GetPixel(j, i); // 현재 픽셀 (j, i)의 R,G,B 정보를 얻어옴
                    for (int n = 0; n < 3; n++) // 루프를 3번 돎
                    {
                        switch (colorUnitIndex % 3) // R,G,B 인덱스에 따른 switch문
                        {
                            case 0: // R 인덱스인 경우
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // 왼쪽으로 한 칸 쉬프트 후 마지막 비트에 숨긴 비트 삽입
                                } break;
                            case 1: // G 인덱스인 경우
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // 왼쪽으로 한 칸 쉬프트 후 마지막 비트에 숨긴 비트 삽입
                                } break;
                            case 2: // B 인덱스인 경우
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // 왼쪽으로 한 칸 쉬프트 후 마지막 비트에 숨긴 비트 삽입
                                } break;
                        }

                        colorUnitIndex++; // R,G,B 인덱스 증가시킴

                        if (colorUnitIndex % 8 == 0) // 모든 비트 추출이 된 경우
                        {
                            charValue = reverseBits(charValue); // 비트를 바꿈으로 원래 값 얻음

                            if (charValue == 0) // 마지막 문자인 경우
                            {
                                return extractedText; // 얻은 문자열 반환
                            }
                            char c = (char)charValue; // 얻은 문자를 넣기 위해 강제 형변환

                            extractedText += c.ToString(); // 얻은 문자를 extractedText에 저장
                        }
                    }
                }
            }

            return extractedText; // 얻은 문자열 반환
        }

        public static int reverseBits(int n) // reverseBits 메소드 선언(비트를 반대로 뒤집는 메소드)
        {
            int result = 0; // 결과값

            for (int i = 0; i < 8; i++) // 루프를 8번 돎
            {
                result = result * 2 + n % 2; // 왼쪽으로 한 칸 쉬프트 후 n의 가장 마지막 비트 삽입

                n /= 2; // n의 마지막 비트 없앰
            }

            return result; // 결과값 반환
        }
    }
}
