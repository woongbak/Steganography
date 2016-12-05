
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

          /// <summary>
          /// 이미지에 텍스트를 숨기는 메서드
          /// 
          /// #  숨김 원리
          /// 1. 전달인자로 받은 비트맵 객체의 각 요소를 순차 접근하여 해당 요소의 픽셀에서 RGB 추출
          /// 2. 추출한 RGB의 LSB를 0으로 세팅
          /// 3. RGB의 LSB에 텍스트 문자열에서 가져온 문자를 한 비트 씩 삽입
          ///  - 8비트 단위로 문자열에서 새로운 문자열 가져옴
          ///  - RGB는 각각 mod3 합동으로 설정하여 인덱스를 이용해 문자를 숨길 위치 판별 가능
          ///  - 해당 픽셀의 RGB 값이 모두 변경될 때 마다 비트맵 객체에 반영
          /// 4. 이미지에 텍스트 문자열을 모두 삽입했다면 마지막으로 텍스트 문자의 종료를 알리는 0(8비트) 삽입
          ///  - 삽입 완료 시점에서 해당 픽셀의 RGB 값에서 변경이 B 값까지 이루어 지지 않아 비트맵 객체에 변경된 이미지가
          ///  적용되지 않은 경우, 나머지 값을 0으로 비트맵 객체에 반영
          ///  5. 텍스트 삽입이 완료된 비트맵 객체 반환
          /// </summary>
          /// <param name="text">Crypto() 메서드에 의해 AES 암호화 된 텍스트(password + plain text) </param>
          /// <param name="bmp">비트맵 포맷으로 저장 된 이미지</param>
          /// <returns></returns>
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

               //텍스트를 삽입할 bmp 포맷 이미지의 각 요소 접근
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);  //해당 bmp 포맷 이미지에서 픽셀을 가져옴

                         //픽셀 RGB의 LSB를 0으로 셋팅
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                         //이미지에 텍스트 삽입을 위해 해당 픽셀 RGB의 LSB를 R,G,B 순으로 텍스트의 ASCII 문자를 1비트씩 삽입하여 셋팅
                    for (int n = 0; n < 3; n++)
                    {

                        if (pixelElementIndex % 8 == 0)     //R : pixelElementIndex mod3 = 0
                                                            //G : pixelElementIndex mod3 = 1
                                                            //B : pixelElementIndex mod3 = 2
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)     //마지막 8비트(0)까지 이미지에 삽입 완료가 된 경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)  //삽입 완료 시점에서 해당 픽셀의 RGB 값 중 
                                                                      //삼입이 이루어 지지 않고 남아있는 값이 있는 경우  

                                {
                                   //해당 경우 변경된 RGB가 bmp 객체에 변경이 반영되지 않았으므로
                                   //변경된 값을 bmp 객체의 해당 요소에 세팅
                                   bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;  //변경 완료된 bmp 객체를 반환
                            }

                            if (charIndex >= text.Length)   //텍스트의 모든 문자를 다 숨긴 경우
                            {
                                state = State.Filling_With_Zeros;     //state를 텍스트 숨김 완료 상태로 만들어 줌.
                                                                      //텍스트 숨김 완료 상태가 되면 추가로 8비트를 0으로 세팅과정을 거쳐
                                                                      //스테가노그래피 과정 종료
                            }
                            else//ASCII 8비트를 모두 숨긴 후에도 숨길 문자가 남은 경우
                            {
                                charValue = text[charIndex++];   //텍스트의 다음 문자를 가져옴
                            }
                        }

                        switch (pixelElementIndex % 3)
                        {
                            case 0:     //R : pixelElementIndex mod3 = 0
                                {
                                    if (state == State.Hiding)   //state가 아직 문자를 숨기는 중 이라면
                                    {
                                        R += charValue % 2; //R의 LSB를 숨길 문자의  LSB로 변환
                                        charValue /= 2;     //charValue>>1 (underflow)
                                    }
                                } break;
                            case 1:     //G : pixelElementIndex mod3 = 1
                                        {
                                    if (state == State.Hiding)   //state가 아직 문자를 숨기는 중 이라면
                                    {
                                        G += charValue % 2; //G의 LSB를 숨길 문자의  LSB로 변환
                                        charValue /= 2;     //charValue>>1 (underflow)
                                    }
                                } break;
                            case 2:     //B : pixelElementIndex mod3 = 2
                                {
                                    if (state == State.Hiding)   //state가 아직 문자를 숨기는 중 이라면
                                    {
                                        B += charValue % 2; //B의 LSB를 숨길 문자의  LSB로 변환
                                        charValue /= 2;     //charValue>>1 (underflow)
                                    }

                                    //  특정 픽셀의 RGB 값 변경이 완료되면 변경된 픽셀을 bmp 포맷의 해당 요소에 세팅 
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); 
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;    //zeros:0~7 8회 반복 (8회 반복동안 R,G,B는 LSB가 0인 상태로 저장됨.)
                        }
                    }
                }
            }

            return bmp;  //변경 완료된 bmp 객체를 반환
        }




          /// <summary>
          /// 이미지에 삽입된 텍스트를 추출하는 메서드
          /// </summary>
          /// <param name="bmp"></param>
          /// <returns></returns>
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

               //bmp 포맷 이미지의 각 요소에 순차적으로 접근
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);  //해당 요소의 픽셀을 가져옴
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;    //R값의 LSB 추출
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;    //G값의 LSB 추출
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;    //B값의 LSB 추출
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)   //이미지에서 8비트가 추출된 경우
                        {
                            charValue = reverseBits(charValue);  //비트를 역순으로 변경 : 문자 1개

                            if (charValue == 0)   //이미지에서 추출한 문자가 0인 경우 이미지 삽입이 종료되는 것을 의미
                            {
                                return extractedText;  //추출된 텍스트 리턴
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString();  //문자열에 누적
                        }
                    }
                }
            }

            return extractedText;  //추출된 텍스트 리턴(마지막 문자가 0이 아닌 경우도 추출한 문자 리턴)
        }


          /// <summary>
          /// 8비트를 역순으로 저장
          /// </summary>
          /// <param name="n">변경 대상 8비트 데이터</param>
          /// <returns result>비트가 역순으로 변경된 데이터</returns>
        public static int reverseBits(int n)
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2; //result에 n의 LSB 누적(with left shift)

                n /= 2;  //n>>1(underflow)
            }

            return result;    //변경된 값 리턴
        }
    }
}
