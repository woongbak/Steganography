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

            int charIndex = 0;	// refers the length of the text

            int charValue = 0;  // a character of text

            long pixelElementIndex = 0; // mod 3 result will 0:red 1:green 2:blue

            int zeros = 0;

            int R = 0, G = 0, B = 0;  // A pixel's LSBs of each R, G, B
	    // loop will be end at the end of storing all the characters of text
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i); // Get a pixel value
		   // Set the LSBs of each R G B to 0
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;
		    // loop 3 times for R G B each
                    for (int n = 0; n < 3; n++)
                    {	// A character is 8 bit, so when 8 bit values are stored,
                        if (pixelElementIndex % 8 == 0)
                        {   // If all characters are set in pixels
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {	// 8bit set but and remained G or B LSB is exist
                                if ((pixelElementIndex - 1) % 3 < 2)
				{   // set pixel value because the cases G and B blow don't set the modified value of pixel
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
				// if the character bits are 8's multiple
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
			/* state set to Filling_with_zeros, but this condition means
			all the characters are stored but remained pixels are still
			exist so zeros will be increased to 8 to handle remained pixel
			*/
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
                            case 0: // Extract a bit from R
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1: // Extract a bit from G
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2: // Extract a bit from B
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }
			// get next pixel
                        colorUnitIndex++;
			// if 8 bit extracted
                        if (colorUnitIndex % 8 == 0)
                        {   // 8 bit will be translated to a character
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
