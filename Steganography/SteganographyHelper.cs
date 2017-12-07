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

        // 숨기기 위한 함수? (메소드)
        // 인자값 :text, bmp --> 숨길 문자열 (string 형), 사진 파일 

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;

            // int charIndex set =0, string text 의 문자열을 컨트롤 하기 위한 변수 
            int charIndex = 0; 
    
            // int charValue set =0,  string text의 문자들을 정수로 표현한 변수 
            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;    // 사진의 RGB 영역의 선언 및 0 으로 치기화 

            // 가져온 이미지 파일의 높이를 i 변수를 통해 전체적으로 for문 돌림
            for (int i = 0; i < bmp.Height; i++) 
            {
                // 가져온 이미지 파일의 넓이를 j 변수를 통해 전체적으로 for 문 돌림
                for (int j = 0; j < bmp.Width; j++)
                {

                    // 가리키고 있는 이미지의 높이, 넓이 (i,j)의 해당 부분 픽셀 가져오기.
                    Color pixel = bmp.GetPixel(j, i);

                    // 읽어들인 pixel 중 RGB 영역의 LSB를 모두 0으로 세팅한다. 
                    // 2진수로 8비트이기 때문에. LSB를 0으로 바꿈.

                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    // i는 0,1,2 순서로 돌면서 총 3번 for문이 반복 된다.

                    for (int n = 0; n < 3; n++)
                    {
                        //가져온 픽셀의 요소가 0과 같거나, 8(bit)가 될 떄.
                        if (pixelElementIndex % 8 == 0)
                        {
                            // ??  이 코드는 이해하지 못함. aeros?? ==8 ?????????????
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {

                                //가져온 픽셀의 요소가의 -1 이, 3으로 나눈 나머지가 2보다 작을 떄 
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    // 해당 픽셀의 i,j 위치에 0으로 R,G,B 를 주입함. (R,G,B 는 0으로 초기화 되어 있음)
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                // 텍스트가 숨겨진 이미지를 다시 반환함.

                                return bmp;
                            }

                            // 문자열을 가르키는 index 변수가 string text 의 길이보다 크거나 같을 때.
                            if (charIndex >= text.Length)
                            {
                                // 문자를 숨김.
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                // charValue에 text를 가르키고 있는 charindex 를 1 증가하여 다음 문자를 가르키게 한 text 변수를 대입.
                                charValue = text[charIndex++];
                            }
                        }

                        // 가져온 pixel요소가 3으로 나누었을 때 0이 아닐 떄 
                        switch (pixelElementIndex % 3)
                        {
                            // 0 일 떄 R영역에 해당 됨.
                            case 0:
                                {
                                    // 상태를 hidding 으로 변환 시키는 것 같습니다.
                                    if (state == State.Hiding)
                                    {
                                        // R 의 마지막 비트에, 계산 값(해당 문자의 정수을 2로 나눈 나머지)를 더합니다.
                                        R += charValue % 2;
                                        // 2로 나눕니다.
                                        charValue /= 2;
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        // G 의 마지막 비트에, 계산 값(해당 문자의 정수을 2로 나눈 나머지)를 더합니다.
                                        G += charValue % 2;
                                        // 2로 나눕니다.
                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        // B 의 마지막 비트에, 계산 값(해당 문자의 정수을 2로 나눈 나머지)를 더합니다.
                                        B += charValue % 2;
                                        // 2로 나눕니다.
                                        charValue /= 2;
                                    }

                                    // 가져온 픽셀 값의 j,i 부분에 위에서 setting 한 RGB 값으로 변경합니다.
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        // 픽셀 요소 인덱스를 1 증가 시킵니다.
                        pixelElementIndex++; 

                        // 상태가 ??????????????????????????????
                        if (state == State.Filling_With_Zeros)
                        {
                            // zeros를 1 증가 . 이 부분은 잘 모르겠습니다.....? zero???????
                            zeros++;
                        }
                    }
                }
            }
            //최종 텍스트가 숨겨진 이미지를 반환 합니다.

            return bmp;
        }

        public static string extractText(Bitmap bmp)
        {
            // 컬러 언틸 인덱스 를 0으로 초기화 및 선언.
            int colorUnitIndex = 0;
            // 숨겨진 문자의 정수 값을 저장할 변수의 초기화 및 선언 
            int charValue = 0;

            // 비어있는 extractedText 문자열 변수 선언 및 공백 초기화
            string extractedText = String.Empty;

            // i 변수를 통해 사진의 높이 만큼 반복.
            for (int i = 0; i < bmp.Height; i++)
            {
                // j 변수를 통해 사진의 넓이 만큼 반복 
                for (int j = 0; j < bmp.Width; j++)
                {
                    // 해당하는 위치 (j,i)의 픽셀을 가져옵니다.
                    Color pixel = bmp.GetPixel(j, i);

                    // 3번 반복 n은 0,1,2
                    for (int n = 0; n < 3; n++)
                    {
                        // RGB의 마지막 LSB 부분을 추출 한 후, 8비트의 문자로 만드는 swtich 문입니다.
                        switch (colorUnitIndex % 3)
                        {
                            // 추출한 부분의 R 영역의 마지막 비트와 charvalue에 2를 곱한 값을 2진수 형식을 더한 후 저장.
                            case 0:
                                {

                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            // 추출한 부분의 G 영역의 마지막 비트와 charvalue에 2를 곱한 값을 2진수 형식을 더한 후 저장.
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;

                            // 추출한 부분의 B 영역의 마지막 비트와 charvalue에 2를 곱한 값을 2진수 형식을 더한 후 저장.    
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        // 컨트롤 유닛 인덱스를 1씩 증가 시킵니다.
                        colorUnitIndex++;

                        // 인덱스가, 0이거나 8의 배수 일 때 
                        if (colorUnitIndex % 8 == 0)

                        {
                            //비트를 정수 형을 변화여 charvalue 변수에 저장합니다.
                            charValue = reverseBits(charValue);

                            // 값이 0 일 때 
                            if (charValue == 0)
                            {
                                // 추출한 텍스트를 반환 합니다.
                                return extractedText;
                            }
                            // 정수형인 charValue의 값을 char 형으로 형변환 하여 캘기터형 c 에 저장합니다.
                            char c = (char)charValue;

                            //추출 되는 텍스트의 c 부분의 string을 저장합니다.
                            extractedText += c.ToString();
                        }
                    }
                }
            }
            // 추출한 텍스트를 반홥합니다.
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
