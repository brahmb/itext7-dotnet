//Copyright (c) 2006, Adobe Systems Incorporated
//All rights reserved.
//
//        Redistribution and use in source and binary forms, with or without
//        modification, are permitted provided that the following conditions are met:
//        1. Redistributions of source code must retain the above copyright
//        notice, this list of conditions and the following disclaimer.
//        2. Redistributions in binary form must reproduce the above copyright
//        notice, this list of conditions and the following disclaimer in the
//        documentation and/or other materials provided with the distribution.
//        3. All advertising materials mentioning features or use of this software
//        must display the following acknowledgement:
//        This product includes software developed by the Adobe Systems Incorporated.
//        4. Neither the name of the Adobe Systems Incorporated nor the
//        names of its contributors may be used to endorse or promote products
//        derived from this software without specific prior written permission.
//
//        THIS SOFTWARE IS PROVIDED BY ADOBE SYSTEMS INCORPORATED ''AS IS'' AND ANY
//        EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//        WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//        DISCLAIMED. IN NO EVENT SHALL ADOBE SYSTEMS INCORPORATED BE LIABLE FOR ANY
//        DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//        (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//        LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//        ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//        (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//        SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//        http://www.adobe.com/devnet/xmp/library/eula-xmp-library-java.html
using System;
using System.Text;
using iTextSharp.Kernel.Xmp;

namespace iTextSharp.Kernel.Xmp.Impl
{
	/// <summary>Converts between ISO 8601 Strings and <code>Calendar</code> with millisecond resolution.
	/// 	</summary>
	/// <since>16.02.2006</since>
	public sealed class Iso8601Converter
	{
		/// <summary>Hides public constructor</summary>
		private Iso8601Converter()
		{
		}

		// EMPTY
		/// <summary>Converts an ISO 8601 string to an <code>XmpDateTime</code>.</summary>
		/// <remarks>
		/// Converts an ISO 8601 string to an <code>XmpDateTime</code>.
		/// Parse a date according to ISO 8601 and
		/// http://www.w3.org/TR/NOTE-datetime:
		/// <ul>
		/// <li>YYYY
		/// <li>YYYY-MM
		/// <li>YYYY-MM-DD
		/// <li>YYYY-MM-DDThh:mmTZD
		/// <li>YYYY-MM-DDThh:mm:ssTZD
		/// <li>YYYY-MM-DDThh:mm:ss.sTZD
		/// </ul>
		/// Data fields:
		/// <ul>
		/// <li>YYYY = four-digit year
		/// <li>MM = two-digit month (01=January, etc.)
		/// <li>DD = two-digit day of month (01 through 31)
		/// <li>hh = two digits of hour (00 through 23)
		/// <li>mm = two digits of minute (00 through 59)
		/// <li>ss = two digits of second (00 through 59)
		/// <li>s = one or more digits representing a decimal fraction of a second
		/// <li>TZD = time zone designator (Z or +hh:mm or -hh:mm)
		/// </ul>
		/// Note that ISO 8601 does not seem to allow years less than 1000 or greater
		/// than 9999. We allow any year, even negative ones. The year is formatted
		/// as "%.4d".
		/// <p>
		/// <em>Note:</em> Tolerate missing TZD, assume is UTC. Photoshop 8 writes
		/// dates like this for exif:GPSTimeStamp.<br />
		/// <em>Note:</em> DOES NOT APPLY ANYMORE.
		/// Tolerate missing date portion, in case someone foolishly
		/// writes a time-only value that way.
		/// </remarks>
		/// <param name="iso8601String">a date string that is ISO 8601 conform.</param>
		/// <returns>Returns a <code>Calendar</code>.</returns>
		/// <exception cref="iTextSharp.Kernel.Xmp.XmpException">Is thrown when the string is non-conform.
		/// 	</exception>
		public static XmpDateTime Parse(String iso8601String)
		{
			return Parse(iso8601String, new XmpDateTimeImpl());
		}

