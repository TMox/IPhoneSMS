using System;
using System.Security.Cryptography;
using System.Text;

namespace iPhoneMessageExport
{
    internal class MiscUtil
    {

        /// <summary>
        /// Converts date in DateTime format to Unix Timestamp.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static int datetimeToTimestamp(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0);

            return (int)(datetime - sTime).TotalSeconds;
        }

        /// <summary>
        /// Converts date in DateTime format to Unix Timestamp.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static long datetimeToTimestampMS(DateTime datetime)
        {
            if (datetime == DateTime.MinValue)
                return 0;
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0);
            if (datetime.IsDaylightSavingTime())
                datetime = datetime.AddHours(1);
            return (long)(datetime - sTime).TotalMilliseconds;
        }

        /// <summary>
        /// Converts date in Unix Timestamp format to DateTime.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime timestampToDateTime(long timestamp)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0);
            return sTime.AddMilliseconds(timestamp);
        }

        /// <summary>
        /// Converts date in Unix Timestamp format to DateTime.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime timestampToDateTimeiPhone(long timestamp)
        {
            DateTime sTime = new DateTime(2001, 1, 1, 0, 0, 0);
            return sTime.AddSeconds(timestamp);
        }


        /// <summary>
        /// Converts byte array to hex string.  Used by getSHAHash().
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string hexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns SHA1 Hash for specified string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string getSHAHash(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            using (var sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(bytes);
                return hexStringFromBytes(hashBytes);
            }
        }
    }
}
