using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace W3CXSD2001
{
    [Serializable]
    public sealed class GDay : IXsd
    {
        // Fields
        private DateTime m_value;
        private static string[] formats = new string[] { "---dd", "---ddzzz" };

        // Methods
        public GDay()
        {
            this.m_value = DateTime.MinValue;
        }

        public GDay(DateTime value)
        {
            this.m_value = DateTime.MinValue;
            this.m_value = value;
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static GDay Parse(string value)
        {
            return new GDay(DateTime.ParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None));
        }

        public override string ToString()
        {
            return this.m_value.ToString("---dd", CultureInfo.InvariantCulture);
        }

        // Properties
        public DateTime Value
        {
            get
            {
                return this.m_value;
            }
            set
            {
                this.m_value = value;
            }
        }

        public static string XsdType
        {
            get
            {
                return "gDay";
            }
        }
    }

}
