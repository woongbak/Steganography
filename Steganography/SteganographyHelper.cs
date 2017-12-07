using System;
using System.Drawing;

//네임 스페이스를 통한 정의
namespace Steganography
{
    class SteganographyHelper
    {
        //상태 확인해줌 (hiding, filling_with_zeros)
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        };

        //변수로 text와 bmp를 받아 text를 숨기는 메소드
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            //상태 hiding으로 설정
            State state = State.Hiding;
            //hiding하는 문자 갯수 카운트
            int charIndex = 0;
            //hiding하는 문자 저장
            int charValue = 0;
            //RGB 값 저장
            long pixelElementIndex = 0;
            //문자 한 개당의 비트 수
            int zeros = 0;
            //RGB 초기화
            int R = 0, G = 0, B = 0;
            //세로 스캔
            for (int i = 0; i < bmp.Height; i++)
            {   //가로 스캔
                for (int j = 0; j < bmp.Width; j++)
                {   //색 추출
                    Color pixel = bmp.GetPixel(j, i);
                    //RGB에 해당하는 LSB를 0으로 초기화 후 저장
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)
                    {   //0또는 8일 경우에
                        if (pixelElementIndex % 8 == 0)
                        {   //Filling_with_Zeros state 및 8비트 확인한 뒤
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {   // 변경 필요 RGB값 존재하면
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {   // RGB값을 설정
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
                                //bmp 리턴
                                return bmp;
                            }
                            // 문자열 다 저장했는지 확인
                            if (charIndex >= text.Length)
                            {   // 상태 저장
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {   //문자 가져온 뒤 인덱스++ 
                                charValue = text[charIndex++];
                            }
                        }
                        //스위치로 분기
                        switch (pixelElementIndex % 3)
                        {   //결과값이 0인경우(R)
                            case 0:
                                {   //Hiding 모드이면
                                    if (state == State.Hiding)
                                    {   //R값에 문자 저장
                                        R += charValue % 2;
                                        //저장 값 삭제
                                        charValue /= 2;
                                    }
                                } break;
                            case 1: //G이면
                                {
                                    if (state == State.Hiding)
                                    {   //G에 저장
                                        G += charValue % 2;
                                        //저장 값 삭제
                                        charValue /= 2;
                                    }
                                } break;
                            case 2: //B이면
                                {
                                    if (state == State.Hiding)
                                    {   //B에 저장
                                        B += charValue % 2;
                                        //저장 값 삭제
                                        charValue /= 2;
                                    }
                                    //RGB에 값 
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }
                        //1증가
                        pixelElementIndex++;
                        //상태가 Filling_With_Zeros이면
                        if (state == State.Filling_With_Zeros)
                        {   //Zeros 증가
                            zeros++;
                        }
                    }
                }
            }
            // bmp 리턴
            return bmp;
        }
        //bmp 인자로 받고 숨긴 문자열 추출하는 메소드
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;
            //추출할 문자열 저장 변수
            string extractedText = String.Empty;
            //bmp 객체의 세로
            for (int i = 0; i < bmp.Height; i++)
            {   //bmp 객체의 가로
                for (int j = 0; j < bmp.Width; j++)
                {   //픽셀 값 추출
                    Color pixel = bmp.GetPixel(j, i);
                    // RGB 값 가져옴
                    for (int n = 0; n < 3; n++)
                    {   //스위치 분기
                        switch (colorUnitIndex % 3)
                        {   //R이면
                            case 0:
                                {
                                    //1시프트 후 숨긴 문자 삽입
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1: //G이면
                                {
                                    //1시프트 후 숨긴 문자 삽입
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2: //B이면
                                {
                                    //1시프트 후 숨긴 문자 삽입
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;
                        //colorUnitIndex가 처음 또는 마지막이면
                        if (colorUnitIndex % 8 == 0)
                        {   //정상 값으로 교체
                            charValue = reverseBits(charValue);
                            //정상으로 만들 값이 없으면
                            if (charValue == 0)
                            {   //숨겨진 문자열 리턴
                                return extractedText;
                            }
                            // 문자열을 문자열에 넣기 위해 변환
                            char c = (char)charValue;
                            // 문자열 삽입
                            extractedText += c.ToString();
                        }
                    }
                }
            }
            //추출 문자열 리턴
            return extractedText;
        }
        //n을 변수로 받는 비트를 반대로 정렬하는 메소드
        public static int reverseBits(int n)
        {   //결과 저장 변수
            int result = 0;
            //0~8비트 확인
            for (int i = 0; i < 8; i++)
            {
                //result을 << 1 한뒤 n 값의 맨 왼쪽에 삽입
                result = result * 2 + n % 2;
                //n 오른쪽으로 >> 1
                n /= 2;
            }
            //결과 리턴
            return result;
        }
    }
}
