using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding, // (0)
            Filling_With_Zeros // 숨김 완료 상태 (1)
        };

        public static Bitmap embedText(string text, Bitmap bmp) // text를 bmp에 숨기는 함수. (payload->text, carrier->bmp)
        {
            State state = State.Hiding; // state 를 Hiding 으로 초기화.

            int charIndex = 0; //숨긴 문자 갯수를 저장하는 변수

            int charValue = 0; //숨길 text의 값

            long pixelElementIndex = 0; // pixelElementIndex 를 0으로 초기화.

            int zeros = 0; // 문자 하나의 비트 수

            int R = 0, G = 0, B = 0; // 변수 R, G, B 를 0으로 초기화

            for (int i = 0; i < bmp.Height; i++) // 높이 (세로 길이)를 1비트씩 확인
            {
                for (int j = 0; j < bmp.Width; j++)// 넓이 (가로 길이)를 1비트씩 확인
                {// 따라서 두 for문에 의해 이미지 파일의 모든 픽셀을 돌게 된다.
                    Color pixel = bmp.GetPixel(j, i); 
                    // 가로, 세로 위치를 인자로 받아, 각 픽셀의 RGB 값을 가져온다.
                   
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    // 각 RGB 값에서 자신의 LSB(최하위비트)를 빼준다.
                    // 따라서 R, G, B 의 최하위비트는 0이 된다.
                    for (int n = 0; n < 3; n++) // 각 픽셀당 R G B 값을 수정하므로, for문을 세 번 반복한다.
                    {
                        if (pixelElementIndex % 8 == 0) // pixelElementIndex가 0 또는 8의배수인 경우
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8) //상태가 숨김완료 상태이고, zeros 변수가 8일때.
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // (pixelElementIndex-1)을 3으로 나눈 나머지가 0 또는 1일때
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 세 if 문의 조건을 만족하면, 픽셀의 RGB 값을 설정한다.
                                }

                                return bmp; //bmp 값 반환
                            }
                            
                            if (charIndex >= text.Length) //charIndex, 즉 숨긴 문자의 개수가 text의 길이와 같아질 때,
                            {
                                state = State.Filling_With_Zeros; //변수 state 에 1 저장.
                            }
                            else//charIndex, 즉 숨긴 문자의 개수가 text의 길이보다 작을 때,
                            {
                                charValue = text[charIndex++]; //charValue에 charIndex 번째의 text 가 저장된다.
                            }
                        }

                        switch (pixelElementIndex % 3) // pixelElementIndex를 3으로 나눈 나머지에 따라 switch 문 실행.
                        {
                            case 0: // 0일때
                                {
                                    if (state == State.Hiding) //state가 Hiding 상태이면
                                    {
                                        R += charValue % 2; // R 에 해당하는 값에, charValue를 2로 나눈 나머지를 더한다.
                                        charValue /= 2;
                                    }
                                } break;
                            case 1: // 1일때
                                {
                                    if (state == State.Hiding) //state가 Hiding 상태이면
                                    {
                                        G += charValue % 2; // G 에 해당하는 값에, charValue를 2로 나눈 나머지를 더한다.

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: // 2일때
                                {
                                    if (state == State.Hiding) //state가 Hiding 상태이면
                                    {
                                        B += charValue % 2; // B 에 해당하는 값에, charValue를 2로 나눈 나머지를 더한다.

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //변경된 R,G,B 값으로 픽셀을 설정한다.
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros) // 만약 state가 Fillin_With_Zeros 라면
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp) // 이미지를 파라미터로 받아, 숨겨진 텍스트를 추출하는 함수.
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty; // 추출된 텍스트를 저장하는 string형 변수를 빈공간으로 초기화한다.

            for (int i = 0; i < bmp.Height; i++) // 높이 (세로 길이)를 1비트씩 확인
            {
                for (int j = 0; j < bmp.Width; j++) // 너비 (가로 길이)를 1비트씩 확인
                {
                    Color pixel = bmp.GetPixel(j, i); // 가로, 세로 위치를 인자로 받아, 각 픽셀의 RGB 값을 가져온다.
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3) // colorUnitIndex를 3으로 나눈 나머지값 에 따라 switch문을 실행한다.
                        {
                            case 0: // 0인 경우
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1: // 1인 경우
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2: // 2인 경우
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++; // colorUnitIndex 값을 증가시킨다.

                        if (colorUnitIndex % 8 == 0) // colorUnitIndex를 8로 나눈 나머지가 0이면
                        {
                            charValue = reverseBits(charValue);

                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;  //charValue를 char형으로 변환 해서 문자 하나를 얻어낸다.

                            extractedText += c.ToString(); // 그 문자를 extractedText에 추가한다.
                        }
                    }
                }
            }

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
