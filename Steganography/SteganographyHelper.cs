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
        {
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0; // mod 3 result will 0:red 1:green 2:blue

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
		   // Set the LSB of R G B to 0
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
		    // loop 3 times for R G B each
                    for (int n = 0; n < 3; n++)
                    {	// pixel 8th bit
                        if (pixelElementIndex % 8 == 0)
                        {   // all 8bit is set
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {	// 8bit set but and remained bit is exist
                                if ((pixelElementIndex - 1) % 3 < 2)
				{   // set pixel color to value of R, G, B(modified)
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
				// if there's no remained, return bmp
                                return bmp;
                            }
			    // all text are embedded
                            if (charIndex >= text.Length)
                            {	// set state ..
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {	// index increase. Get next character of text
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)
			{
			    case 0: // pixel's R
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 1: // pixel's G
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            case 2: // pixel's B
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;
                                        charValue /= 2;
                                    } // if B is set, then set pixel(modified)
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }
			// increase pixel index
                        pixelElementIndex++;
			// set state
                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++; // zeros will increase to 8
                        }
                    }
                }
            }

            return bmp;
        }
	// method of Extract embedded text
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0: // Extract character from R
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1: // Extract character from G
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2: // Extract character from B
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }
			// get next pixel
                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue);

                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;
			    // extracted character to string(use method)
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
