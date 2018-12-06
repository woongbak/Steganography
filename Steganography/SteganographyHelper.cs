using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding,
            //숨김 상태
            Filling_With_Zeros
            //0으로 채워진 상태
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {//text를 숨기는 메소드
            State state = State.Hiding;
            //상태를 숨김으로 변환
            int charIndex = 0;
            //숨길 문자열의 인덱스를 0으로 초기화
            int charValue = 0;
            //숨길 문자열의 문자를 0으로 초기화
            long pixelElementIndex = 0;
            //픽셀의 인덱스를 0으로 초기화
            int zeros = 0;

            int R = 0, G = 0, B = 0;
            //데이터를 숨길 때 사용할 픽셀 값들을 저장할 변수 0으로 초기화
            for (int i = 0; i < bmp.Height; i++)
            {//이미지의 세로 길이만큼 반복
                for (int j = 0; j < bmp.Width; j++)
                {//이미지의 가로 길이만큼 반복
                    Color pixel = bmp.GetPixel(j, i);
                    //이미지의 (i,j) 위치의 픽셀을 가져옴
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    //가져온 픽셀의 R,G,B의 LSB의 값을 각각 0으로 설정
                    for (int n = 0; n < 3; n++)
                    {//R,G,B의 LSB의 값을 변경하기 위해 3번 반복
                        if (pixelElementIndex % 8 == 0)
                        {//문자 하나를 다 숨겼을 경우
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {//문자열의 모든 문자를 다 숨겼을 경우
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {//B 채널까지 모두 변경 후
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }   //(i,j) 위치 픽셀에 변경한 R,G,B 값 저장

                                return bmp;
                                //text가 숨겨진 이미지 반환
                            }

                            if (charIndex >= text.Length)
                            {//문자열의 인덱스가 text의 문자열 인덱스와 같거나 크면
                                state = State.Filling_With_Zeros;
                                //0이 가득 찬 상태로 바꿔줌
                            }
                            else
                            {//문자열의 인덱스가 text의 문자열 인덱스보다 작으면
                                charValue = text[charIndex++];
                                //다음 숨길 문자의 인덱스를 저장
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {//R,G,B 중 어디에 text를 숨길지
                            case 0:
                                {//R
                                    if (state == State.Hiding)
                                    {//숨김 상태면
                                        R += charValue % 2;
                                        //R에 최하위 비트 저장
                                        charValue /= 2;
                                        //charValue에 2로 나눈 값 저장
                                    }
                                } break;
                            case 1:
                                {//G
                                    if (state == State.Hiding)
                                    {//숨김 상태면
                                        G += charValue % 2;
                                        //G에 최하위 비트 저장
                                        charValue /= 2;
                                        //charValue에 2로 나눈 값 저장
                                    }
                                } break;
                            case 2:
                                {//B
                                    if (state == State.Hiding)
                                    {//숨김 상태면
                                        B += charValue % 2;
                                        //B에 최하위 비트 저장
                                        charValue /= 2;
                                        //charValue에 2로 나눈 값 저장
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                    //R, G, B 채널의 LSB 값을 모두 변경한 후 해당 위치 픽셀에 저장
                                }
                                break;
                        }

                        pixelElementIndex++;
                        //픽셀의 인덱스를 하나 증가
                        if (state == State.Filling_With_Zeros)
                        {//0으로 채워진 상태이면
                            zeros++;
                            //zeros를 하나 증가
                        }
                    }
                }
            }

            return bmp;
            //text가 숨겨진 이미지 반환
        }

        public static string extractText(Bitmap bmp)
        {//text를 추출하기 위한 메소드
            int colorUnitIndex = 0;
            //읽어온 LSB의 수를 저장할 변수 0으로 초기화
            int charValue = 0;
            //추출한 문자를 저장할 변수를 0으로 초기화
            string extractedText = String.Empty;
            //추출한 문자를 저장할 배열
            for (int i = 0; i < bmp.Height; i++)
            {//세로 길이만큼 반복
                for (int j = 0; j < bmp.Width; j++)
                {//가로 길이만큼 반복
                    Color pixel = bmp.GetPixel(j, i);
                    //(i,j) 위치의 픽셀을 가져옴
                    for (int n = 0; n < 3; n++)
                    {//R,G,B 각각 처리를 위해 3번 반복
                        switch (colorUnitIndex % 3)
                        {//R,G,B 중 어느 채널인지
                            case 0:
                                {//R
                                    charValue = charValue * 2 + pixel.R % 2;
                                    //charValue에 2를 곱하고 R 채널의 최하위 비트와 더함
                                }
                                break;
                            case 1:
                                {//G
                                    charValue = charValue * 2 + pixel.G % 2;
                                    //charValue에 2를 곱하고 G 채널의 최하위 비트와 더함
                                }
                                break;
                            case 2:
                                {//B
                                    charValue = charValue * 2 + pixel.B % 2;
                                    //charValue에 2를 곱하고 B 채널의 최하위 비트와 더함
                                }
                                break;
                        }

                        colorUnitIndex++;
                        //colorUnitIndex 하나 증가
                        if (colorUnitIndex % 8 == 0)
                        {//문자 하나를 다 추출했을 경우
                            charValue = reverseBits(charValue);
                            //charValue의 값을 역순으로 저장
                            if (charValue == 0)
                            {//문자열을 다 추출했으면
                                return extractedText;
                                //추출한 text를 반환
                            }
                            char c = (char)charValue;
                            //읽어온 정수를 문자형으로 변환
                            extractedText += c.ToString();
                            //추출한 text에 c 저장
                        }
                    }
                }
            }

            return extractedText;
            //추출한 text 반환
        }

        public static int reverseBits(int n)
        {//역순으로 바꾸는 메소드
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
