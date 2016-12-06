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
        /// Hide message into last 1 bit of RGB data
        /// The message will be stored separately in 1 bit into last 1 bit of RGB data
        /// </summary>
        ///
        /// <param name="text">Message to be hidden</param>
        /// <param name="bmp">Bitmap to hide message</param>
        ///
        /// <returns>Bitmap type data with a message hidden</returns>
        public static Bitmap embedText(string text, Bitmap bmp)
        {
            // the state of hiding : hiding text or hiding NULLptr
            State state = State.Hiding;

            int charIndex = 0;

            int charValue = 0;

            long pixelElementIndex = 0;
            // zero counter
            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);

                    // set last 1 bit of pixel to 0 (NULL)
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    for (int n = 0; n < 3; n++)
                    /**
                     * loop for updating R, G, B
                     */
                    {
                        if (pixelElementIndex % 8 == 0)
                        /**
                         * 8 mean sizeof char (1 byte == 8 bit)
                         * this is for checking that 1 character is finished or not
                         */
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            /**
                             * checking for NULLptr is inserted or not
                             * zero counter flag (zeros) == 8 mean NULLptr is inserted
                             */
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)
                                /**
                                 * check if n is 0
                                 * cannot understand why this exist...
                                 */
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                // hidding is END so return!
                                return bmp;
                            }

                            if (charIndex >= text.Length)
                            /**
                             * Hidding text is end. prepare to insert NULLptr
                             */
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else
                            /**
                             * Hidding text is not end. Hide next character
                             */
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelElementIndex % 3)
                        /**
                         * who's turn? R, G, B
                         */
                        {
                            case 0: // R's turn
                                {
                                    if (state == State.Hiding)  // if not Hiding, hide 0
                                    {
                                        R += charValue % 2; // hide last bit of character
                                        charValue /= 2;     // delete last bit of character
                                    }
                                } break;
                            case 1: // G's turn
                                {
                                    if (state == State.Hiding)  // if not Hiding, hide 0
                                    {
                                        G += charValue % 2; // hide last bit of character

                                        charValue /= 2;     // delete last bit of character
                                    }
                                } break;
                            case 2: // B's turn
                                {
                                    if (state == State.Hiding)  // if not Hiding, hide 0
                                    {
                                        B += charValue % 2; // hide last bit of character

                                        charValue /= 2;     // delete last bit of character
                                    }
                                    // RGB hidding end. Set pixel(width, height, (R,G,B))
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        /**
                         * time to insert NULL
                         */
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        /// <summary>
        /// Extract hidden message from image
        /// </summary>
        ///
        /// <param name="bmp">Bitmap type of data with hidden message</param>
        ///
        /// <returns>Extracted message</returns>
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
                        /**
                         * who's turn? R, G, B
                         */
                        {
                            case 0: // R's turn
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                    // do left shift and store last 1 bit of R data
                                } break;
                            case 1: // G's turn
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                    // do left shift and store last 1 bit of G data
                                } break;
                            case 2: // B's turn
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                    // do left shift and store last 1 bit of B data
                                } break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)
                        /**
                         * When stored 1 character
                         */
                        {
                            charValue = reverseBits(charValue);
                            // Reverse bits

                            if (charValue == 0)
                            /**
                             * if the character is NULLptr than return (extract END)
                             */
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;

                            extractedText += c.ToString();
                        }
                    }
                }
            }

            // Checked all pixels
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
