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
        // 어디서나 사용 가능한 정적 메소드 선언, 인자는 Text 문자열과 이미지파일 //
        { // 함수의 효과는 인자로 입력받은 Text를 이미지파일속에 숨기는 것 //
            State state = State.Hiding; // 열거형 선언, State.Hiding의 값은 0 // 

            int charIndex = 0; 

            int charValue = 0; 

			long pixelElementIndex = 0; 

			int zeros = 0; 

			int R = 0, G = 0, B = 0; 

			for (int i = 0; i < bmp.Height; i++)
            { // 반복문 선언, bmp의 높이만큼 반복 //
                for (int j = 0; j < bmp.Width; j++)
                { // 반복문 선언, bmp의 넓이만큼 반복 //
                    Color pixel = bmp.GetPixel(j, i); // 내장되어있는 Color 변수 선언 그리고 Getpixel 메소드로 인자로 받은 비트맵의 색을 저장 //

                    R = pixel.R - pixel.R % 2; // R의 픽셀값의 반을 원형에서 뺌//
                    G = pixel.G - pixel.G % 2; // G의 픽셀값의 반을 원형에서 뺌//
				B = pixel.B - pixel.B % 2; // B의 픽셀값의 반을 원형에서 뺌//
				// RGB 값의 LSB를 0으로 세팅 //
				for (int n = 0; n < 3; n++)
					{ // 반복문 선언 0부터 3까지 반복, R,G,B 채널에 참여하기 위해서 //
						if (pixelElementIndex % 8 == 0) // 픽셀의 인덱스를 8로 나눈 나머지가 0이면 조건문 실행, 8bit 모두 완료 되었다는 의미 //
                        { 
                            if (state == State.Filling_With_Zeros && zeros == 8) // State 값이 Filling_with_zeros 값과 같고 zeros 값이 8이면 조건문 실행 //
                            {
                                if ((pixelElementIndex - 1) % 3 < 2) // (픽셀의 인덱스-1)을 3으로 나눈 나머지가 2보다 작으면 조건문 실행 //
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); // 비트맵의 색을 FromArgb 메소드를 지정하여 사용자 지정색을 생성 후 대입 // 
                                } // case 2:가 실행되지 않았으면 전의 픽셀값 조정 //

                                return bmp; // 이미지 비트맵 리턴 //
                            }

                            if (charIndex >= text.Length) // charIndex 변수의 정수 값이 텍스트길이보다 크거나 같으면 조건문 실행 //
                            {
                                state = State.Filling_With_Zeros; // State 값에 Filling_with_zeros의 값을 저장
                            } // 문자를 정수로 변환하는 과정은 완료 되었음을 의미 //
                            else // if문에 반하면 //
                            {
                                charValue = text[charIndex++]; // charvalue에 text배열값 삽입, 삽입 후 charindex의 값 1증가 //
                            }
                        } // 숨길 문자를 정수로 변환 //

                        switch (pixelElementIndex % 3) // switch문 : 픽셀의 인덱스를 3으로 나눈 값이 양수이면 //
                        {
                            case 0: // 나머지 값이 0 이면 //
                                {
                                    if (state == State.Hiding) // state 값에 hiding 값을 저장 // 
                                    {
                                        R += charValue % 2; // R값에 charValue 값을 2로 나눈 나머지값을 저장 //
                                        charValue /= 2; // CharValue 값을 2로 나눈값을 저장
                                    }
                                } break; // case문 종료 //
                            case 1: // 나머지 값이 1이면 //
                                {
                                    if (state == State.Hiding) // state 값에 hiding 값을 저장 //
								{
                                        G += charValue % 2; // G값에 charValue 값을 2로 나눈 나머지값을 저장 //
								charValue /= 2; // CharValue 값을 2로 나눈값을 저장 //
							}
                                } break; // case문 종료 //
                            case 2: // 나머지 값이 2이면 //
							{
                                    if (state == State.Hiding) // state 값에 hiding 값을 저장 //
								{ 
                                        B += charValue % 2; // B값에 charValue 값을 2로 나눈 나머지값을 저장 //
								charValue /= 2; // CharValue 값을 2로 나눈값을 저장 //
							}
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));  // 비트맵의 색을 FromArgb 메소드를 지정하여 사용자 지정색을 생성 후 대입 // 
						  } break; // case문 종료 //
                        } //  변환된 값을 해당되는 픽셀에 인코딩 // 

                        pixelElementIndex++; // 픽셀의 인덱스 값 1증가 //

                        if (state == State.Filling_With_Zeros) // state값이 Filling_with_zeros 값과 같으면 조건문 실행 즉 플래그가 1이면 //
                        {
                            zeros++; // zeros 값 1증가 //
                        }
                    }
                }
            }

            return bmp; // 이미지파일 리턴 //
        } // 메소드 종료 // 

        public static string extractText(Bitmap bmp) // 어디서나 사용 정적 메소드 선언, 숨겨진 텍스트를 추출하는 메소드, 인자는 이미지파일 //
        {
            int colorUnitIndex = 0;
            int charValue = 0; 

            string extractedText = String.Empty; // 추출된 텍스트를 저장하기 위해 선언, 빈 문자열으로 초기화 //

            for (int i = 0; i < bmp.Height; i++)
            { // 반복문 실행, 0부터 이미지파일의 높이 만큼 반복 //
                for (int j = 0; j < bmp.Width; j++)
                { // 반복문 실행, 0부터 이미지파일의 넓이 만큼 반복 // 
                    Color pixel = bmp.GetPixel(j, i); // 내장되어있는 Color 변수 선언 그리고 Getpixel 메소드로 인자로 받은 비트맵의 색을 저장 //
                    for (int n = 0; n < 3; n++)
                    { // 반복문 0부터 3까지 반복, R,G,B 채널에 참여하기 위해서 // 
                        switch (colorUnitIndex % 3)
                        { // Switch문 : colorunitindex를 3으로 나눈 값이 양수이면 // 
                            case 0: // 값이 0 이면 //
                                {
                                    charValue = charValue * 2 + pixel.R % 2; // charvalue에 읽어온 비트값을 더함// 
                                } break; // 케이스문 종료 //
                            case 1: // 값이 1 이면 //
                                {
                                    charValue = charValue * 2 + pixel.G % 2; // charvalue에 읽어온 비트값을 더함// 
                                } break; // 케이스문 종료 //
                            case 2: // 값이 2 이면 //
                                {
                                    charValue = charValue * 2 + pixel.B % 2; // charvalue에 읽어온 비트값을 더함// 
                                } break; // 케이스문 종료 //
                        }

                        colorUnitIndex++; // colorunitindex 값 1증가, bit를 처리할때 마다 증감 //

                        if (colorUnitIndex % 8 == 0) // colorunitindex값을 8로 나눈 나머지가 0이면 //
                        { 
                            charValue = reverseBits(charValue); // charvalue에 뒤집은 비트를 삽입 //  

                            if (charValue == 0) // charvalue가 0이면 // 
                            {
                                return extractedText; //추출된 텍스트 리턴 //
                            }
                            char c = (char)charValue; // 문자형 변수 선언 후, 정수형인 charvalue에 문자형을 대입 // 

                            extractedText += c.ToString(); // c에 있는 추출된 문자열에 저장 //
                        }
                    }
                }
            }

            return extractedText; // 추출된 문자열 리턴 //
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
