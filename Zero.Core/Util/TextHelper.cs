using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zero.Core.Util
{
    /// <summary>
    /// 有关文本及编码方面的实用方法
    /// </summary>
    public class TextHelper
    {
        #region 获取（中文）字符串的每个字符的首字母（GetChineseInitials）


        /// <summary>
        /// 获取（中文）字符串的每个字符的首字母。
        /// </summary>
        /// <param name="chineseString"></param>
        /// <returns></returns>
        public static string GetChineseInitials(string chineseString)
        {
            StringBuilder initialsBuilder = new StringBuilder();
            for (int i = 0; i < chineseString.Length; i++)
            {
                char curChar = chineseString[i];
                if (curChar >= 0 && curChar < 256)
                    initialsBuilder.Append(curChar);
                else
                    initialsBuilder.Append(GetInitial(curChar));
            }
            return initialsBuilder.ToString().ToLower();
        }

        private const string CODE_DATA = "cjwgnspgcenegypbtwxzdxykygtpjnmjqmbsgzscyjsyyfpggbzgydywjkgaljswkbjqhyjwpdzlsgmr"
                 + "ybywwccgznkydgttngjeyekzydcjnmcylqlypyqbqrpzslwbdgkjfyxjwcltbncxjjjjcxdtqsqzycdxxhgckbphffss"
                 + "pybgmxjbbyglbhlssmzmpjhsojnghdzcdklgjhsgqzhxqgkezzwymcscjnyetxadzpmdssmzjjqjyzcjjfwqjbdzbjgd"
                 + "nzcbwhgxhqkmwfbpbqdtjjzkqhylcgxfptyjyyzpsjlfchmqshgmmxsxjpkdcmbbqbefsjwhwwgckpylqbgldlcctnma"
                 + "eddksjngkcsgxlhzaybdbtsdkdylhgymylcxpycjndqjwxqxfyyfjlejbzrwccqhqcsbzkymgplbmcrqcflnymyqmsqt"
                 + "rbcjthztqfrxchxmcjcjlxqgjmshzkbswxemdlckfsydsglycjjssjnqbjctyhbftdcyjdgwyghqfrxwckqkxebpdjpx"
                 + "jqsrmebwgjlbjslyysmdxlclqkxlhtjrjjmbjhxhwywcbhtrxxglhjhfbmgykldyxzpplggpmtcbbajjzyljtyanjgbj"
                 + "flqgdzyqcaxbkclecjsznslyzhlxlzcghbxzhznytdsbcjkdlzayffydlabbgqszkggldndnyskjshdlxxbcghxyggdj"
                 + "mmzngmmccgwzszxsjbznmlzdthcqydbdllscddnlkjyhjsycjlkohqasdhnhcsgaehdaashtcplcpqybsdmpjlpcjaql"
                 + "cdhjjasprchngjnlhlyyqyhwzpnccgwwmzffjqqqqxxaclbhkdjxdgmmydjxzllsygxgkjrywzwyclzmcsjzldbndcfc"
                 + "xyhlschycjqppqagmnyxpfrkssbjlyxyjjglnscmhcwwmnzjjlhmhchsyppttxrycsxbyhcsmxjsxnbwgpxxtaybgajc"
                 + "xlypdccwqocwkccsbnhcpdyznbcyytyckskybsqkkytqqxfcwchcwkelcqbsqyjqcclmthsywhmktlkjlychwheqjhtj"
                 + "hppqpqscfymmcmgbmhglgsllysdllljpchmjhwljcyhzjxhdxjlhxrswlwzjcbxmhzqxsdzpmgfcsglsdymjshxpjxom"
                 + "yqknmyblrthbcftpmgyxlchlhlzylxgsssscclsldclepbhshxyyfhbmgdfycnjqwlqhjjcywjztejjdhfblqxtqkwhd"
                 + "chqxagtlxljxmsljhdzkzjecxjcjnmbbjcsfywkbjzghysdcpqyrsljpclpwxsdwejbjcbcnaytmgmbapclyqbclzxcb"
                 + "nmsggfnzjjbzsfqyndxhpcqkzczwalsbccjxpozgwkybsgxfcfcdkhjbstlqfsgdslqwzkxtmhsbgzhjcrglyjbpmljs"
                 + "xlcjqqhzmjczydjwbmjklddpmjegxyhylxhlqyqhkycwcjmyhxnatjhyccxzpcqlbzwwwtwbqcmlbmynjcccxbbsnzzl"
                 + "jpljxyztzlgcldcklyrzzgqtgjhhgjljaxfgfjzslcfdqzlclgjdjcsnclljpjqdcclcjxmyzftsxgcgsbrzxjqqcczh"
                 + "gyjdjqqlzxjyldlbcyamcstylbdjbyregklzdzhldszchznwczcllwjqjjjkdgjcolbbzppglghtgzcygezmycnqcycy"
                 + "hbhgxkamtxyxnbskyzzgjzlqjdfcjxdygjqjjpmgwgjjjpkjsbgbmmcjssclpqpdxcdyykypcjddyygywchjrtgcnyql"
                 + "dkljczzgzccjgdyksgpzmdlcphnjafyzdjcnmwescsglbtzcgmsdllyxqsxsbljsbbsgghfjlwpmzjnlyywdqshzxtyy"
                 + "whmcyhywdbxbtlmswyyfsbjcbdxxlhjhfpsxzqhfzmqcztqcxzxrdkdjhnnyzqqfnqdmmgnydxmjgdhcdycbffallztd"
                 + "ltfkmxqzdngeqdbdczjdxbzgsqqddjcmbkxffxmkdmcsychzcmljdjynhprsjmkmpcklgdbqtfzswtfgglyplljzhgjj"
                 + "gypzltcsmcnbtjbhfkdhbyzgkpbbymtdlsxsbnpdkleycjnycdykzddhqgsdzsctarlltkzlgecllkjljjaqnbdggghf"
                 + "jtzqjsecshalqfmmgjnlyjbbtmlycxdcjpldlpcqdhsycbzsckbzmsljflhrbjsnbrgjhxpdgdjybzgdlgcsezgxlblg"
                 + "yxtwmabchecmwyjyzlljjshlgndjlslygkdzpzxjyyzlpcxszfgwyydlyhcljscmbjhblyjlycblydpdqysxktbytdkd"
                 + "xjypcnrjmfdjgklccjbctbjddbblblcdqrppxjcglzcshltoljnmdddlngkaqakgjgyhheznmshrphqqjchgmfprxcjg"
                 + "dychghlyrzqlcngjnzsqdkqjymszswlcfqjqxgbggxmdjwlmcrnfkkfsyyljbmqammmycctbshcptxxzzsmphfshmclm"
                 + "ldjfyqxsdyjdjjzzhqpdszglssjbckbxyqzjsgpsxjzqznqtbdkwxjkhhgflbcsmdldgdzdblzkycqnncsybzbfglzzx"
                 + "swmsccmqnjqsbdqsjtxxmbldxcclzshzcxrqjgjylxzfjphymzqqydfqjjlcznzjcdgzygcdxmzysctlkphtxhtlbjxj"
                 + "lxscdqccbbqjfqzfsltjbtkqbsxjjljchczdbzjdczjccprnlqcgpfczlclcxzdmxmphgsgzgszzqjxlwtjpfsyaslcj"
                 + "btckwcwmytcsjjljcqlwzmalbxyfbpnlschtgjwejjxxglljstgshjqlzfkcgnndszfdeqfhbsaqdgylbxmmygszldyd"
                 + "jmjjrgbjgkgdhgkblgkbdmbylxwcxyttybkmrjjzxqjbhlmhmjjzmqasldcyxyqdlqcafywyxqhz";

        private static char GetInitial(char _char)
        {
            byte[] gb2312Bytes = Encoding.GetEncoding("GB2312").GetBytes(new char[] { _char });
            int gb2312Code = Convert.ToInt32(string.Format("{0:D2}{1:D2}", (Int16)gb2312Bytes[0] - 160, (Int16)gb2312Bytes[1] - 160));

            if (gb2312Code >= 1601 && gb2312Code < 1637) return 'a';
            if (gb2312Code >= 1637 && gb2312Code < 1833) return 'b';
            if (gb2312Code >= 1833 && gb2312Code < 2078) return 'c';
            if (gb2312Code >= 2078 && gb2312Code < 2274) return 'd';
            if (gb2312Code >= 2274 && gb2312Code < 2302) return 'e';
            if (gb2312Code >= 2302 && gb2312Code < 2433) return 'f';
            if (gb2312Code >= 2433 && gb2312Code < 2594) return 'g';
            if (gb2312Code >= 2594 && gb2312Code < 2787) return 'h';
            if (gb2312Code >= 2787 && gb2312Code < 3106) return 'j';
            if (gb2312Code >= 3106 && gb2312Code < 3212) return 'k';
            if (gb2312Code >= 3212 && gb2312Code < 3472) return 'l';
            if (gb2312Code >= 3472 && gb2312Code < 3635) return 'm';
            if (gb2312Code >= 3635 && gb2312Code < 3722) return 'n';
            if (gb2312Code >= 3722 && gb2312Code < 3730) return 'o';
            if (gb2312Code >= 3730 && gb2312Code < 3858) return 'p';
            if (gb2312Code >= 3858 && gb2312Code < 4027) return 'q';
            if (gb2312Code >= 4027 && gb2312Code < 4086) return 'r';
            if (gb2312Code >= 4086 && gb2312Code < 4390) return 's';
            if (gb2312Code >= 4390 && gb2312Code < 4558) return 't';
            if (gb2312Code >= 4558 && gb2312Code < 4684) return 'w';
            if (gb2312Code >= 4684 && gb2312Code < 4925) return 'x';
            if (gb2312Code >= 4925 && gb2312Code < 5249) return 'y';
            if (gb2312Code >= 5249 && gb2312Code <= 5589) return 'z';
            if (gb2312Code >= 5601 && gb2312Code <= 8794)
            {
                int position = ((Int16)gb2312Bytes[0] - 160 - 56) * 94 + (Int16)gb2312Bytes[1] - 160;
                return CODE_DATA.Substring(position - 1, 1)[0];
            }

            return ' ';
        }

        #endregion

        #region 将ASCII字符串转为七位编码字节数组（Encode7Bit）


        /// <summary>
        /// 将ASCII字符串转为七位编码字节数组。
        /// </summary>
        /// <param name="sourceString"></param>
        /// <returns></returns>
        public static byte[] Encode7Bit(string sourceString)
        {
            byte[] sourceBytes = Encoding.ASCII.GetBytes(sourceString);
            int dataLength = sourceBytes.Length * 7;
            dataLength = dataLength % 8 == 0 ? dataLength / 8 : dataLength / 8 + 1;
            byte[] destBytes = new byte[dataLength];
            int byteIndex1 = 0;
            int bitIndex1 = 0;
            int byteIndex2 = 0;
            int bitIndex2 = 0;
            int index = 0;
            while (byteIndex1 < destBytes.Length)
            {
                int m = 8 * byteIndex1 + bitIndex1;
                int n = m + 6;
                byteIndex2 = byteIndex1;
                bitIndex2 = n - 8 * byteIndex2;
                if (bitIndex2 > 7)
                {
                    byteIndex2 = byteIndex1 + 1;
                    if (byteIndex2 >= destBytes.Length) break;
                    bitIndex2 = n - 8 * byteIndex2;
                    destBytes[byteIndex1] |= (byte)((int)sourceBytes[index] << bitIndex1);
                    destBytes[byteIndex2] |= (byte)((int)sourceBytes[index] >> (8 - bitIndex1));
                }
                else
                {
                    destBytes[byteIndex1] |= (byte)((int)sourceBytes[index] << bitIndex1);
                }
                bitIndex1 = bitIndex2 + 1;
                byteIndex1 = byteIndex2;
                if (bitIndex1 > 7)
                {
                    byteIndex1 = byteIndex2 + 1;
                    bitIndex1 = 0;
                }
                index++;
            }
            return destBytes;
        }

        #endregion

        #region 将七位编码的字节数组转为ASCII字符串（Decode7Bit）


        /// <summary>
        /// 将七位编码的字节数组转为ASCII字符串。
        /// </summary>
        /// <param name="sourceBytes"></param>
        /// <returns></returns>
        public static string Decode7Bit(byte[] sourceBytes)
        {
            int destBytesLength = sourceBytes.Length * 8 / 7;
            byte[] destBytes = new byte[destBytesLength];
            int byteIndex1 = 0;
            int bitIndex1 = 0;
            int byteIndex2 = 0;
            int bitIndex2 = 0;
            int index = 0;
            while (byteIndex1 < sourceBytes.Length)
            {
                int m = 8 * byteIndex1 + bitIndex1;
                int n = m + 6;
                byteIndex2 = byteIndex1;
                bitIndex2 = n - 8 * byteIndex2;
                if (bitIndex2 > 7)
                {
                    byteIndex2 = byteIndex1 + 1;
                    if (byteIndex2 >= sourceBytes.Length) break;
                    bitIndex2 = n - 8 * byteIndex2;
                    destBytes[index] = (byte)((int)sourceBytes[byteIndex1] >> bitIndex1);
                    destBytes[index] |= (byte)((int)sourceBytes[byteIndex2] << (8 - bitIndex1));
                }
                else
                {
                    destBytes[index] = (byte)((int)sourceBytes[byteIndex1] >> bitIndex1);
                }

                destBytes[index] &= 0x7F;
                bitIndex1 = bitIndex2 + 1;
                byteIndex1 = byteIndex2;
                if (bitIndex1 > 7)
                {
                    byteIndex1 = byteIndex2 + 1;
                    bitIndex1 = 0;
                }
                index++;
            }
            return Encoding.ASCII.GetString(destBytes);
        }

        #endregion

        #region 将字节数组转换为十六进制字符串


        private static char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        /// <summary>
        /// 将字节数组转换为十六进制字符串。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToHexString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }

        #endregion

        #region 将十六进制字符串转换为字节数组


        /// <summary>
        /// 将十六进制字符串转换为字节数组。
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] FromHexString(string hexString)
        {
            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0, j = 0; i < hexString.Length; i += 2, j++)
            {
                bytes[j] = ToByte(hexString[i]);
                bytes[j] <<= 4;
                bytes[j] |= ToByte(hexString[i + 1]);

            }
            return bytes;
        }

        private static byte ToByte(char hexChar)
        {
            switch (hexChar)
            {
                case '0':
                    return 0;
                case '1':
                    return 1;
                case '2':
                    return 2;
                case '3':
                    return 3;
                case '4':
                    return 4;
                case '5':
                    return 5;
                case '6':
                    return 6;
                case '7':
                    return 7;
                case '8':
                    return 8;
                case '9':
                    return 9;
                case 'A':
                    return 10;
                case 'B':
                    return 11;
                case 'C':
                    return 12;
                case 'D':
                    return 13;
                case 'E':
                    return 14;
                case 'F':
                    return 15;
                case 'a':
                    return 10;
                case 'b':
                    return 11;
                case 'c':
                    return 12;
                case 'd':
                    return 13;
                case 'e':
                    return 14;
                case 'f':
                    return 15;
                default:
                    return 0;
            }
        }

        #endregion

    }
}