		/// <param name="iso8601String">a date string that is ISO 8601 conform.</param>
		/// <param name="binValue">an existing XmpDateTime to set with the parsed date</param>
		/// <returns>Returns an XmpDateTime-object containing the ISO8601-date.</returns>
		/// <exception cref="iTextSharp.Kernel.Xmp.XmpException">Is thrown when the string is non-conform.
		/// 	</exception>
		public static XmpDateTime Parse(String iso8601String, XmpDateTime binValue)
		{
			if (iso8601String == null) {
				throw new XmpException("Parameter must not be null", XmpError.BADPARAM);
			}
			if (iso8601String.Length == 0) {
				return binValue;
			}

			ParseState input = new ParseState(iso8601String);

			if (input.Ch(0) == '-') {
				input.Skip();
			}

			// Extract the year.
			int value = input.GatherInt("Invalid year in date string", 9999);
			if (input.HasNext() && input.Ch() != '-') {
				throw new XmpException("Invalid date string, after year", XmpError.BADVALUE);
			}

			if (input.Ch(0) == '-') {
				value = -value;
			}
			binValue.SetYear(value);
			if (!input.HasNext()) {
				return binValue;
			}
			input.Skip();


			// Extract the month.
			value = input.GatherInt("Invalid month in date string", 12);
			if (input.HasNext() && input.Ch() != '-') {
				throw new XmpException("Invalid date string, after month", XmpError.BADVALUE);
			}
			binValue.SetMonth(value);
			if (!input.HasNext()) {
				return binValue;
			}
			input.Skip();


			// Extract the day.
			value = input.GatherInt("Invalid day in date string", 31);
			if (input.HasNext() && input.Ch() != 'T') {
				throw new XmpException("Invalid date string, after day", XmpError.BADVALUE);
			}
			binValue.SetDay(value);
			if (!input.HasNext()) {
				return binValue;
			}
			input.Skip();

			// Extract the hour.
			value = input.GatherInt("Invalid hour in date string", 23);
			binValue.SetHour(value);
			if (!input.HasNext()) {
				return binValue;
			}

			// Extract the minute.
			if (input.Ch() == ':') {
				input.Skip();
				value = input.GatherInt("Invalid minute in date string", 59);
				if (input.HasNext() && input.Ch() != ':' && input.Ch() != 'Z' && input.Ch() != '+' && input.Ch() != '-') {
					throw new XmpException("Invalid date string, after minute", XmpError.BADVALUE);
				}
				binValue.SetMinute(value);
			}

			if (!input.HasNext()) {
				return binValue;
			}
			if (input.HasNext() && input.Ch() == ':') {
				input.Skip();
				value = input.GatherInt("Invalid whole seconds in date string", 59);
				if (input.HasNext() && input.Ch() != '.' && input.Ch() != 'Z' && input.Ch() != '+' && input.Ch() != '-') {
					throw new XmpException("Invalid date string, after whole seconds", XmpError.BADVALUE);
				}
				binValue.SetSecond(value);
				if (input.Ch() == '.') {
					input.Skip();
					int digits = input.Pos();
					value = input.GatherInt("Invalid fractional seconds in date string", 999999999);
					if (input.HasNext() && (input.Ch() != 'Z' && input.Ch() != '+' && input.Ch() != '-')) {
						throw new XmpException("Invalid date string, after fractional second",
							XmpError.BADVALUE);
					}
					digits = input.Pos() - digits;
					for (; digits > 9; --digits) {
						value = value/10;
					}
					for (; digits < 9; ++digits) {
						value = value*10;
					}
					binValue.SetNanoSecond(value);
				}
			}
			else if (input.Ch() != 'Z' && input.Ch() != '+' && input.Ch() != '-') {
				throw new XmpException("Invalid date string, after time", XmpError.BADVALUE);
			}


			if (!input.HasNext()) {
				// no Timezone at all
				return binValue;
			}
			if (input.Ch() == 'Z') {
				input.Skip();
			}
			else if (input.HasNext()) {
				if (input.Ch() == '+') {
				}
				else if (input.Ch() == '-') {
				}
				else {
					throw new XmpException("Time zone must begin with 'Z', '+', or '-'", XmpError.BADVALUE);
				}

				input.Skip();
				// Extract the time zone hour.
				if (input.HasNext()) {
					if (input.Ch() == ':') {
						input.Skip();
					}
					else {
						throw new XmpException("Invalid date string, after time zone hour", XmpError.BADVALUE);
					}
				}
			}

			// create a corresponding TZ and set it time zone
			binValue.SetTimeZone(TimeZone.CurrentTimeZone);

			if (input.HasNext()) {
				throw new XmpException("Invalid date string, extra chars at end", XmpError.BADVALUE);
			}

			return binValue;
		}

