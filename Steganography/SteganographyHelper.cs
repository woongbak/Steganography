using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding,                 // Hiding(숨김 작업)
            Filling_With_Zeros      // Filling_With_Zeros(마무리 작업: End of Hidden Messege를 '\0'으로 표시)
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;     // (start) -> Hiding -> Filling_With_Zeros -> (end)

            int charIndex = 0;              // 현재 작업중인 문자의 번호

            int charValue = 0;              // 현재 작업중인 문자의 값

            long pixelElementIndex = 0;     // 각 Color Unit 순회 번호 ( 최소: 0, 최대: min( bmp.Height*bmp.Width*3 - 1, text.Length*8 ) )

            int zeros = 0;                  // 마무리 작업 count

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);   // 한 pixel의 RGB 정보

                    R = pixel.R - pixel.R % 2;  // 각 RGB 값의 마지막 1bit를 삭제 (즉, 0으로 set)    
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) // 1 pixel당 switch문에서 R, G, B 한번 씩 해당되도록
                    {
                        if (pixelElementIndex % 8 == 0)     // 다음 문자 시작할 차례일 때
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)    // 마무리 작업이 완료되면
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)    // 마지막 pixel이 아직 적용되기 전이라면
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    // 마지막으로 적용
                                }

                                return bmp;     // 숨김 종료
                            }

                            if (charIndex >= text.Length)   // 숨김 작업이 완료되면
                            {
                                state = State.Filling_With_Zeros;   // 마무리 작업 시작
                            }
                            else                            // 숨김 작업이 아직 완료되지 않았으면
                            {
                                charValue = text[charIndex++];  // 다음 문자 선택
                            }
                        }

                        switch (pixelElementIndex % 3)  // RGB에 각각 1bit씩 숨김
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2; // R에 최하위 1bit 삽입
                                        charValue /= 2;     // 문자값 shift right 1
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; // G에 최하위 1bit 삽입

                                        charValue /= 2;     // 문자값 shift right 1
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2; // B에 최하위 1bit 삽입

                                        charValue /= 2;     // 문자값 shift right 1
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));    // pixel값 적용
                                } break;
                        }

                        pixelElementIndex++;    // 다음 색 (R or G or B)

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp; // 숨김 종료
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;     // 각 Color Unit 순회 번호 (최소: 0, 최대: bmp.Height*bmp.Width*3 - 1) 
            int charValue = 0;          // 현재 작업중인 문자의 값

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;    // shift left 1 그리고 최하위 1 bit 추출
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;    // shift left 1 그리고 최하위 1 bit 추출
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;    // shift left 1 그리고 최하위 1 bit 추출
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)    // 이번 문자 추출이 끝났을 때 (다음 문자 시작할 차례일 때)
                        {
                            charValue = reverseBits(charValue);     // bit값 순서를 거꾸로 모았으므로 원래 순서대로

                            if (charValue == 0)     // End of Hidden messege(즉, '\0')가 추출된 경우 (더 이상 추출된 문자가 없는 경우)
                            {
                                return extractedText;   // 추출 종료
                            }
                            char c = (char)charValue;   // 문자값 타입 변경 (int -> char)

                            extractedText += c.ToString();  // 전체 문자열에 Append
                        }
                    }
                }
            }

            return extractedText;   // 추출 종료
        }

        public static int reverseBits(int n)    // bit값 거꾸로
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
