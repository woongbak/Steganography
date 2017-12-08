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

        public static Bitmap embedText(string text, Bitmap bmp) //데이터를 숨기기 위한 메소드
        {
            State state = State.Hiding;
            
            // 숨겨진 문자의 인덱스를 가지고 있는 변수
            int charIndex = 0;
            
            // 정수로 변환된 문자 값을 가지고 있는 변수 
            int charValue = 0;
            
            // 현재 처리중인 색상(R or G or B)의 인덱스를 가지는 변수
            long pixelElementIndex = 0;

            // 처리가 끝났을 때 더해진 0의 개수
            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);

                    // 각 픽셀의 LSB를 먼저 정리하는 단계
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)
                    {
                        // 새로운 8비트가 처리되었는지 확인
                        if (pixelElementIndex % 8 == 0)
                        {
                            //모든 처리 과정이 끝났는지 확인하기
                            //8개의 0이 모두 더해졌으면 처리 과정이 끝났음
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                //이미지의 마지막 픽셀 적용
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                //텍스트가 숨겨진 비트맵을 return
                                return bmp;
                            }

                            // 모든 문자가 숨겨졌는지 확인
                            if (charIndex >= text.Length)
                            {
                                // 텍스트가 끝났음을 알리기 위해 0을 써줌
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                //텍스트가 아직 안 끝났으므로 다음 캐릭터로 가서 처리 과정 진행
                                charValue = text[charIndex++];
                            }
                        }
                        
                        // 어떤 픽셀 요소가 LSB의 비트를 숨길 차례인지 확인
                        switch (pixelElementIndex % 3)
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        // 문자의 가장 오른쪽 비트는 charValue %2
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
                            // 0의 개수가 8이 될 때까지 zeros 변수 증가 
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp) //숨겨진 데이터를 추출하기 위한 메소드
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            //이미지로부터 추출된 텍스트를 가진 변수
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
                                    //픽셀 요소에서 LSB를 얻음 (pixel.R % 2)
                                    //현재 문자의 오른쪽에 1비트를 더함 (charValue=charValue*2)
                                    //추가된 비트를 (기본값 0) 픽셀 요소의 LSB와 더하기로 바꿈


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
                        
                        //8비트가 추가된 경우 결과 텍스트에 현재 문자를 추가
                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue);

                            
                            if (charValue == 0)
                            {
                                return extractedText;
                            }

                            // 정수를 문자로 바꿈
                            char c = (char)charValue;

                            // 결과 값에 현재 문자를 더함
                            extractedText += c.ToString();
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
