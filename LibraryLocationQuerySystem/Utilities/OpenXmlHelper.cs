using System.Text;
using System.Text.RegularExpressions;

namespace LibraryLocationQuerySystem.Utilities
{
    public class OpenXmlHelper
    {
        /// <summary>
        /// A~Z, 26
        /// AA~ZZ, 26*26
        /// ...
        /// </summary>
        static public int LetterToNum(string letter)
        {
            if (letter == string.Empty | letter == null) return -1;
            letter = letter.ToUpper();
            byte[] bytes = Encoding.ASCII.GetBytes(letter);
            int sum = (int)(26 * (Math.Pow(26, bytes.Length - 1) - 1) / 25);
            for (int i = 0; i < bytes.Length; i++)
            {
                sum = sum + (bytes[i] - 65) * (int)Math.Pow(26, bytes.Length - i - 1);
            }
            return sum + 1;
        }
        static public string NumToLetter(string num)
        {
            return NumToLetter(int.Parse(num));
        }
        static public string NumToLetter(int num)
        {
            num = num - 1;
            if (num < 0) return string.Empty;
            int sum = 26;
            int i = 1;
            for (; num >= sum; i++)
            {
                num = num - sum;
                sum = sum * 26;
            }
            Stack<int> rs = new Stack<int>(i);
            for (int ii = 0; ii < i; ii++)
            {
                rs.Push(num % 26);
                num = num / 26;
            }
            StringBuilder stringBuilder = new StringBuilder(i);
            byte[] bytes = new byte[1];
            while (rs.Count != 0)
            {
                bytes[0] = (byte)(rs.Pop() + 65);
                stringBuilder.Append(Encoding.ASCII.GetString(bytes));
            }
            return stringBuilder.ToString();
        }
        static public string GetCellAdd(int row, int col)
        {
            return NumToLetter(col) + row.ToString();
        }
        static public uint AddressSplitRow(string CellAdd)
        {
            MatchCollection mc = Regex.Matches(CellAdd, @"\d+");
            if (mc.Count == 0) return 0;
            return uint.Parse(mc[0].ToString());
        }
        static public uint AddressSplitColumnN(string CellAdd)
        {
            MatchCollection mc = Regex.Matches(CellAdd, @"[A-Z]+");
            if (mc.Count == 0) return 0;
            return (uint)LetterToNum(mc[0].ToString());
        }
        static public string AddressSplitColumnL(string CellAdd)
        {
            MatchCollection mc = Regex.Matches(CellAdd, @"[A-Z]+");
            if (mc.Count == 0) return string.Empty;
            return mc[0].ToString();
        }
    }
}
