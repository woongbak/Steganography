using System;//네임 스페이스 추가
using System.Drawing;//네임 스페이스 추가

namespace Steganography //스테가노 그래피 네임 스페이스 추가
{
    class SteganographyHelper//클래스를 선언한다.
    {
        public enum State//변수를 선언한다.
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp)//전역변수를 선언한다.
        {
            State state = State.Hiding;//state state에 state.hiding를 저장한다.

            int charIndex = 0;//정수형 변수를 선언한다.

            int charValue = 0;//정수형 변수를 선언한다.

            long pixelElementIndex = 0;//long형 변수를 선언한다.

            int zeros = 0;//정수형 변수를 선언한다.

            int R = 0, G = 0, B = 0;//r,g,b에 0을 저장한다.

            for (int i = 0; i < bmp.Height; i++)//i가 0이며 bmp.Height보다 작을때 i를 1씩 증가시키며 반복분을 실행한다
            {
                for (int j = 0; j < bmp.Width; j++)//j가 0이며 bmp.width보다 작을때 j를 1씩 증가시키며 반복문을 사용한다.
                {
                    Color pixel = bmp.GetPixel(j, i);//color pixel에 bmp.getpixel의 j값과 i값을 넣어 실행한 값을 저장한다.

                    R = pixel.R - pixel.R % 2;//r값에 pixel.R - pixel.R % 2의 값을 저장한다.
                    G = pixel.G - pixel.G % 2;//위와 같은 방법으로 실행된다.
                    B = pixel.B - pixel.B % 2;//위와 같은 방법으로 실행된다.

                    for (int n = 0; n < 3; n++)//n이 0이고 3보다 작을때 1을 증가시키면서 반복문을 실행한다.
                    {
                        if (pixelElementIndex % 8 == 0)//pixelElementIndex을 8로 나눈 나머지의 값이 0이면 실행한다.
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//state의값이State.Filling_With_Zeros의 값과 같고 zeros의 값이 8이면 실행한다.
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)//pixelElementIndex에서 1을 빼고 3으로 나눈 나머지의 값이 2보다 작으면 실행한다.
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//bmp.setpixel함수에 j,i,color,fromargb(r,g,b)값을 넣어서 실행한다.
                                }

                                return bmp;//bmp의 값을 리턴한다.
                            }

                            if (charIndex >= text.Length)//charIndex의 값이 text.Length값보다 같거나 크면 실행한다.
                            {
                                state = State.Filling_With_Zeros;//state에 State.Filling_With_Zeros값을 저장한다.
                            }
                            else//아니면 실행한다.
                            {
                                charValue = text[charIndex++];//charValue에text[charIndex++]의 값을 넣어서 실행한다.
                            }
                        }

