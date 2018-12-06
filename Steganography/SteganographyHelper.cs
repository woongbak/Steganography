using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        // 인스턴스 생성을 위한 public 설정
        public enum State
        {
            // 현재의 상태를 나타낼 상수.
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            // 현재 상태를 나타낼 인스턴스를 생성하고
            // 숨김모드로 설정.
            State state = State.Hiding;

            // 텍스트 중 어느 문자를 가리키는지 알려준다.
            int charIndex = 0;

            // 문자의 아스키값을 담는다.
            int charValue = 0;

            // R, G, B 중 어느 곳을 가리킬지 결정.
            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            // 픽셀 위에서부터 아래로 검사
            for (int i = 0; i < bmp.Height; i++)
            {
                // 픽셀 왼쪽부터 오른쪽 검사
                for (int j = 0; j < bmp.Width; j++)
                {
                    // 현재 검사중인 픽셀의 색상정보 할당.
                    Color pixel = bmp.GetPixel(j, i);

                    // LSB를 0으로 초기화.
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    // 현재 픽셀의 R, G, B 검사.
                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)
                        {
                            // zeros 가 증가된 횟수로 모든 과정이 끝났는지 검사.
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                // B 로 끝나지 않는 상태,
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    // 현재 픽셀을 현재 상태 그대로 저장한다.
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                // 텍스트가 숨겨진 이미지를 반환해준다.
                                return bmp;
                            }

                            // 모든 문자들이 숨겨졌는지 확인한다.
                            if (charIndex >= text.Length)
                            {
                                // 끝을 표시하기 위해 0으로 채우기 위한 상태로 만들어준다.
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                // 끝이 아니라면 다음 문자를 가리키게 해준다.
                                charValue = text[charIndex++];
                            }
                        }

                        // RGB 중 어느 LSB 에 숨길지 확인한다.
                        switch (pixelElementIndex % 3)
                        {
                            case 0:
                                {
                                    // 숨기는 과정 진행 중.
                                    if (state == State.Hiding)
                                    {
                                        // 숨길 문자의 LSB 를 픽셀 중 R 정보 끝에 남긴다.
                                        R += charValue % 2;

                                        // 숨길 문자를 오른쪽으로 한칸씩 이동 한다.
                                        charValue /= 2;
                                    }
                                } break;
                            case 1:
                                {
                                    // 숨기는 과정 진행 중.
                                    if (state == State.Hiding)
                                    {
                                        // 숨길 문자의 LSB 를 픽셀 중 R 정보 끝에 남긴다.
                                        G += charValue % 2;

                                        // 숨길 문자를 오른쪽으로 한칸씩 이동 한다.
                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    // 숨기는 과정 진행 중.
                                    if (state == State.Hiding)
                                    {
                                        // 숨길 문자의 LSB 를 픽셀 중 R 정보 끝에 남긴다.
                                        B += charValue % 2;

                                        // 숨길 문자를 오른쪽으로 한칸씩 이동 한다.
                                        charValue /= 2;
                                    }

                                    // 현재 픽셀 RGB 모두 값이 담기면
                                    // 현재 픽셀에 저장한다.
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        // 다음 픽셀 요소(R, G, B 중)를 기리킨다.
                        pixelElementIndex++;

                        // 텍스트를 다 넣은 상태,
                        // 0을 넣고 있는 상태라면
                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            // 설정이 완료된 이미지 파일을 반환.
            return bmp;
        }

        // 숨긴 텍스트 추출하기
        public static string extractText(Bitmap bmp)
        {
            // 픽셀을 가리킬 인덱스.
            int colorUnitIndex = 0;

            // 추출될 문자의 값을 담을 변수.
            int charValue = 0;

            // 추출될 문자열을 담을 변수.
            string extractedText = String.Empty;

            // 픽셀을 위에서부터 아래로 검사.
            for (int i = 0; i < bmp.Height; i++)
            {
                // 픽셀을 왼쪽에서부터 오른쪽으로 검사.
                for (int j = 0; j < bmp.Width; j++)
                {
                    // 현재 픽셀의 색상 정보를 받아온다.
                    Color pixel = bmp.GetPixel(j, i);

                    // R, G, B 각각 접근.
                    for (int n = 0; n < 3; n++)
                    {
                        // 어떤 픽셀 요소인지 검사.
                        switch (colorUnitIndex % 3)
                        {
                            // R인 경우.
                            case 0:
                                {
                                    // 현재 변수의 값을 왼쪽으로 이동하고
                                    // LSB 에 현재 요소의 LSB 를 담는다.
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            // G인 경우
                            case 1:
                                {
                                    // 현재 변수의 값을 왼쪽으로 이동하고
                                    // LSB 에 현재 요소의 LSB 를 담는다.
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            // B인 경우.
                            case 2:
                                {
                                    // 현재 변수의 값을 왼쪽으로 이동하고
                                    // LSB 에 현재 요소의 LSB 를 담는다.
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        // 다음 픽셀을 기리킨다.
                        colorUnitIndex++;

                        // 8비트 = 1바이트 를 모두 검사 했다면.
                        if (colorUnitIndex % 8 == 0)
                        {
                            // 거꾸로 된 문자의 값을 다시 돌려 변수에 담는다.
                            charValue = reverseBits(charValue);

                            // 텍스트를 숨길때 마킹된 종료 조건
                            // 0이라면
                            if (charValue == 0)
                            {
                                // 추출된 문자열을 반환한다.
                                return extractedText;
                            }
                            // 0이 아니라면
                            // 담긴 아스키 값을 문자로 변환해준다.
                            char c = (char)charValue;

                            // 추출할 문자열 변수에 추가해준다.
                            extractedText += c.ToString();
                        }
                    }
                }
            }

            // 추출된 문자열을 반환.
            return extractedText;
        }

        // 거꾸로 담긴 문자를 다시 되돌리기 위한 함수.
        public static int reverseBits(int n)
        {
            // 결과를 담을 변수.
            int result = 0;

            // 1바이트 = 8비트 를 검사.
            for (int i = 0; i < 8; i++)
            {
                // 문자를 왼쪽으로 밀며 LSB를 앞에서부터 담는다.
                result = result * 2 + n % 2;

                n /= 2;
            }

            // 담긴 문자 아스키값을 반환한다.
            return result;
        }
    }
}
