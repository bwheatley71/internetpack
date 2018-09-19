﻿// Public Domain OpenPOP.NET < http://hpop.sourceforge.net > library MIME decoder code portions
//
// Author of OpenPOP.NET library is Kasper Foens ( http://foens.users.sourceforge.net )
// Full copy of OpenPOP.NET can be obtained from http://hpop.sourceforge.net
//

namespace RemObjects.InternetPack.Messages.Mime.Decode
{
	/// <summary>
	/// Used for decoding Quoted-Printable text.<br/>
	/// This is a robust implementation of a Quoted-Printable decoder defined in <a href="http://tools.ietf.org/html/rfc2045">RFC 2045</a> and <a href="http://tools.ietf.org/html/rfc2047">RFC 2047</a>.<br/>
	/// Every measurement has been taken to conform to the RFC.
	/// </summary>
	public static class QuotedPrintable
	{
		/// <summary>
		/// Decodes a Quoted-Printable String according to <a href="http://tools.ietf.org/html/rfc2047">RFC 2047</a>.<br/>
		/// RFC 2047 is used for decoding Encoded-Word encoded strings.
		/// </summary>
		/// <param name="toDecode">Quoted-Printable encoded String</param>
		/// <param name="encoding">Specifies which encoding the returned String will be in</param>
		/// <returns>A decoded String in the correct encoding</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="toDecode"/> or <paramref name="encoding"/> is <see langword="null"/></exception>
		public static String DecodeEncodedWord(String toDecode, Encoding encoding)
		{
			if (toDecode == null)
				throw new ArgumentNullException("toDecode");

			if (encoding == null)
				throw new ArgumentNullException("encoding");

			// Decode the QuotedPrintable String and return it
			return encoding.GetString(Rfc2047QuotedPrintableDecode(toDecode, true));
		}

		/// <summary>
		/// Decodes a Quoted-Printable String according to <a href="http://tools.ietf.org/html/rfc2045">RFC 2045</a>.<br/>
		/// RFC 2045 specifies the decoding of a body encoded with Content-Transfer-Encoding of quoted-printable.
		/// </summary>
		/// <param name="toDecode">Quoted-Printable encoded String</param>
		/// <returns>A decoded Byte array that the Quoted-Printable encoded String described</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="toDecode"/> is <see langword="null"/></exception>
		public static Byte[] DecodeContentTransferEncoding(String toDecode)
		{
			if (toDecode == null)
				throw new ArgumentNullException("toDecode");

			// Decode the QuotedPrintable String and return it
			return Rfc2047QuotedPrintableDecode(toDecode, false);
		}

		/// <summary>
		/// This is the actual decoder.
		/// </summary>
		/// <param name="toDecode">The String to be decoded from Quoted-Printable</param>
		/// <param name="encodedWordVariant">
		/// If <see langword="true"/>, specifies that RFC 2047 quoted printable decoding is used.<br/>
		/// This is for quoted-printable encoded words<br/>
		/// <br/>
		/// If <see langword="false"/>, specifies that RFC 2045 quoted printable decoding is used.<br/>
		/// This is for quoted-printable Content-Transfer-Encoding
		/// </param>
		/// <returns>A decoded Byte array that was described by <paramref name="toDecode"/></returns>
		/// <exception cref="ArgumentNullException">If <paramref name="toDecode"/> is <see langword="null"/></exception>
		/// <remarks>See <a href="http://tools.ietf.org/html/rfc2047#section-4.2">RFC 2047 section 4.2</a> for RFC details</remarks>
		private static Byte[] Rfc2047QuotedPrintableDecode(String toDecode, Boolean encodedWordVariant)
		{
			if (toDecode == null)
				throw new ArgumentNullException("toDecode");

			// Create a Byte array builder which is roughly equivalent to a StringBuilder
			using (MemoryStream byteArrayBuilder = new MemoryStream())
			{
				// Remove illegal control characters
				toDecode = RemoveIllegalControlCharacters(toDecode);

				// Run through the whole String that needs to be decoded
				for (Int32 i = 0; i < toDecode.Length; i++)
				{
					char currentChar = toDecode[i];
					if (currentChar == '=')
					{
						// Check that there is at least two characters behind the equal sign
						if (toDecode.Length - i < 3)
						{
							// We are at the end of the toDecode String, but something is missing. Handle it the way RFC 2045 states
							WriteAllBytesToStream(byteArrayBuilder, DecodeEqualSignNotLongEnough(toDecode.Substring(i)));

							// Since it was the last part, we should stop parsing anymore
							break;
						}

						// Decode the Quoted-Printable part
						String quotedPrintablePart = toDecode.Substring(i, 3);
						WriteAllBytesToStream(byteArrayBuilder, DecodeEqualSign(quotedPrintablePart));

						// We now consumed two extra characters. Go forward two extra characters
						i += 2;
					}
					else
					{
						// This character is not quoted printable hex encoded.

						// Could it be the _ character, which represents space
						// and are we using the encoded word variant of QuotedPrintable
						if (currentChar == '_' && encodedWordVariant)
						{
							// The RFC specifies that the "_" always represents hexadecimal 20 even if the
							// SPACE character occupies a different code position in the character set in use.
							byteArrayBuilder.WriteByte(0x20);
						}
						else
						{
							// This is not encoded at all. This is a literal which should just be included into the output.
							byteArrayBuilder.WriteByte((Byte)currentChar);
						}
					}
				}

				return byteArrayBuilder.ToArray();
			}
		}