		/// <summary>Converts a <code>Calendar</code> into an ISO 8601 string.</summary>
		/// <remarks>
		/// Converts a <code>Calendar</code> into an ISO 8601 string.
		/// Format a date according to ISO 8601 and http://www.w3.org/TR/NOTE-datetime:
		/// <ul>
		/// <li>YYYY
		/// <li>YYYY-MM
		/// <li>YYYY-MM-DD
		/// <li>YYYY-MM-DDThh:mmTZD
		/// <li>YYYY-MM-DDThh:mm:ssTZD
		/// <li>YYYY-MM-DDThh:mm:ss.sTZD
		/// </ul>
		/// Data fields:
		/// <ul>
		/// <li>YYYY = four-digit year
		/// <li>MM	 = two-digit month (01=January, etc.)
		/// <li>DD	 = two-digit day of month (01 through 31)
		/// <li>hh	 = two digits of hour (00 through 23)
		/// <li>mm	 = two digits of minute (00 through 59)
		/// <li>ss	 = two digits of second (00 through 59)
		/// <li>s	 = one or more digits representing a decimal fraction of a second
		/// <li>TZD	 = time zone designator (Z or +hh:mm or -hh:mm)
		/// </ul>
		/// <p>
		/// <em>Note:</em> ISO 8601 does not seem to allow years less than 1000 or greater than 9999.
		/// We allow any year, even negative ones. The year is formatted as "%.4d".<p>
		/// <em>Note:</em> Fix for bug 1269463 (silently fix out of range values) included in parsing.
		/// The quasi-bogus "time only" values from Photoshop CS are not supported.
		/// </remarks>
		/// <param name="dateTime">an XmpDateTime-object.</param>
		/// <returns>Returns an ISO 8601 string.</returns>
		public static String Render(XmpDateTime dateTime) {
			return dateTime.GetCalendar().GetDateTime().ToString("s");
		}
	}

	/// <since>22.08.2006</since>
	internal class ParseState
	{
		private readonly String str;

		private int pos = 0;

		/// <param name="str">initializes the parser container</param>
		public ParseState(String str)
		{
			this.str = str;
		}

		/// <returns>Returns the length of the input.</returns>
		public virtual int Length()
		{
			return str.Length;
		}

		/// <returns>Returns whether there are more chars to come.</returns>
		public virtual bool HasNext()
		{
			return pos < str.Length;
		}

		/// <param name="index">index of char</param>
		/// <returns>Returns char at a certain index.</returns>
		public virtual char Ch(int index)
		{
			return index < str.Length ? str[index] : (char) 0x0000;
		}

		/// <returns>Returns the current char or 0x0000 if there are no more chars.</returns>
		public virtual char Ch()
		{
			return pos < str.Length ? str[pos] : (char) 0x0000;
		}

		/// <summary>Skips the next char.</summary>
		public virtual void Skip()
		{
			pos++;
		}

		/// <returns>Returns the current position.</returns>
		public virtual int Pos()
		{
			return pos;
		}

		/// <summary>Parses a integer from the source and sets the pointer after it.</summary>
		/// <param name="errorMsg">Error message to put in the exception if no number can be found
		/// 	</param>
		/// <param name="maxValue">the max value of the number to return</param>
		/// <returns>Returns the parsed integer.</returns>
		/// <exception cref="iTextSharp.Kernel.Xmp.XmpException">Thrown if no integer can be found.
		/// 	</exception>
		public virtual int GatherInt(String errorMsg, int maxValue)
		{
			int value = 0;
			bool success = false;
			char ch = Ch(pos);
			while ('0' <= ch && ch <= '9')
			{
				value = (value * 10) + (ch - '0');
				success = true;
				pos++;
				ch = Ch(pos);
			}
			if (success)
			{
				if (value > maxValue)
				{
					return maxValue;
				}
				else
				{
					if (value < 0)
					{
						return 0;
					}
					else
					{
						return value;
					}
				}
			}
			else
			{
				throw new XmpException(errorMsg, XmpError.BADVALUE);
			}
		}
	}
}