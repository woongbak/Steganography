using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        // 2 types of state
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        };

        // Function for embedding text into an image
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            State state = State.Hiding;     // change state to 'Hiding'

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;     // Index of pixel element. Red = 0, Green = 1, Blue = 2.

            int zeros = 0;

            int R = 0, G = 0, B = 0;


            for (int i = 0; i < bmp.Height; i++)         // For loop for height of the image
            {
                for (int j = 0; j < bmp.Width; j++)      // For loop for the width of the image
                {
                    Color pixel = bmp.GetPixel(j, i);    // get the color of the pixel having j width and i height

                    // Change the last bit of R, G, B values to 0.
                    //     This is done by subtracting 'VALUE mod 2' from the original value.
                    //     ('VALUE mod 2' will be 1 for an odd number, and 0 for an even number.)
                    //     So, the last bit of the result will always be 0.
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)    // Loop 3 times
                    {
                        if (pixelElementIndex % 8 == 0)    // For the 8th bit
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)    // If the state is 'Filling_With_Zeros' and zeros is 8
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)    // If the modulus of the 7th bit is less than 2
                                {
                                    // Change the color of the pixel at position (j,i) with the values of R, G, and B
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            // Get the character value from the text.
                            // If character index is greater than the text length
                            if (charIndex >= text.Length)
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        // Encrypt character values with the help of pixel elements (R, G or B)
                        // Check if state is 'Hiding'
                        // and change the value of the pixel element, and the character value
                        switch (pixelElementIndex % 3)
                        {
                            // For R value
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        R += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            // For G value
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;
                                        charValue /= 2;
                                    }
                                } break;
                            // For B value
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;
                                        charValue /= 2;
                                    }

                                    // Change the color of the pixel at position (j,i) with the values of R, G, and B
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        pixelElementIndex++;

                        // If the state is 'Filling_With_Zeros', increase the value of zeros
                        if (state == State.Filling_With_Zeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        // Function for extracting text from steganography image
        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)          // For loop for height of the image
            {
                for (int j = 0; j < bmp.Width; j++)       // For loop for the width of the image
                {
                    Color pixel = bmp.GetPixel(j, i);     // get the color of the pixel having j width and i height
                    for (int n = 0; n < 3; n++)
                    {
                        // Decrypt character values with the concerned pixel elements
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

                        colorUnitIndex++;

                        // If all the character values for color units have been decoded
                        if (colorUnitIndex % 8 == 0)
                        {
                            // Reverse the character value and store it back to 'charValue'
                            charValue = reverseBits(charValue);
                            
                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;    // The extracted string

                            // append extracted string
                            extractedText += c.ToString();
                        }
                    }
                }
            }

            return extractedText;
        }

        // Function for reversing the bits of the character value
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
