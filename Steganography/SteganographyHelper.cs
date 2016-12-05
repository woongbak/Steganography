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
        {
            State state = State.Hiding; // Hiding상태로 세팅.

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지의 가로x세로 크기만큼 반복
                {

                    Color pixel = bmp.GetPixel(j, i); // 이미지의 각각 픽셀을 pixel에 저장

                    R = pixel.R - pixel.R % 2; // 기존의 픽셀 값에서 1 or 0을 뺌
                    G = pixel.G - pixel.G % 2; // 맨 하위값인 LSB를 세팅하기 위함임.
                    B = pixel.B - pixel.B % 2; // LSB가 0일 경우 0을 빼고, 1일경우 1을 빼서 0으로 세팅.

                    for (int n = 0; n < 3; n++) //RGB총 3개므로 3번 반복.
                    {
                        if (pixelElementIndex % 8 == 0)
                        { // 문자 하나당 8bits 이므로 한 문자를 처리했을 경우 조건 만족.
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {//zero==8이므로 문자의 마지막임을 뜻함.(마지막은 8bits를 0으로 세팅하기 때문)
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {// 문자의 bits가 항상 B로 끝나는건 아니라서 G랑R이 마지막문자 bit의 끝일때.
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//그림 한 점찍기
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length) // 총 텍스트길이에 도달하면
                            {
                                state = State.Filling_With_Zeros; // Filling_With_Zeros로 상태바뀜.
                            }
                            else
                            {
                                charValue = text[charIndex++]; 
                                        // charValue에 텍스트 값을 넣음, charindex로 총 텍스트길이 확인.
                            }
                        }

                        switch (pixelElementIndex % 3)
                        { 
                            case 0: //R
                                {
                                    if (state == State.Hiding) // Hiding상태는 텍스트 숨김처리가 안된 경우
                                    {
                                        R += charValue % 2; // 0으로 세팅된 R의 LSB값에 1bit를 추가함. 
                                        charValue /= 2; // 문자의 1bit를 처리했기 때문에 1bit를 밀어줌. 
                                    }
                                } break;
                            case 1: //G
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2; // 위와 같음.

                                        charValue /= 2;
                                    }
                                } break;
                            case 2: //B
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                             // 한 RGB가 다 처리되었으므로 점을 찍어줌(그림)
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++; //숨긴 데이터가 끝났다는 위치를 알려주기 위해 8bits를 0으로 세팅하기위해 사용.
                              }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지의 가로x세로 크기만큼 반복.
                {
                    Color pixel = bmp.GetPixel(j, i);// 이미지 각각 픽셀을 가져와서 저장.
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break; // R의 LSB값을 추출해서 저장.
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break; // G의 LSB값을 추출해서 저장.
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break; // B의 LSB값을 추출해서 저장.
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0) // 8bits를 모두 추출했을 경우.
                        {
                            charValue = reverseBits(charValue); // 값을 뒤집어서 저장.

                            if (charValue == 0) // 마지막을 의미하는 8bits를 읽었을때 (0으로 가득찬)
                            {
                                return extractedText; // 추출한 텍스트 출력
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString(); //c에 저장한 문자열을 저장
                        }
                    }
                }
            }

            return extractedText; // 총 저장한 내용을 출력 = 추출한 텍스트 출력.
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
