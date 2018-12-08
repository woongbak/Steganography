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

        // hiding text in bmp file
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            // Hiding 상태로 설정
            State state = State.Hiding;

            // 숨길 문자의 값 저장 변수
            int charIndex = 0;

            // 숨길 문자를 정수형으로 저장할 변수
            int charValue = 0;

            // 픽셀을 순회하며 숨김(변환)해야 하는 해당 색을 저장할 변수
            long pixelElementIndex = 0;

            // 공백 저장 변수
            int zeros = 0;

            // R,G,B값 저장 변수
            int R = 0, G = 0, B = 0;

            // bmp file의 모든 픽셀 순회
            for (int i = 0; i < bmp.Height; i++)    // bmp file의 세로 크기만큼 위에서부터 한 줄씩 전체 순회
            {
                for (int j = 0; j < bmp.Width; j++) // bmp file의 가로 크기만큼 왼쪽에서부터 한 픽셀씩 전체 순회
                {
                    // 각 픽셀의 RGB값을 받아옴
                    Color pixel = bmp.GetPixel(j, i);

                    // 해당 픽셀의 LSB값을 0으로 설정하여 저장
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    // R,G,B값 설정을 위한 반복문
                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0) // 하나의 문자(8비트) 단위로 확인
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)    // 숨김이 완료, zeros의 값이 8 -> 초과된 상태
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)   // 모든 비트를 확인
                                {
                                    // 픽셀의 R,G,B값에 숨김
                                    // 숨김이 다 끝난 상태이면 원본 이미지 그대로일것
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                // 정상적으로 숨김 완료 후 return image
                                return bmp;
                            }

                            // 숨김 완료
                            if (charIndex >= text.Length)
                            {
                                // 상태 복구
                                state = State.Filling_With_Zeros;
                            }
                            else // 숨김 미완료
                            {
                                // 다음 숨길 텍스트 받아옴
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)  // 저장할 R,G,B 결정
                        {
                            case 0: // R
                                {
                                    if (state == State.Hiding)  // 숨김상태
                                    {
                                        R += charValue % 2; // 숨길 문자의 LSB를 더함
                                        charValue /= 2;     // 숨긴 문자 전체 비트를 오른쪽으로 한칸씩 이동
                                    }                       // 숨긴 문자의 LSB를 삭제
                                } break;
                            case 1: // G
                                {
                                    if (state == State.Hiding) // 숨김상태
                                    {
                                        G += charValue % 2; // 숨길 문자의 LSB를 더함
                                        charValue /= 2;     // 숨긴 문자 전체 비트를 오른쪽으로 한칸씩 이동
                                    }                       // 숨긴 문자의 LSB를 삭제
                                } break;
                            case 2: // B
                                {
                                    if (state == State.Hiding) // 숨김상태
                                    {
                                        B += charValue % 2; // 숨길 문자의 LSB를 더함
                                        charValue /= 2;     // 숨긴 문자 전체 비트를 오른쪽으로 한칸씩 이동
                                    }                       // 숨긴 문자의 LSB를 삭제

                                    // 변경된 값으로 설정
                                    // 실제 숨겨지는 데이터는 '/'연산에 의해 LSB값부터 저장되기 때문에
                                    // 기존 데이터의 역순으로 저장됩니다.
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        pixelElementIndex++;    // 다음 색상으로

                        if (state == State.Filling_With_Zeros)  // 문자열 전부 숨김
                        {
                            zeros++;
                        }
                    }
                }
            }

            // 이미지의 모든 픽셀을 순회 했으므로
            // 데이터가 이미지 초과 or 같은 경우
            // return image
            return bmp;
        }

        public static string extractText(Bitmap bmp)
        {
            // 픽셀을 순회하며 추출해야 하는 해당 색을 저장할 변수
            int colorUnitIndex = 0;
            // 추출할 문자의 값 저장 변수
            int charValue = 0;

            // 추출된 문자를 정리할 문자열
            string extractedText = String.Empty;

            // bmp file의 모든 픽셀 순회
            for (int i = 0; i < bmp.Height; i++)    // bmp file의 세로 크기만큼 위에서부터 한 줄씩 전체 순회
            {
                for (int j = 0; j < bmp.Width; j++) // bmp file의 가로 크기만큼 왼쪽에서부터 한 픽셀씩 전체 순회
                {
                    // 각 픽셀의 RGB값을 받아옴
                    Color pixel = bmp.GetPixel(j, i);
                    // R,G,B값 확인을 위한 반복문
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0: // R
                                {
                                    // 문자 전체 비트를 오른쪽으로 한칸씩 이동
                                    // 이미지 픽셀의 현재 색의 LSB를 더함
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1: // G
                                {
                                    // 문자 전체 비트를 오른쪽으로 한칸씩 이동
                                    // 이미지 픽셀의 현재 색의 LSB를 더함
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2: // B
                                {
                                    // 문자 전체 비트를 오른쪽으로 한칸씩 이동
                                    // 이미지 픽셀의 현재 색의 LSB를 더함
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;    // 다음 색상으로

                        if (colorUnitIndex % 8 == 0) // 하나의 문자(8비트) 단위로 확인
                        {
                            // 역순으로 저장되었던 데이터를 다시 역순으로 저장
                            charValue = reverseBits(charValue);

                            if (charValue == 0) // 데이터 추출 완료상태
                            {
                                // 정상적으로 추출한 데이터 반환
                                return extractedText;
                            }
                            // 추출 문자 char 형변환
                            char c = (char)charValue;

                            // 추출 문자를 정리할 문자열에 추가
                            extractedText += c.ToString();
                        }
                    }
                }
            }

            // 이미지의 모든 픽셀을 순회 했으므로
            // 데이터가 이미지 초과 or 같은 경우
            // 이미지에 숨긴 데이터만 반환
            return extractedText;
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
