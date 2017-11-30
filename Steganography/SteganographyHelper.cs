using System;
using System.Drawing;

namespace Steganography
{
    /// <summary>
    /// Steganography에 쓰이는 utility class
    /// <para><see cref="State"/></para>
    /// <para><seealso cref="embedText(string, Bitmap)"/></para>
    /// <para><seealso cref="extractText(Bitmap)"/></para>
    /// </summary>
    class SteganographyHelper
    {
        /// <summary>
        /// embedText에서의 작업 상태
        /// </summary>
        public enum State
        {
            /// <summary>
            /// 텍스트를 이미지에 저장하는 상태
            /// <para>embedText 실행 시 <c>state</c>는 이 상태로 초기화됩니다.</para>
            /// <para><c>text</c>의 문자를 모두 숨겼을 경우 <see cref="State.Filling_With_Zeros"/>로 전환됩니다.</para>
            /// </summary>
            Hiding,
            /// <summary>
            /// 0을 이미지에 저장하는 상태
            /// <para><see cref="State.Hiding"/>으로부터 전환됩니다.</para>
            /// <para>embedText 종료 시까지 상태는 변하지 않습니다.</para>
            /// </summary>
            Filling_With_Zeros
        };

        /// <summary>
        /// 이미지에 텍스트를 숨깁니다.
        /// <para>bmp를 Deep copy하지 않습니다. 반환값과 bmp는 같습니다.</para>
        /// </summary>
        /// <param name="text">숨기려는 문자열</param>
        /// <param name="bmp">문자열을 삽입할 이미지</param>
        /// <returns>텍스트가 숨겨진 이미지</returns>
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            //현재 상태를 저장할 열거자
            State state = State.Hiding;

            //text에서 char를 순차적으로 가져오기 위한 인덱스
            int charIndex = 0;

            //charIndex에 해당하는 text의 문자
            int charValue = 0;

            //bmp에서 접근한 픽셀 색상을 나타내는 인덱스. extractText의 colorUnitIndex와 역할이 같다.
            long pixelElementIndex = 0;

            //text를 전부 bmp에 숨긴 후 8개의 0을 저장하기 위한 카운터
            int zeros = 0;

            //색상값. 0~255. 마지막 비트에 값을 저장
            int R = 0, G = 0, B = 0;

            //높이 순회
            for (int i = 0; i < bmp.Height; i++)
            {
                //너비 순회
                for (int j = 0; j < bmp.Width; j++)
                {
                    //현재 픽셀 색상 읽음
                    Color pixel = bmp.GetPixel(j, i);

                    //R,G,B 색상의 마지막 비트를 0으로 만듬
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    //R,G,B 3개 채널이므로 3회 반복
                    for (int n = 0; n < 3; n++)
                    {
                        //8개 색상 순회마다 실행된다
                        if (pixelElementIndex % 8 == 0)
                        {
                            //텍스트 입력이 끝났고 0을 8개 저장했다면
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                //
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
                                //embed를 종료하고 이미지를 리턴한다.
                                return bmp;
                            }

                            //텍스트를 전부 저장했다면 Filling_With_Zeros 상태로 이행한다.
                            if (charIndex >= text.Length)
                            {
                                state = State.Filling_With_Zeros;
                            }
                            //텍스트가 남았다면 문자 값을 읽는다.
                            else
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        //현재 색상에 따라 분기
                        switch (pixelElementIndex % 3)
                        {
                            //case Color.R:
                            case 0:
                                {
                                    //아직 숨길 텍스트가 남았으면
                                    if (state == State.Hiding)
                                    {
                                        //charValue의 마지막 1비트를 R채널에 더함
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            //case Color.G:
                            case 1:
                                {
                                    //아직 숨길 텍스트가 남았으면
                                    if (state == State.Hiding)
                                    {
                                        //charValue의 마지막 1비트를 G채널에 더함
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            //case Color.B:
                            case 2:
                                {
                                    //아직 숨길 텍스트가 남았으면
                                    if (state == State.Hiding)
                                    {
                                        //charValue의 마지막 1비트를 B채널에 더함
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    //RGB 값이 완성되었으므로 픽셀 색상을 수정한다.
                                    //도중에 state가 Filing_With_Zeros가 되었다면 해당 색상부터 마지막 비트에 0이 저장될 것이다.
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        //색상 처리를 했으므로 인덱스 증가
                        pixelElementIndex++;

                        //0을 8개 저장할 거니까 0 몇개 썼는지 셈
                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            //여기까지 왔으면 입력한 텍스트가 너무 길어서 다 못썼을 확률이 매우 높다.
            //또한 그나마 저장된 마지막 문자가 깨질 위험도 있다.
            return bmp;
        }

        /// <summary>
        /// 이미지에서 텍스트를 추출합니다
        /// </summary>
        /// <param name="bmp">텍스트를 추출할 이미지</param>
        /// <returns>추출한 텍스트</returns>
        public static string extractText(Bitmap bmp)
        {
            //bmp에서 접근한 픽셀 색상을 나타내는 인덱스. embedText의 pixelElementIndex와 역할이 같다.
            int colorUnitIndex = 0;

            //색상에 저장된 비트를 조합하여 문자 값을 계산할 변수
            int charValue = 0;

            //반환할 값. 공백으로 시작. += charValue 하는 것으로 한글자씩 추가
            //C#에서 String은 불변성이라 StringBuilder를 쓰는게 좋지 않았나 싶다.
            string extractedText = String.Empty;

            //높이 순회
            for (int i = 0; i < bmp.Height; i++)
            {
                //너비 순회
                for (int j = 0; j < bmp.Width; j++)
                {
                    //현재 픽셀 색상 읽음
                    Color pixel = bmp.GetPixel(j, i);
                    //R,G,B 3개 채널이므로 3회 반복
                    for (int n = 0; n < 3; n++)
                    {
                        //현재 색상에 따라 분기
                        switch (colorUnitIndex % 3)
                        {
                            //각 색상값에 따라 1비트씩 charValue에 가져온다.

                            //case Color.R:
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            //case Color.G:
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2:
                            //case Color.B:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        //색상 처리를 했으므로 인덱스 증가
                        colorUnitIndex++;

                        //8개 색상에서 8개 비트를 읽었다면
                        if (colorUnitIndex % 8 == 0)
                        {
                            //저장을 역순으로 했으니 읽을때도 뒤집어야 함
                            charValue = reverseBits(charValue);

                            //Zeroing에 도달. 더이상의 문자 없으면 리턴
                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            //추출한 문자를 문자열에 붙임
                            char c = (char)charValue;

                            extractedText += c.ToString();
                        }
                    }
                }
            }

            return extractedText;
        }

        /// <summary>
        /// int의 마지막 8비트를 역순으로 만듬
        /// </summary>
        public static int reverseBits(int n)
        {
            //리턴 값 저장할 변수
            int result = 0;

            //8비트 순회
            for (int i = 0; i < 8; i++)
            {
                //n의 마지막 비트를 떼서 result 끝에 붙임
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }
    }
}