		/// <summary>
		/// Writes all bytes in a Byte array to a stream
		/// </summary>
		/// <param name="stream">The stream to write to</param>
		/// <param name="toWrite">The bytes to write to the <paramref name="stream"/></param>
		private static void WriteAllBytesToStream(Stream stream, Byte[] toWrite)
		{
			stream.Write(toWrite, 0, toWrite.Length);
		}

		/// <summary>
		/// RFC 2045 states about robustness:<br/>
		/// <code>
		/// Control characters other than TAB, or CR and LF as parts of CRLF pairs,
		/// must not appear. The same is true for octets with decimal values greater
		/// than 126.  If found in incoming quoted-printable data by a decoder, a
		/// robust implementation might exclude them from the decoded data and warn
		/// the user that illegal characters were discovered.
		/// </code>
		/// Control characters are defined in RFC 2396 as<br/>
		/// <c>control = US-ASCII coded characters 00-1F and 7F hexadecimal</c>
		/// </summary>
		/// <param name="input">String to be stripped from illegal control characters</param>
		/// <returns>A String with no illegal control characters</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="input"/> is <see langword="null"/></exception>
		private static String RemoveIllegalControlCharacters(String input)
		{
			if (input == null)
				throw new ArgumentNullException("input");

			// First we remove any \r or \n which is not part of a \r\n pair
			input = RemoveCarriageReturnAndNewLinewIfNotInPair(input);

			// Here only legal \r\n is left over
			// We now simply keep them, and the \t which is also allowed
			// \x0A = \n
			// \x0D = \r
			// \x09 = \t)

			//return Regex.Replace(input, "[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");

			var lBuilder = new StringBuilder(input);
			for (int i = lBuilder.Length - 1; i >= 0; i--)
			{
				var lNumber = ord(lBuilder[i]);
				if ((lNumber >= 0 && lNumber <= 8) || (lNumber == 0x0B) || (lNumber == 0x0C) || ((lNumber >= 0x0E) && (lNumber <= 0x1F)) || (lNumber == 0x7F))
					lBuilder.Delete(i, 1);
			}
			return lBuilder.ToString();
		}

		/// <summary>
		/// This method will remove any \r and \n which is not paired as \r\n
		/// </summary>
		/// <param name="input">String to remove lonely \r and \n's from</param>
		/// <returns>A String without lonely \r and \n's</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="input"/> is <see langword="null"/></exception>
		private static String RemoveCarriageReturnAndNewLinewIfNotInPair(String input)
		{
			if (input == null)
				throw new ArgumentNullException("input");

			// Use this for building up the new String. This is used for performance instead
			// of altering the input String each time a illegal token is found
			StringBuilder newString = new StringBuilder(input.Length);

			for (Int32 i = 0; i < input.Length; i++)
			{
				// There is a character after it
				// Check for lonely \r
				// There is a lonely \r if it is the last character in the input or if there
				// is no \n following it
				if (input[i] == '\r' && (i + 1 >= input.Length || input[i + 1] != '\n'))
				{
					// Illegal token \r found. Do not add it to the new String

					// Check for lonely \n
					// There is a lonely \n if \n is the first character or if there
					// is no \r in front of it
				}
				else if (input[i] == '\n' && (i - 1 < 0 || input[i - 1] != '\r'))
				{
					// Illegal token \n found. Do not add it to the new String
				}
				else
				{
					// No illegal tokens found. Simply insert the character we are at
					// in our new String
					newString.Append(input[i]);
				}
			}

			return newString.ToString();
		}

