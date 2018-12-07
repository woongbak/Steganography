using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State // State라는 이름으로 열거형 타입을 선언
        {
            Hiding,
            Filling_With_Zeros
        };
        // 열거형 타입의 멤버들을 선언한다.
        // 열거형 타입의 멤버들은 문자로 표현되었지만, 실제로는 정수값이다.
        // 각 멤버는 차례로 0으로 시작해서 1씩 증가하는 값을 가진다.
        // 즉, Hiding = 0, Filling_With_Zeros = 1 와 같다.


        public static Bitmap embedText(string text, Bitmap bmp) // 숨기기
        // param bmp: steganography.cs 에서 선언됨.
        // 디지털 이미지파일을 저장하는데 쓰이는 메모리 저장 방식
        {
            State state = State.Hiding; // State 열거형 변수 선언. 초기 0의 값을 가지고 있음

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                // 이미지파일의 모든 픽셀을 지나며 수행하는 for문
                {
                    Color pixel = bmp.GetPixel(j, i);
                    // 각 픽셀의 RGB 값을 가져옴
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    // 각 RGB값에서 자신의 최하위 비트를 빼줌.  그러면 변수 R, G, B의 최하위비트는 0으로 초기화 됨  
                    for (int n = 0; n < 3; n++)
                    // swich case 문을 통해 각 픽셀당 RGB값을 모두 수정하고 다음 픽셀로 넘어감.
                    // 그러므로 이 for문은 3 번 반복함.
                    {
                        if (pixelElementIndex % 8 == 0)
                        // 픽셀이 다음 픽셀로 넘어가는 것과 상관없이,
                        // 한 문자당 8개의 비트를 남기고 다음 문자로 넘어감.
                        // 하나의 문자는 8-bit의 2진법으로 표현되어 저장됨.
                        // pixelElementIndex 변수를 통해서 한 문자당 8-bit의 정보를 저장하는 것을 관리함
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            // state == State.Filling_With_Zeros를 만족하려면,
                            // charValue가 text의 마지막 문자를 가리키고 있어야 함.
                            // zeors == 8이면 마지막 문자를 8-bit의 2진수로 모두 저장했다는 뜻.
                            // 그러므로 모든 text를 다 저장하면 이곳으로 오게 됨.
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp; // 모든 text를 다 저장하고 bmp를 리턴
                            }

                            if (charIndex >= text.Length)
                            {
                                state = State.Filling_With_Zeros;
                            }
                            // charIndex가 text의 길이와 같아질 때,
                            // state는 1의 값을 가짐
                            else
                            {
                                charValue = text[charIndex++];
                            }
                            // charIndex가 text의 길이를 넘지 않았을 때,
                            // charValue에 text[charIndex]가 저장됨. 초기 charIndex는 0
                        }

                        switch (pixelElementIndex % 3)
                        {
                            // R, G, B 돌아가며 charValue를 2로 나눈 나머지 값이 저장.
                            // charValue에는 자신을 2로 나눈 몫이 저장.
                            // charValue에 저장되어있는 문자를 2진수로 변환한 값이 차례로 들어가는 것과 같음.
                            // case 2: 에서 한 픽셀이 B까지 모두 변환이 되면 setPixel을 통해서
                            // pixel을 수정된 RGB값으로 바꿈.
                            case 0:
                                
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }            
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                        // charValue가 text의 마지막 문자를 가리키고 있을 떄,
                        // zeros를 하나씩 증가시켜 줌.
                    }
                }
            }

            return bmp; // 모든 픽셀 내에 text를 저장하고 bmp를 리턴
        }

        public static string extractText(Bitmap bmp) // 추출
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                // 이미지파일의 모든 픽셀을 지나며 수행하는 for문
                {
                    Color pixel = bmp.GetPixel(j, i);
                    // 각 픽셀의 RGB 값을 가져옴
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        // 숨기기의 반대 방식이다.
                        // pixel의 RGB 값의 최하위 비트를 이용하여 숨겨져있는 text를 구하는 방식이다.
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)
                        // 한 문자를 모두 처리 했을 때, 이곳으로 옴.
                        // 여기서 charValue에는 text의 2진수가 거꾸로 연산이 된 값을 가지고 있음.
                        {
                            charValue = reverseBits(charValue);
                            // reversBits를 이용하여 charValue에 제대로된 text의 값을 설정해 줌.

                            if (charValue == 0)
                            // 더 이상 추출할 text가 없을 때,
                            {
                                return extractedText;
                            }
                            char c = (char)charValue; // 변수 c에 charValue를 문자로 변환하여 저장함.

                            extractedText += c.ToString(); // 문자열 extractedText에 c를 이어붙임.
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)
        // Bits를 반대로 계산해 줌.
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
