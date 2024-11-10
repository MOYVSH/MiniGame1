using System.Globalization;

namespace MOYV
{
    public static class StringUtils
    {
        public static bool TryParseFloat(string str, out float v)
        {
            return float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out v);
        }

        public static bool TryParseDouble(string str, out double v)
        {
            return double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out v);
        }

        public static bool TryParseInt(string str, out int v)
        {
            return int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out v);
        }
        
        public static bool CustomEndsWith(this string a, string b)
        {
            int ap = a.Length - 1;
            int bp = b.Length - 1;
    
            while (ap >= 0 && bp >= 0 && a [ap] == b [bp])
            {
                ap--;
                bp--;
            }
    
            return (bp < 0);
        }

        public static bool CustomStartsWith(this string a, string b)
        {
            int aLen = a.Length;
            int bLen = b.Length;
    
            int ap = 0; int bp = 0;
    
            while (ap < aLen && bp < bLen && a [ap] == b [bp])
            {
                ap++;
                bp++;
            }
    
            return (bp == bLen);
        }
    }
}