                        switch (pixelElementIndex % 3)//pixelElementIndex의 3으로 나눈 나머지의 값이 참이면 실행한다.
                        {
                            case 0://0일때 실행한다.
                                {
                                    if (state == State.Hiding)//state와State.Hiding 값이 같으면 실행한다.
                                    {
                                        R += charValue % 2;//r의 값에 charvalue의 값에 2를 나눈 나머지의 값을 더해서 저장한다.
                                        charValue /= 2;//charvalue를 2로 나눈다.
                                    }
                                } break;//스위치문을 탈출한다.
                            case 1://1일때 실행한다.
                                {
                                    if (state == State.Hiding)//state의 값과 State.Hiding의 값이 같으면 실행한다.
                                    {
                                        G += charValue % 2;//g의 값에 charvalue를 2로 나눈 나머지 값을 더한다.

                                        charValue /= 2;//charvalue를 2로 나눈다.
                                    }
                                } break;//스위치문을 탈출한다.
                            case 2://값이 2일때 실행한다.
                                {
                                    if (state == State.Hiding)//state와State.Hiding의 값이 같을때 실행한다.
                                    {
                                        B += charValue % 2;//b의값에 charvalue를 2로 나눈 나머지의 값을 더한다.

                                        charValue /= 2;//charvalue를 2로 나누어서 저장한다.
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//bmp.SetPixel의 함수에 j,i,color,fromargb(r,g,b)의 값을 넣어서 실행한다.
                                } break;//스위치 문을 탈출한다.
                        }

                        pixelElementIndex++;//pixelElementIndex의 값에 1을 더한다.

                        if (state == State.Filling_With_Zeros)//state과 State.Filling_With_Zeros이 같으면 실행한다.
                        {
                            zeros++;//zeros의 값에 1을 더한다.
                        }
                    }
                }
            }

            return bmp;//bmp를 리턴한다.
        }

        public static string extractText(Bitmap bmp)//extractText함수를 정의한다.
        {
            int colorUnitIndex = 0;//정수형 변수를 선언한다.
            int charValue = 0;//정수형 변수를 선언한다.

            string extractedText = String.Empty;//extractedText의 값에 비어있는 배열을 저장한다.

            for (int i = 0; i < bmp.Height; i++)//i가 0이고 bmp.height의 값보다 작을때 i를 1씩 증가시키며 반복문을 실행한다.
            {
                for (int j = 0; j < bmp.Width; j++)//j가 0이고 bmp.Width의 값보다 작을때 j를 1씩 증가시키며 반복문을 실행한다.
                {
                    Color pixel = bmp.GetPixel(j, i);//Color pixel에 bmp.GetPixel(j, i)의 값을 저장한다.
                    for (int n = 0; n < 3; n++)//만약 n이 0이고 n이 3보다 작을때 n을 1씩 증가시키면서 저장한다.
                    {
                        switch (colorUnitIndex % 3)//colorUnitIndex을 3으로 나눈 나머지의 값이 참이면 스위치문을 실행한다.
                        {
                            case 0://0일때 실행한다.
                                {
                                    charValue = charValue * 2 + pixel.R % 2;//charvalue의 값에 *를 곱하고 pixel.r을 2로 나눈 나머지값을 저장한다.
                                } break;//케이스 문을 탈출한다.
                            case 1://1일때 실행한다.
                                {
                                    charValue = charValue * 2 + pixel.G % 2;//charvalue의 값에 *를 곱하고 pixel.g을 2로 나눈 나머지값을 저장한다.
                                } break;//케이스문을 탈출한다.
                            case 2://2일대 실행한다.
                                {
                                    charValue = charValue * 2 + pixel.B % 2;//charvalue의 값에 *를 곱하고 pixel.b을 2로 나눈 나머지값을 저장한다.
                                } break;//케이스문을 탈출한다.
                        }

                        colorUnitIndex++;//colorUnitIndex값에 1을 더한다.

                        if (colorUnitIndex % 8 == 0)//colorUnitIndex에 8을 나눈 나머지의 값이 0이면 실행한다.
                        {
                            charValue = reverseBits(charValue);//charValue의 값에 reverseBits(charValue)의 값을 저장한다.

                            if (charValue == 0)//만약 charvalue값이 0이면 실행한다.
                            {
                                return extractedText;//extractedText의 값을 리턴한다.
                            }
                            char c = (char)charValue;//c의 문자형에charValue의 값을 저장한다.

                            extractedText += c.ToString();// extractedText에c.ToString()의 값을 더한다.
                        }
                    }
                }
            }

            return extractedText;//extractedText의 값을 리턴한다.
        }

        public static int reverseBits(int n)//reverseBits함수를 시작한다.
        {
            int result = 0;//정수형 변수를 선언한다.

            for (int i = 0; i < 8; i++)//만약 i의 값이 0이고 8보다 작다면 i를 1씩 증가시키며서 반복문을 실행한다.
            {
                result = result * 2 + n % 2;//result에 2를 곱하고 n을 2로 나눈 나머지의 값을 더해서 저장한다.

                n /= 2;//n을 2로 나눈다
            }

            return result;//result의값을 리턴한다.
        }
    }
}