		/// <summary>
		/// RFC 2045 says that a robust implementation should handle:<br/>
		/// <code>
		/// An "=" cannot be the ultimate or penultimate character in an encoded
		/// object. This could be handled as in case (2) above.
		/// </code>
		/// Case (2) is:<br/>
		/// <code>
		/// An "=" followed by a character that is neither a
		/// hexadecimal digit (including "abcdef") nor the CR character of a CRLF pair
		/// is illegal.  This case can be the result of US-ASCII text having been
		/// included in a quoted-printable part of a message without itself having
		/// been subjected to quoted-printable encoding.  A reasonable approach by a
		/// robust implementation might be to include the "=" character and the
		/// following character in the decoded data without any transformation and, if
		/// possible, indicate to the user that proper decoding was not possible at
		/// this point in the data.
		/// </code>
		/// </summary>
		/// <param name="decode">
		/// The String to decode which cannot have length above or equal to 3
		/// and must start with an equal sign.
		/// </param>
		/// <returns>A decoded Byte array</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="decode"/> is <see langword="null"/></exception>
		/// <exception cref="ArgumentException">Thrown if a the <paramref name="decode"/> parameter has length above 2 or does not start with an equal sign.</exception>
		private static Byte[] DecodeEqualSignNotLongEnough(String decode)
		{
			if (decode == null)
				throw new ArgumentNullException("decode");

			// We can only decode wrong length equal signs
			if (decode.Length >= 3)
				throw new ArgumentException("decode must have length lower than 3", "decode");

			// First char must be =
			if (decode[0] != '=')
				throw new ArgumentException("First part of decode must be an equal sign", "decode");

			// We will now believe that the String sent to us, was actually not encoded
			// Therefore it must be in US-ASCII and we will return the bytes it corrosponds to
			return Encoding.ASCII.GetBytes(decode);
		}

		/// <summary>
		/// This helper method will decode a String of the form "=XX" where X is any character.<br/>
		/// This method will never fail, unless an argument of length not equal to three is passed.
		/// </summary>
		/// <param name="decode">The length 3 character that needs to be decoded</param>
		/// <returns>A decoded Byte array</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="decode"/> is <see langword="null"/></exception>
		/// <exception cref="ArgumentException">Thrown if a the <paramref name="decode"/> parameter does not have length 3 or does not start with an equal sign.</exception>
		private static Byte[] DecodeEqualSign(String decode)
		{
			if (decode == null)
				throw new ArgumentNullException("decode");

			// We can only decode the String if it has length 3 - other calls to this function is invalid
			if (decode.Length != 3)
				throw new ArgumentException("decode must have length 3", "decode");

			// First char must be =
			if (decode[0] != '=')
				throw new ArgumentException("decode must start with an equal sign", "decode");

			// There are two cases where an equal sign might appear
			// It might be a
			//   - hex-String like =3D, denoting the character with hex value 3D
			//   - it might be the last character on the line before a CRLF
			//     pair, denoting a soft linebreak, which simply
			//     splits the text up, because of the 76 chars per line restriction
			if (decode.Contains("\r\n"))
			{
				// Soft break detected
				// We want to return String.Empty which is equivalent to a zero-length Byte array
				return new Byte[0];
			}

			// Hex String detected. Convertion needed.
			// It might be that the String located after the equal sign is not hex characters
			// An example: =JU
			// In that case we would like to catch the FormatException and do something else
			try
			{
				// The number part of the String is the last two digits. Here we simply remove the equal sign
				String numberString = decode.Substring(1);

				// Now we create a Byte array with the converted number encoded in the String as a hex value (base 16)
				// This will also handle illegal encodings like =3d where the hex digits are not uppercase,
				// which is a robustness requirement from RFC 2045.
				Byte[] oneByte = Convert.HexStringToByteArray(numberString);

				// Simply return our one Byte Byte array
				return oneByte;
			}
			catch (FormatException)
			{
				// RFC 2045 says about robust implementation:
				// An "=" followed by a character that is neither a
				// hexadecimal digit (including "abcdef") nor the CR
				// character of a CRLF pair is illegal.  This case can be
				// the result of US-ASCII text having been included in a
				// quoted-printable part of a message without itself
				// having been subjected to quoted-printable encoding.  A
				// reasonable approach by a robust implementation might be
				// to include the "=" character and the following
				// character in the decoded data without any
				// transformation and, if possible, indicate to the user
				// that proper decoding was not possible at this point in
				// the data.

				// So we choose to believe this is actually an un-encoded String
				// Therefore it must be in US-ASCII and we will return the bytes it corrosponds to
				return Encoding.ASCII.GetBytes(decode);
			}
		}
	}
}