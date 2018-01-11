using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace W3CXSD2001
{
    [Serializable]
    public sealed class GMonth : IXsd
    {
        // Fields
        private DateTime m_value;
        private static string[] formats = new string[] { "--MM--", "--MM--zzz" };

        // Methods
        public GMonth() { this.m_value = DateTime.MinValue; }

        public GMonth(DateTime value) { this.m_value = DateTime.MinValue; this.m_value = value; }

        public string GetXsdType() { return XsdType; }

        public static GMonth Parse(string value)
        {
            return new GMonth(DateTime.ParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None));
        }

        public override string ToString() { return this.m_value.ToString("--MM--", CultureInfo.InvariantCulture); }

        // Properties
        public DateTime Value
        {
            get { return this.m_value; }
            set { this.m_value = value; }
        }

        public static string XsdType { get { return "gMonth"; } }
    }


}
