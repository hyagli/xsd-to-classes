using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace W3CXSD2001
{
    [Serializable]
    public sealed class GYear : IXsd
    {
        // Fields
        private int m_sign;
        private DateTime m_value;
        private static string[] formats = new string[] { "yyyy", "'+'yyyy", "'-'yyyy", "yyyyzzz", "'+'yyyyzzz", "'-'yyyyzzz" };

        // Methods
        public GYear()
        {
            this.m_value = DateTime.MinValue;
            this.m_sign = 0;
        }

        public GYear(DateTime value)
        {
            this.m_value = DateTime.MinValue;
            this.m_sign = 0;
            this.m_value = value;
        }

        public GYear(DateTime value, int sign)
        {
            this.m_value = DateTime.MinValue;
            this.m_sign = 0;
            this.m_value = value;
            this.m_sign = sign;
        }

        public string GetXsdType() { return XsdType; }

        public static GYear Parse(string value)
        {
            int sign = 0;
            if (value[0] == '-')
            {
                sign = -1;
            }
            return new GYear(DateTime.ParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None), sign);
        }

        public override string ToString()
        {
            if (this.m_sign < 0)
            {
                return this.m_value.ToString("'-'yyyy", CultureInfo.InvariantCulture);
            }
            return this.m_value.ToString("yyyy", CultureInfo.InvariantCulture);
        }

        // Properties
        public int Sign
        {
            get { return this.m_sign; }
            set { this.m_sign = value; }
        }

        public DateTime Value
        {
            get { return this.m_value; }
            set { this.m_value = value; }
        }

        public static string XsdType { get { return "gYear"; } }
    }


}
