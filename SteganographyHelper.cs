using System;
using System.Drawing;

namespace Steganography // Definition using namespace
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        }; // Check State

        public static Bitmap embedText(string text, Bitmap bmp)
        { // text와 bmp를 인자로 받는 함수
            State state = State.Hiding; // state hiding으로 셋팅

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0; // 값들 다 0으로 셋팅

            for (int i = 0; i < bmp.Height; i++)
            { //행
                for (int j = 0; j < bmp.Width; j++)
                {//열 검사
                    Color pixel = bmp.GetPixel(j, i);
                    // pixel값 셋팅
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
                    //RGB의 LSB 0으로 셋팅
                    for (int n = 0; n < 3; n++)
                    { // RGB 0,1,2 
                        if (pixelElementIndex % 8 == 0)
                        { // 0 or 8 일때,
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {// 끝 나서 0으로 셋팅했는지 확인
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {// 변경 해야하면
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                    //RGB셋팅
                                }

                                return bmp; //bmp 리턴
                            }

                            if (charIndex >= text.Length)
                            {//문자열 저장 했다면
                                state = State.Filling_With_Zeros; // 상태 저장
                            }
                            else
                            {//아니면
                                charValue = text[charIndex++]; //문자 호출후 Index값 ++
                            }
                        }

                        switch (pixelElementIndex % 3)
                        { // switch 함수 이용 case 0,1,2 로 나눠줌
                            case 0: //0일때 (RGB 중 R)
                                {
                                    if (state == State.Hiding)
                                    {//hiding 이면
                                        R += charValue % 2; //R값에 문자 저장
                                        charValue /= 2; //저장해 준 값 삭제
                                    }
                                } break; //탈출
                            case 1: //1일때 (RGB 중 G)
                                {
                                    if (state == State.Hiding)
                                    { // hiding 이면
                                        G += charValue % 2; //G값에 문자 저장

                                        charValue /= 2; //저장해 준 값 삭제
                                    }
                                } break;
                            case 2: //2일때 (RGB 중 B)
                                {
                                    if (state == State.Hiding)
                                    { 
                                        B += charValue % 2; //B값에 문자 저장

                                        charValue /= 2; //저장해 준 값 삭제
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // RGB셋팅
                                } break;
                        }

                        pixelElementIndex++; // 1증가 해줌

                        if (state == State.Filling_With_Zeros)
                        { // Filling_With_Zero 이면
                            zeros++; //zeros++
                        }
                    }
                }
            }

            return bmp; // bmp 리턴
        }

        public static string extractText(Bitmap bmp)
        { // bmp 인자로 받고 암호화된 문자열 추출
            int colorUnitIndex = 0;
            int charValue = 0;
           
            string extractedText = String.Empty;
            //암호화된 문자열을 저장해줄 변수
            for (int i = 0; i < bmp.Height; i++)
            {// 픽셀의 행
                for (int j = 0; j < bmp.Width; j++)
                {//픽셀의 열
                    Color pixel = bmp.GetPixel(j, i); //픽셀값 셋팅
                    for (int n = 0; n < 3; n++)
                    {//RGB(0,1,2) 체크
                        switch (colorUnitIndex % 3)
                        { // switch 함수 이용하여 0,1,2 체크
                            case 0: // 0일때 (R,G,B)중 R
                                {
                                    charValue = charValue * 2 + pixel.R % 2; //숨긴 문자 넣어줌
                                } break;
                            case 1: // 1일때 (R,G,B)중 G
                                {
                                    charValue = charValue * 2 + pixel.G % 2; //숨긴 문자 넣어줌
                                } break;
                            case 2: // 2일때 (R,G,B) 중 B
                                {
                                    charValue = charValue * 2 + pixel.B % 2; //숨긴 문자 넣어줌
                                } break;
                        }

                        colorUnitIndex++; // colorUnitIndex++ 다음 값으로

                        if (colorUnitIndex % 8 == 0)
                        {// colorUnitIndex가 0(처음)or8(마지막) 일때
                            charValue = reverseBits(charValue); //정상값으로 바꿔줌

                            if (charValue == 0)
                            {// 0이면(다 교체 됐으면)
                                return extractedText; // 숨겨진 문자열 리턴
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString(); //문자열 삽입
                        }
                    }
                }
            }

            return extractedText; //추출된 문자열 리턴해줌
        }

        public static int reverseBits(int n)
        {//비트를 반대로 정렬
            int result = 0; // 결과 저장 변수

            for (int i = 0; i < 8; i++)
            {//0~8비트 for문으로 확인
                result = result * 2 + n % 2; //result 1쉬프트한 후 n값 맨 왼쪽에 삽입

                n /= 2; //쉬프트해준 값 다시 돌리기 
            }

            return result; // 결과 리턴
        }
    }
}
