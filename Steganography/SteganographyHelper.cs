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

        public static Bitmap embedText(string text, Bitmap bmp)
        { // 이미지에 메시지를 숨기는 함수. 숨길 내용을 text로, 이미지 파일을 bmp로 받음
            State state = State.Hiding;
            // state 변수를 만들고 Hiding(0) 값을 넣음
            int charIndex = 0;
            // 숨기는 메시지의 Index 값
            int charValue = 0;
            // 숨기는 메시지의 값
            long pixelElementIndex = 0;
            // 이미지 픽셀의 Index 값
            int zeros = 0;
            // 메시지를 숨길 때 사용하는 픽셀 값 저장
            int R = 0, G = 0, B = 0;
            // 색 데이터를 담을 변수를 선언
            for (int i = 0; i < bmp.Height; i++)
            { // 세로부분을 훑는 루프
                for (int j = 0; j < bmp.Width; j++)
                { // 가로부분을 훑는 두번째 루프
                    Color pixel = bmp.GetPixel(j, i);
                    //이미지 파일의 (i,j)위치에 해당하는 픽셀 정보를 가져온다.
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    // R, G, B에 해당하는 값들을 모두 짝수로 만들어 준다.
                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)
                        { // pixelElementIndexrk 0이거나 8의 배수일 때
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            { // 만약 state가 Filling_With_Zeros(1)이고, zeros가 8이면
                                if ((pixelElementIndex - 1) % 3 < 2)
                                { // pixelElementIndex - 1을 3으로 나눈 나머지가 2가 아닌 경우
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                    // (i,j)에 해당하는 픽셀을 해당 RGB값으로 설정
                                }

                                return bmp; // 이미지를 반환
                            }

                            if (charIndex >= text.Length)
                            { // 숨기려는 메시지의 인덱스 값보다 입력받은 텍스트의 길이가 크거나 같으면
                                state = State.Filling_With_Zeros;
                                // state를 Filling_With_Zeros(1)로 바꿔준다.
                            }
                            else
                            {
                                charValue = text[charIndex++];
                                // 아닌 경우 인덱스에 1을 더한 위치의 text 값을 charValue에 넣어준다.
                            }
                        }

                        switch (pixelElementIndex % 3)
                        { // 이미지의 픽셀의 Index를 3으로 나눈 나머지를 이용해 switch문을 돌리는데
                            case 0:
                                { // 0인 경우
                                    if (state == State.Hiding)
                                    { // State가 Hiding(0)인 상태이면
                                        R += charValue % 2; // R에 charValue를 2로 나눈 나머지를 더해주고
                                        charValue /= 2; // charValue는 자신을 2로 나눈 몫을 넣어준다.
                                    }
                                } break;
                            case 1:
                                { // 1인 경우
                                    if (state == State.Hiding)
                                    { // State가 Hiding 상태이면
                                        G += charValue % 2;
                                        // G에 charValue를 2로 나눈 나머지를 더해주고
                                        charValue /= 2; // charValue는 자신을 2로 나눈 몫을 넣어준다.
                                    }
                                } break;
                            case 2:
                                { // 2인 경우
                                    if (state == State.Hiding)
                                    { // State가 Hiding 상태이면
                                        B += charValue % 2;
                                        // B에 charValue를 2로 나눈 나머지를 더해주고
                                        charValue /= 2; // charValue는 자신을 2로 나눈 몫을 넣어준다.
                                    }                        
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                    // 픽셀 값을 해당하는 RGB값으로 설정
                                } break;
                        }

                        pixelElementIndex++;
                        // 픽셀 Index 값 증가
                        if (state == State.Filling_With_Zeros)
                        { // State가 Filling_With_Zeros(1)이라면
                            zeros++; // zeros 값 증가
                        }
                    }
                }
            }

            return bmp; // 이미지 반환
        }

        public static string extractText(Bitmap bmp)
        { // 숨긴 메시지를 추출하는 함수. 이미지 파일 bmp를 인자로 받는다.
            int colorUnitIndex = 0; // 
            int charValue = 0; // 메시지 값을 저장하는 변수

            string extractedText = String.Empty; // 메시지를 저장할 String 변수

            for (int i = 0; i < bmp.Height; i++)
            { // 이미지의 세로부분을 훑는 루프
                for (int j = 0; j < bmp.Width; j++)
                { // 가로부분을 훑는 루프
                    Color pixel = bmp.GetPixel(j, i); // 해당 위치의 이미지 픽셀값을 가져옴
                    for (int n = 0; n < 3; n++)
                    { 
                        switch (colorUnitIndex % 3)
                        { // colorUnitIndex를 3으로 나눈 나머지가 
                            case 0: // 0인 경우
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // charValue에 2를 곱해주고 R을 2로 나눈 값을 더해줌
                                } break;
                            case 1: // 1인 경우
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // charValue에 2를 곱해주고 G를 2로 나눈 값을 더해줌
                                } break;
                            case 2: // 2인 경우
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // charValue에 2를 곱해주고 B를 2로 나눈 값을 더해줌
                                } break;
                        }

                        colorUnitIndex++;
                        // colorUnitIndex 값 증가
                        if (colorUnitIndex % 8 == 0)
                        { // colorUnitIndex를 8으로 나눈 나머지가 0이면
                            charValue = reverseBits(charValue);
                            // charValue에 비트를 뒤집은 값을 넣어준다.
                            if (charValue == 0)
                            { // charValue가 0이면
                                return extractedText; // 추출된 메시지를 반환하고
                            }
                            char c = (char)charValue; // c에 charValue값을 char타입으로 캐스팅해서 넣어준다.

                            extractedText += c.ToString(); // 추출된 메시지 변수에 c를 스트링으로 변환한 값을 넣어준다.
                        }
                    }
                }
            }

            return extractedText; // 추출된 메시지를 반환한다.
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
