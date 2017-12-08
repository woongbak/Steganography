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

		// to hide your secret text
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;

            int charIndex = 0;
            int charValue = 0;
            long pixelElementIndex = 0;
            int zeros = 0;
            int R = 0, G = 0, B = 0;

			// loop entire image. row first
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
					// get pixel value
                    Color pixel = bmp.GetPixel(j, i);

					// maybe.. normalize..?
					// change to a multiple of 2
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;


                    for (int n = 0; n < 3; n++)
                    {
						// when 8 bits are finished
                        if (pixelElementIndex % 8 == 0)
                        {
							// if last 8 bits change to 0
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

							// if it finished hiding the text, change the status
                            if (charIndex >= text.Length)
                            {
                                state = State.Filling_With_Zeros;
                            }
							// not finished hiding
                            else
                            {
								// change text to integer
                                charValue = text[charIndex++];
                            }
                        }

						// encode the chagned text value
                        switch (pixelElementIndex % 3)
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

						// count the bit num
                        pixelElementIndex++;

						// if finished hiding text, to change last 8bits to 0, count the zeros num.
                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

		// extract your hiding text
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

			// loop entire image. row first
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
					// get one pixel
                    Color pixel = bmp.GetPixel(j, i);

					// decode the value..?
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

						// count the bit num
                        colorUnitIndex++;

						// if one byte (8bits) decode is finished
						// change integer to text?
                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue);

							// if charValue is 0, it means text is finished
                            if (charValue == 0)
                            {
                                return extractedText;
                            }
							
							// int to char
                            char c = (char)charValue;

							// make final text
                            extractedText += c.ToString();
                        }
                    }
                }
            }

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
