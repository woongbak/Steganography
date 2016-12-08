using System;
using System.Drawing;

// Steganography 네임스페이스 정의
namespace Steganography
{
    class SteganographyHelper // SteganographyHelper
    {
        // 스테가노그래피 도구의 상태를 나타냄 (숨김 모드, 숨김 완료 모드)
        public enum State
        {
            Hiding, // 숨김 모드 (0)
            Filling_With_Zeros // 숨김 완료 모드 (1)
        };

        /* 
         * 메소드명 : embedText
         * 매개변수 : text (문자열), bmp (Bitmap 객체)
         * 설명 : text를 숨기기 위한 메소드
        */
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding; // 상태를 숨김 모드로 설정

            int charIndex = 0; // 숨긴 문자 갯수를 저장하는 변수

            int charValue = 0; // 숨길 대상의 문자를 저장하는 변수

            long pixelElementIndex = 0; // RGB 값을 저장하기 위한 변수

            int zeros = 0; // 문자 하나의 비트 수

            int R = 0, G = 0, B = 0; // RGB값을 모두 0으로 초기화

            for (int i = 0; i < bmp.Height; i++) // 비트맵 객체의 세로 길이(높이)부터 1비트씩 스캔
            {
                for (int j = 0; j < bmp.Width; j++) // 비트맵 객체의 가로 길이(너비)부터 1비트씩 스캔
                {
                    Color pixel = bmp.GetPixel(j, i); // 해당 비트맵 객체의 가로 세로 위치를 이용해 색 정보를 리턴

                    // 해당 픽셀의 RGB값에 해당하는 각각의 LSB 비트를 0으로 초기화한 값을 저장
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    // 
                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0) // 만약 pixelElementIndex가 0 또는 8이라면
                        {
                            // 만약 해당 상태가 숨김 완료 모드이고 8비트 모두 스캔 완료하였다면
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                // 변경해야 할 RGB값이 남은 경우
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    // 해당 픽셀에 RGB값을 세팅한다
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                // 비트맵 객체 반환
                                return bmp;
                            }

                            // 만약 숨길 대상의 문자열을 다 저장한 경우
                            if (charIndex >= text.Length)
                            {
                                // 문자열이 다 숨겨진 상태인 것으로 상태 값 변환
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                // 숨길 문자열 하나를 가져오고 인덱스 1 증가
                                charValue = text[charIndex++];
                            }
                        }

                        // pixelElementIndex % 3 값에 따라서 분류
                        switch (pixelElementIndex % 3)
                        {
                            // 만약 결과값이 0이라면 : R인 경우
                            case 0:
                                {
                                    if (state == State.Hiding) // 만약 숨김 모드라면
                                    {
                                        R += charValue % 2; // R값에 문자 저장
                                        charValue /= 2; // 저장된 값 제거
                                    }
                                } break;
                            case 1: // G인 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; // G값에 문자 저장

                                        charValue /= 2; // 저장된 값 제거
                                    }
                                } break;
                            case 2: // B인 경우
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2; // B값에 문자 저장

                                        charValue /= 2; // 저장된 값 제거
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // RGB에 해당하는 값을 세팅
                                } break;
                        }

                        pixelElementIndex++; // pixelElementIndex값 1 증가

                        if (state == State.Filling_With_Zeros) // 만약 문자 숨김이 완료되었다면
                        {
                            zeros++; // zeros 값 증가
                        }
                    }
                }
            }

            return bmp; // 비트맵 객체 반환
        }

        /* 
         * 메소드명 : extractText
         * 매개변수 : bmp (Bitmap 객체)
         * 설명 : 숨겨진 텍스트를 추출하기 위한 메소드
        */
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0; // 문자 하나를 저장할 변수

            string extractedText = String.Empty; // 추출된 문자열을 저장하는 변수

            // 해당 비트맵 객체의 높이를 순차적으로 순회
            for (int i = 0; i < bmp.Height; i++)
            {
                // 해당 비트맵 객체의 너비를 순차적으로 순회
                for (int j = 0; j < bmp.Width; j++)
                {
                    // 픽셀의 값을 가져온다
                    Color pixel = bmp.GetPixel(j, i);

                    // RGB 값의 마지막을 가져온다
                    for (int n = 0; n < 3; n++)
                    {
                        // colorUnitIndex%3 값에 따라서 수행
                        switch (colorUnitIndex % 3)
                        {
                            case 0: // 만약 R인 경우
                                {
                                    // charvalue 1 시프트 한 뒤 숨긴 문자 삽입
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1: // 만약 G인 경우
                                {
                                    // charvalue 1 시프트 한 뒤 숨긴 문자 삽입
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2: // 만약 B인 경우
                                {
                                    // charvalue 1 시프트 한 뒤 숨긴 문자 삽입
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++; // colorUnitIndex 1 증가

                        // 만약 colorUnitIndex가 맨 처음이거나 맨 마지막 비트인 경우 (추출 완료된 경우)
                        if (colorUnitIndex % 8 == 0)
                        {
                            // 정상적으로 값을 되돌린다
                            charValue = reverseBits(charValue);

                            // 만약 되돌린 값이 없다면
                            if (charValue == 0)
                            {
                                // 숨긴 메시지 리턴
                                return extractedText;
                            }
                            // 찾은 문자열을 문자열에 넣기 위해서 형변환
                            char c = (char)charValue;

                            // 형변환한 문자를 문자열에 삽입
                            extractedText += c.ToString();
                        }
                    }
                }
            }

            return extractedText; // 추출된 문자열 리턴
        }

        /* 
         * 메소드명 : reverseBits
         * 매개변수 : n (정수형)
         * 설명 : 비트를 반대로 뒤집어 정렬하기 위한 메소드
        */
        public static int reverseBits(int n)
        {
            int result = 0; // 결과를 저장하는 변수

            // 해당 비트의 0-8비트 모두 스캔
            for (int i = 0; i < 8; i++)
            {
                // result 를 왼쪽으로 하나 시프트 한 뒤 n 값의 가장 왼쪽에 삽입
                result = result * 2 + n % 2;

                n /= 2; // n 하나 오른쪽으로 시프트
            }

            return result; // 결과값 리턴
        }
    }
}
