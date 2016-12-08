using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {   
        public enum State
        {
            Hiding, // 메시지 숨기는중의 상태
            Filling_With_Zeros // 0으로 패딩할떄의 상태
        };
        /// <summary>
        /// 이미지에 텍스트를 숨기는 기능을 합니다.
        /// </summary>
        /// <param name="text">숨길 텍스트</param>
        /// <param name="bmp">텍스트가 숨겨질 사진</param>
        /// <returns1>이미지 파일</returns>
        public static Bitmap embedText(string text, Bitmap bmp)
        {   // 이미지에서 숨기기 위한 상태값 초기화, 문자열 인덱스, 문자열값, 픽셀인덱스, zero카운트, RGB값을 초기화합니다.
            State state = State.Hiding; 

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;
            
            for (int i = 0; i < bmp.Height; i++) // 이미지 세로 루프
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지 가로 루프
                {
                    Color pixel = bmp.GetPixel(j, i); // 이미지의 가로, 세로의 픽셀값을 가져옵니다.

                    R = pixel.R - pixel.R % 2; // 가져온 픽셀값에서 LSB를 빼서 0으로 만듭니다.
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++) // RGB의 LSB에 삽입을 위해 3번 루프 돌면서
                    {
                        if (pixelElementIndex % 8 == 0) //8비트(1글자)를 모두 순회했는지 검사 
                        {

                            if (state == State.Filling_With_Zeros && zeros == 8) // 메시지 숨기고 0패딩까지 했는데
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // 남는 픽셀값들이 있다면
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));  // 변경된 픽셀값을 적용함(switch에서 r/g에서 온경우)
                                }

                                return bmp; // embed완료 사진반환
                            }

                            if (charIndex >= text.Length) // 문자열을 다 순회한경우
                            {
                                state = State.Filling_With_Zeros; // state를 Filling_With_Zeros로 set
                            }h
                            else // 그렇지 않으면
                            {
                                charValue = text[charIndex++]; // charValue는 다음 숨길 문자값을 가리킴
                            }
                        }

                        switch (pixelElementIndex % 3) // RGB를 순차적으로 선택해서
                        {
                            case 0: // R일때
                                {
                                    if (state == State.Hiding) // 숨기는 중이라면
                                    {
                                        R += charValue % 2; // LSB에 메시지 삽입
                                        charValue /= 2; // 삽입할 다음 문자열 가리킴
                                    }
                                } break;
                            case 1: // G일때  
                                {
                                    if (state == State.Hiding) // 숨기는 중이라면
                                    {
                                        G += charValue % 2; // LSB에 메시지 삽입
                                        charValue /= 2; // 삽입할 다음 문자열 가리킴
                                    }
                                } break;
                            case 2: // B일때
                                {
                                    if (state == State.Hiding) // 숨기는 중이라면
                                    {
                                        B += charValue % 2; // LSB에 메시지 삽입
                                        charValue /= 2; // 삽입할 다음 문자열 가리킴
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 변경된 픽셀값을 적용함
                                } break;
                        }

                        pixelElementIndex++; // 다음픽셀 가리키도록 +1

                        if (state == State.Filling_With_Zeros) // 0패딩중일때는 zeros+1함
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp; // 이미지 반환
        }
        /// <summary>
        /// 이미지에서 텍스트를 추출하는 기능을 합니다.
        /// </summary>
        /// <param name="bmp">bmp는 가져올 이미지 입니다.</param>
        /// <returns>텍스트값</returns>
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++) // 이미지 세로 루프
            {
                for (int j = 0; j < bmp.Width; j++) // 이미지 가로 루프
                {
                    Color pixel = bmp.GetPixel(j, i); //  j,i 좌표의 픽셀 값 가져옴.
                    for (int n = 0; n < 3; n++) // 루프 3번 돌면서
                    {
                        switch (colorUnitIndex % 3) // 픽셀의 RGB를 대응함
                        {
                            case 0: 
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // LSB를 얻고 다음 값을 가리킴(charValue*2)
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // LSB를 얻고 다음 값을 가리킴(charValue*2)
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // LSB를 얻고 다음 값을 가리킴(charValue*2)
                                } break;
                        }

                        colorUnitIndex++; //인덱스+1

                        if (colorUnitIndex % 8 == 0) //인덱스가 8까지 다 돌면(==아스키값 하나 얻어오면)
                        {
                            charValue = reverseBits(charValue); // 문자열을 reverse함.(그래야 제대로된 아스키값이 됨)

                            if (charValue == 0) // NULL이면(0패딩)
                            {
                                return extractedText; // 추출한 메시지 반환
                            }
                            char c = (char)charValue; // c에 문자값을 넣고

                            extractedText += c.ToString(); // 반환할 메시지 값에 이어붙임.
                        }
                    }
                }
            }

            return extractedText; // 추출한 메시지 반환
        }
        /// <summary>
        /// 비트(bit)값을 reverse 해주는 기능을 합니다.
        /// </summary>
        /// <param name="n">비트나열</param>
        /// <returns>reverse된 bit값(==인식가능한 아스키값)</returns>
        public static int reverseBits(int n)
        {
            int result = 0;

            for (int i = 0; i < 8; i++) // 8bit 돌면서
            {
                result = result * 2 + n % 2; // n의 마지막자리부터 result에 순서대로(즉 거꾸로) 나열함 

                n /= 2; // n을 다음 자릿수로
            }

            return result; // reverse한 비트나열 반환
        }
    }
}
