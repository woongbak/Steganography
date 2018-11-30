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

        // 텍스트를 숨기는 함수입니다
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;  // 상태를 Hiding으로 설정합니다

            int charIndex = 0; // 문자열에서 숨길 문자의 인덱스입니다

            int charValue = 0; // 숨겨야하는 문자의 값입니다
            // 8비트로 R,G,B의 LSB에 각 1비트씩 숨깁니다

            long pixelElementIndex = 0; // R,G,B,R,G,B... 순서로 수정할 채널을 결정합니다

            int zeros = 0; // 마지막 문자가 끝나고 공백을 저장할 때 사용할 변수입니다

            int R = 0, G = 0, B = 0; // 픽셀의 R,G,B 값을 수정할 변수입니다

            for (int i = 0; i < bmp.Height; i++) // 이미지의 전체 크기만큼 반복합니다
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); // 이미지의 픽셀을 얻어옵니다

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2; // 각 R,G,B 변수에 픽셀 R,G,B의 LSB를 0으로 바꾼 값을 저장합니다

                    for (int n = 0; n < 3; n++) // R,G,B 설정을 위해 3번 반복합니다
                    {
                        if (pixelElementIndex % 8 == 0) // 한 문자의 처리를 시작할 때 입니다
                        {// pixelElementIndex % 8 == 7일때 한 문자의 처리가 끝납니다
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {// 이미 숨김이 끝난상태고 zeros가 8이라면 (숨김이 끝나고도 1문자만큼 더 지나간 경우입니다)

                                if ((pixelElementIndex - 1) % 3 < 2)
                                {   // pixelElementIndex-1의 의미는 현재까지 저장된 채널입니다
                                    // 만약 중간까지 (R, ) (R,G, )까지만 LSB가 0으로 변경되고 zeros가 8이되었다면
                                    // B에서 실행되는 픽셀의 설정이 안되므로 
                                    // 마지막 경우까지 확인해 공백문자를 저장합니다
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp; // 이미지를 리턴합니다
                            }

                            if (charIndex >= text.Length) // 숨길 문자열의 현재 인덱스가 문자열 길이 이상이라면 숨김이 끝났다는 의미입니다
                            {
                                state = State.Filling_With_Zeros; // 상태를 Filling_With_Zeros로 바꿉니다
                            }
                            else // 아직 숨길 문자가 남았다면
                            {
                                charValue = text[charIndex++]; // 숨길 문자값을 얻어옵니다
                            }
                        }

                        switch (pixelElementIndex % 3) // 현재 저장할 R,G,B채널을 결정합니다
                        {   //0 : R, 1 : G, 2 : B입니다
                            // 숨겨야하는 문자의 비트를 마지막부터 저장하기 때문에 실제 데이터의 역순으로 설정됩니다
                            case 0:
                                {
                                    if (state == State.Hiding) // 숨겨야하는 상태면
                                    {
                                        R += charValue % 2; // 숨길 문자의 마지막 비트를 더하고
                                        charValue /= 2; // 한자리를 쉬프트합니다
                                    }
                                }
                                break;
                            case 1:
                                {
                                    if (state == State.Hiding) // 숨겨야하는 상태면
                                    {
                                        G += charValue % 2; // 숨길 문자의 마지막 비트를 더하고

                                        charValue /= 2; // 한자리를 쉬프트합니다
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if (state == State.Hiding) // 숨겨야하는 상태면
                                    {
                                        B += charValue % 2; // 숨길 문자의 마지막 비트를 더하고

                                        charValue /= 2; // 한자리를 쉬프트합니다
                                    }

                                    // 한 픽셀의 설정이 끝났다면
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 픽셀의 R,G,B값을 변경합니다
                                }
                                break;
                        }

                        pixelElementIndex++; // 선택할 R,G,B 채널의 종류를 변경합니다

                        if (state == State.Filling_With_Zeros)
                        {// 문자열을 모두 숨겼다면 
                            zeros++; // 1증가 시킵니다
                        }
                    }
                }
            }

            // 정상적으로 문자열을 모두 숨겼다면 위에서 조기리턴으로 함수가 끝납니다
            // 여기서 리턴하게 되는 경우는 
            // 1. 실제로 숨겨야할 데이터와 숨길 수 있는 비트 수가 같은 경우
            // 2. 숨겨야하는 문자열의 데이터가 숨길수 있는 비트보다 큰 경우입니다
            // 2번의 경우 문자열의 일부분만 저장됩니다
            return bmp;
        }

        // 숨긴 문자열을 추출하는 함수입니다
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0; // 확인할 픽셀의 색 채널을 결정합니다
            int charValue = 0; // 문자열의 값을 저장합니다

            string extractedText = String.Empty; // 빈 문자열을 생성합니다

            for (int i = 0; i < bmp.Height; i++) // 이미지의 전체 크기만큼 반복합니다
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); // 픽셀을 얻어옵니다
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        { // 색 채널을 확인 후
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                    // 기존 데이터를 쉬프트 후 R 채널의 LSB를 더합니다
                                }
                                break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                    // 기존 데이터를 쉬프트 후 G 채널의 LSB를 더합니다
                                }
                                break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                    // 기존 데이터를 쉬프트 후 B 채널의 LSB를 더합니다
                                }
                                break;
                        }

                        colorUnitIndex++; // 색 채널을 변경합니다

                        if (colorUnitIndex % 8 == 0)
                        {// 한 문자가 끝났을 경우입니다
                            charValue = reverseBits(charValue);
                            // 역순으로 저장된 데이터를 바꾸어 정상적인 데이터로 변환합니다

                            if (charValue == 0)
                            {   // 모든 문자열이 끝났을 경우입니다
                                // 숨길 때 zeros변수를 이용해 1문자만큼의 LSB를 모두 0으로 바꾸었기에
                                // charValue에 0이 저장된 경우가 모든 문자열이 끝난 경우입니다
                                return extractedText; // 추출된 문자열을 리턴합니다
                            }
                            char c = (char)charValue; // 문자를 char형으로 변환 후

                            extractedText += c.ToString(); // 문자열에 추가합니다
                        }
                    }
                }
            }

            // 정상적으로 원본 문자열이 모두 숨겨진 경우라면 위에서 먼저 문자열이 반환됩니다
            // 이미지의 전체크기만큼 모두 확인한 경우라면
            // 리턴된 문자열이 숨긴 원본 문자열의 전부인지는 확인 불가능합니다
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
