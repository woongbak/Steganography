using System;
using System.Drawing;

namespace Steganography //Steganography namespace 정의
{
	class SteganographyHelper
	{
		//Steganography 도구의 상태를 나타냄
		public enum State
		{
			Hiding, //숨김 모드(0)
			Filling_With_Zeros //숨김 완료 모드(1)
		};


		/*
		* embedText
		* string text, Bitmap bmp
		* text를 bmp에 숨김
		* payload text
		* carrier bmp
		*/
		public static Bitmap embedText(string text, Bitmap bmp)
		{
			State state = State.Hiding; //상태를 숨김 모드로 설정

			int charIndex = 0; //숨긴 문자 갯수를 저장하는 변수

			int charValue = 0; //숨길 대상의 문자를 저장하는 변수

			long pixelElementIndex = 0; //R=0, G=1, B=2

			int zeros = 0; //문자 하나의 비트 수

			int R = 0, G = 0, B = 0; //R,G,B값을 모두 0으로 초기화

			for (int i = 0; i < bmp.Height; i++) //Bitmap 객체의 세로 길이(높이)부터 1비트씩 확인
			{
				for (int j = 0; j < bmp.Width; j++) //Bitmap 객체의 가로 길이(너비)부터 1비트씩 확인
				{
					Color pixel = bmp.GetPixel(j, i); //가로 세로 위치를 이용해 색 정보(픽셀)를 리턴

					R = pixel.R - pixel.R % 2;
					G = pixel.G - pixel.G % 2;
					B = pixel.B - pixel.B % 2; //해당 픽셀의 R,G,B 각각의 LSB비트를 0으로 초기화

					for (int n = 0; n < 3; n++) //3번 반복 (R,G,B)
					{
						if (pixelElementIndex % 8 == 0) //만약 pixelElementIndex가 0이나 8인 경우
						{
							if (state == State.Filling_With_Zeros && zeros == 8) //만약 해당 상태가 숨김 완료 모드이고 8비트(한 문자) 모두 확인했다면
							{
								if ((pixelElementIndex - 1) % 3 < 2) //R,G,B 모두 처리한 경우
								{
									bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //해당 R,B,B값으로 픽셀 설정
								}

								return bmp; //text가 숨겨져 있는 Bitmap bmp 반환
							}

							if (charIndex >= text.Length) //숨겨야 할 text를 모두 저장한 경우
							{
								state = State.Filling_With_Zeros; //상태를 숨김 완료 모드로 설정
							}
							else
							{
								//숨겨야 할 text를 모두 저장하지 못한 경우
								charValue = text[charIndex++];  //숨길 대상의 문자를 저장하고 charIndex(숨긴 문자 갯수) 1 증가
							}
						}
						//pixelElementIndex % 3으로 분류
						switch (pixelElementIndex % 3)
						{
						case 0: //0일때 : R 
						{
							if (state == State.Hiding) //상태가 숨김 모드이면
							{
								R += charValue % 2; //R에 LSB값 저장(0 또는 1)
							}
							charValue /= 2; //charValue를 2로 나눔(LSB 0으로 만듬)

						} break;
						case 1: //1일때 : G
						{
							if (state == State.Hiding) //상태가 숨김 모드면
							{
								G += charValue % 2; //G에 LSB값 저장(0 또는 1)

								charValue /= 2; //charValue를 2로 나눔(LSB 0으로 만듬)
							}
						} break;
						case 2: //2일때 : B
						{
							if (state == State.Hiding) //상태가 숨김 모드면
							{
								B += charValue % 2; //B에 LSB값 저장(0 또는 1)

								charValue /= 2; //charValue를 2로 나눔(LSB 0으로 만듬)
							}

							bmp.SetPixel(j, i, Color.FromArgb(R, G, B)); //해당 RGB에 해당하는 픽셀 세팅
						} break;
						}

						pixelElementIndex++; //pixelElementIndex 1 증가

						if (state == State.Filling_With_Zeros) //상태가 숨김 완료 모드이면
						{
							zeros++; //zero 1 증가
						}
					}
				}
			}

			return bmp; //Bitmap bmp 반환
		}


		/*
		* extractTest
		* Bitmap bmp
		* 숨겨진 텍스트를 추출
		*/
		public static string extractText(Bitmap bmp)
		{
			int colorUnitIndex = 0; //해독된 문자 개수를 저장하는 변수
			int charValue = 0;  //해독할 대상의 문자를 저장하는 변수

			string extractedText = String.Empty; //추출된 문자열을 저장하는 변수 초기화

			for (int i = 0; i < bmp.Height; i++) //Bitmap 객체의 세로 길이(높이)부터 1비트씩 확인
			{
				for (int j = 0; j < bmp.Width; j++) //Bitmap 객체의 가로 길이(너비)부터 1비트씩 확인
				{
					Color pixel = bmp.GetPixel(j, i); //가로 세로 위치를 이용해 색 정보(픽셀)를 리턴
					for (int n = 0; n < 3; n++) //3번 반복
					{
						switch (colorUnitIndex % 3) //colorUnitIndex % 3 값에 따라 수행
						{
						case 0: //0일때 : R
						{
							charValue = charValue * 2 + pixel.R % 2; //charValue에 2를 곱하고 R의 LSB를 더함
						} break;
						case 1: //1일때 : G
						{
							charValue = charValue * 2 + pixel.G % 2; //charValue에 2를 곱하고 G의 LSB를 더함
						} break;
						case 2: //2일때 : B
						{
							charValue = charValue * 2 + pixel.B % 2; //charValue에 2를 곱하고 B의 LSB를 더함
						} break;
						} 
							// 현재 문자열이 왼쪽 오른쪽 반대로 저장이 됨

							colorUnitIndex++; //colorUnitIndex 에 1을 더함

						if (colorUnitIndex % 8 == 0) //한문자(8bits)처리 완료된 경우
						{
							charValue = reverseBits(charValue); //charValue를 reverse하여 다시 저장

							if (charValue == 0) //만약 해독할 것이 더 이상 없을 경우
							{
								return extractedText; //숨긴 메시지 리턴
							}
							char c = (char)charValue; //찾은 문자열을 문자열에 넣기 위해서 형 변환

							extractedText += c.ToString(); //형변환한 문자를 문자열에 삽입
						}
					}
				}
			}

			return extractedText; //추출된 문자열 리턴
		}


		/*
		* reverseBits
		* n (정수)
		* 비트를 반대로 뒤집어 정렬
		*/
		public static int reverseBits(int n)
		{
			int result = 0;

			for (int i = 0; i < 8; i++) //8비트 동안 한 비트씩 수행
			{
				result = result * 2 + n % 2;
				n /= 2;
			}

			return result; //result 리턴
		}
	}
}
