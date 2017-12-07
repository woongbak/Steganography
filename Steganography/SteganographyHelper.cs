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
        
        public static Bitmap embedText(string text, Bitmap bmp) // 메시지를 숨기기 위한 메소드
        {
            State state = State.Hiding; // state를 hiding으로 설정

            int charIndex = 0; // 숨길 문자열의 인덱스를 0으로 설정

            int charValue = 0; // 숨길 문자의 정수값

            long pixelElementIndex = 0; // 픽셀 인덱스를 0으로 설정

            int zeros = 0;

            int R = 0, G = 0, B = 0; // 데이터를 숨기는 데 활용할 R, G, B 픽셀 값 0으로 설정

            for (int i = 0; i < bmp.Height; i++) // bmp의 세로 길이 만큼 for문 실행
            {
                for (int j = 0; j < bmp.Width; j++) // bmp의 가로 길이 만큼 for문 실행
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀의 (j,i)위치의 bmp 가져오기

                    // 각 픽셀(R, G, B)의 LSB 값을 0으로 설정
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) // 각 픽셀의 R, G, B의 LSB 값 처리를 위해 for문 3번씩 실행
                    {
                        if (pixelElementIndex % 8 == 0) // 한 문자에 대한 처리를 마쳤을 경우
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) // 모든 문자를 처리했을 경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // B 채널까지 모두 확인했으면
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // (j,i)위치에 R, G, B 값 저장
                                }

                                return bmp; // 메시지가 숨겨진 사진 반환
                            }

                            if (charIndex >= text.Length) // 문자의 인덱스가 숨길 메시지 문자열 크기보다 크거나 같으면
                            {
                                state = State.Filling_With_Zeros; // state를 0으로 가득 찬 상태로 변경
                            }
                            else
                            {
                                charValue = text[charIndex++]; // charValue에 메시지의 다음 인덱스를 저장
                            }
                        }

                        switch (pixelElementIndex % 3) // R, G, B 중 어디에 숨길 메시지를 저장할지 결정
                        {
                            case 0: // R
                                {
                                    if (state == State.Hiding) // state가 숨김 상태이면
                                    {
                                        R += charValue % 2; // R에 최하위 비트 저장
                                        charValue /= 2; // charValue를 2로 나눔
                                    }
                                } break;
                            case 1: // G
                                {
                                    if (state == State.Hiding) // state가 숨김 상태이면
                                    {
                                        G += charValue % 2; // G에 최하위 비트 저장

                                        charValue /= 2; // charValue를 2로 나눔
                                    }
                                } break;
                            case 2: // B
                                {
                                    if (state == State.Hiding) // state가 숨김 상태이면
                                    {
                                        B += charValue % 2; // B에 최하위 비트 저장

                                        charValue /= 2; // charValue를 2로 나눔
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // bmp의 (j,i)위치 픽셀에 R, G, B 값 저장
                                } break;
                        }

                        pixelElementIndex++; // 픽셀 인덱스 1 증가

                        if (state == State.Filling_With_Zeros) // state가 0으로 가득 찬 상태이면
                        {
                            zeros++; // zeros 1 증가
                        }
                    }
                }
            }

            return bmp; // 사진 반환
        }

        public static string extractText(Bitmap bmp) // 메시지를 추출하기 위한 메소드
        {
            int colorUnitIndex = 0;
            int charValue = 0; // 추출할 문자의 정수값

            string extractedText = String.Empty; // 추출할 메시지를 저장할 문자열 빈 문자열로 초기화

            for (int i = 0; i < bmp.Height; i++) // bmp의 세로 길이 만큼 for문 실행
            {
                for (int j = 0; j < bmp.Width; j++) // bmp 가로 길이 만큼 for문 실행
                {
                    Color pixel = bmp.GetPixel(j, i); // (j,i)위치의 bmp 가져오기
                    for (int n = 0; n < 3; n++) // 각 픽셀의 R, G, B 모두 처리하기 위해 3번 실행
                    {
                        switch (colorUnitIndex % 3) // RGB 중 어느 채널인지
                        {
                            case 0: // R
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // charValue를 2배 하고 픽셀의 R 채널 최하위 비트를 더함
                                } break;
                            case 1: // G
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // charValue를 2배 하고 픽셀의 G 채널 최하위 비트를 더함
                                } break;
                            case 2: // B
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // charValue를 2배 하고 픽셀의 B 채널 최하위 비트를 더함
                                } break;
                        }

                        colorUnitIndex++; // 인덱스 1 증가

                        if (colorUnitIndex % 8 == 0) // 한 문자에 대한 처리 마쳤을 경우
                        {
                            charValue = reverseBits(charValue); // charValue 값을 reverse해서 저장

                            if (charValue == 0) // charValue가 0이면
                            {
                                return extractedText; // 추출된 메시지 반환
                            }
                            char c = (char)charValue; // charValue를 char형으로 바꿔서 c에 저장

                            extractedText += c.ToString(); // 추출된 메시지에 c 문자 저장
                        }
                    }
                }
            }

            return extractedText; // 추출된 메시지 반환
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
