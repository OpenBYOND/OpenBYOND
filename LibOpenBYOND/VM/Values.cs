using OpenTK.Graphics;
using System.Collections.Generic;

namespace OpenBYOND.VM
{
    public class BYONDValue : Atom
    {
        /// <summary>
        /// The actual value. (Try to use Value, if available)
        /// </summary>
        public object _value;

        /// <summary>
        /// File this was defined in, or null.
        /// </summary>
        public string filename = null;

        /// <summary>
        /// Line of the originating file.
        /// </summary>
        public int line = -1;

        /// <summary>
        /// BYOND type of this value.
        /// </summary>
        public BYONDType type = null;

        /// <summary>
        /// Inherited from 
        /// </summary>
        public bool inherited = false;

        /// <summary>
        ///  /var/?
        /// </summary>
        public bool declarative = false;

        /// <summary>
        /// global, const, etc.
        /// </summary>
        public SpecialValueFlags special = SpecialValueFlags.NONE;

        /// <summary>
        /// Size of /list
        /// </summary>
        public int size = 0;
    }

    public class BYONDValue<T> : BYONDValue
    {
        public BYONDValue(T v, string filename = "", int line = -1)
        {
            _value = v;
            this.filename = filename;
            this.line = line;
        }
        public BYONDValue()
        {
        }
        public T Value
        {
            get { return (T)_value; }
            set { _value = value; }
        }

        public override string ToString()
        {
            return string.Format("/*UNKNOWN VALUE TYPE {0}*/{1}", typeof(T).FullName, _value);
        }
    }

    /// <summary>
    /// Representation of a string in OpenBYOND.
    /// </summary>
    public class BYONDString : BYONDValue<string>
    {
        /// <summary>
        /// Escape a string.
        /// </summary>
        /// <param name="inp"></param>
        /// <returns></returns>
        public static string Escape(string inp)
        {
            string o = inp;
            o = o.Replace("\n", "\\n");
            o = o.Replace("\t", "\\t");
            o = o.Replace("\"", "\\\"");
            return o;
        }
        public override string ToString()
        {
            return string.Format("\"{0}\"", Escape(Value));
        }
    }

    public class BYONDFileRef : BYONDValue<string>
    {
        public DMI GetDMI()
        {
            return DMIManager.GetDMI(Value);
        }

        public override string ToString()
        {
            return string.Format("\'{0}\'", Value);
        }
    }

    public class BYONDColor : BYONDValue<Color4>
    {
        static Dictionary<string, string> COLORS = new Dictionary<string, string>(){
            {"aqua", "#00FFFF"},
            {"black", "#000000"},
            {"blue", "#0000FF"},
            {"brown", "#A52A2A"},
            {"cyan", "#00FFFF"},
            {"fuchsia", "#FF00FF"},
            {"gold", "#FFD700"},
            {"gray", "#808080"},
            {"green", "#008000"},
            {"grey", "#808080"},
            {"lime", "#00FF00"},
            {"magenta", "#FF00FF"},
            {"maroon", "#800000"},
            {"navy", "#000080"},
            {"olive", "#808000"},
            {"purple", "#800080"},
            {"red", "#FF0000"},
            {"silver", "#C0C0C0"},
            {"teal", "#008080"},
            {"white", "#FFFFFF"},
            {"yellow", "#FFFF00"}
        };
        static Dictionary<string, BYONDColor> COLOR_LOOKUP = new Dictionary<string, BYONDColor>();
        static BYONDColor()
        {
            foreach (string name in COLORS.Keys)
                COLOR_LOOKUP[name] = FromString(COLORS[name]);
        }

        public BYONDColor():base()
        {
            Value = new Color4();
        }

        public float R
        {
            get { return Value.R; }
            set
            {
                Color4 c = Value;
                c.R = value;
            }
        }

        public float G
        {
            get { return Value.G; }
            set
            {
                Color4 c = Value;
                c.G = value;
            }
        }

        public float B
        {
            get { return Value.B; }
            set
            {
                Color4 c = Value;
                c.B = value;
            }
        }

        public float A
        {
            get { return Value.A; }
            set
            {
                Color4 c = Value;
                c.A = value;
            }
        }

        public static BYONDColor FromString(string colorstring, float alpha = 1f)
        {
            colorstring = colorstring.Trim();
            if (colorstring[0] == '#')
            {
                colorstring = colorstring.Substring(1);
                BYONDColor C = new BYONDColor();
                C.R = (float)byte.Parse(colorstring.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                C.G = (float)byte.Parse(colorstring.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                C.B = (float)byte.Parse(colorstring.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                C.A = alpha;
                return C;
            }
            else if (colorstring.StartsWith("rgb("))
            {
                BYONDColor C = new BYONDColor();
                colorstring = colorstring.Substring(4, colorstring.Length - 5);
                string[] bands = colorstring.Split(',');
                C.R = (float)uint.Parse(bands[0]) / 255f;
                C.G = (float)uint.Parse(bands[1]) / 255f;
                C.B = (float)uint.Parse(bands[2]) / 255f;
                C.A = alpha;
                return C;
            }
            else
            {
                return COLOR_LOOKUP[colorstring];
            }
        }
    }
}