using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBYOND.VM
{
    public class ObjectTree
    {
        // Can only be modified by the VM.
        public static readonly string[] ReservedSubtrees = new string[]{
            "/reserved",
        };

        /// <summary>
        /// All atoms, as a list.
        /// </summary>
        public List<Atom> Atoms = new List<Atom>();

        /// <summary>
        /// All atoms, as a tree.
        /// </summary>
        public Atom Tree = new Atom();

        /// <summary>
        /// Standard library has been loaded.
        /// </summary>
        public bool LoadedStdLib = false;

        /// <summary>
        /// Values of #defines.
        /// </summary>
        Dictionary<string, BYONDValue> Defines = new Dictionary<string, BYONDValue>();

        List<string> cpath = new List<string>();
        List<string> popLevels = new List<string>();
        List<string> InProc = new List<string>();
        List<string> ignoreLevel = new List<string>();  // Block Comments
        Dictionary<string, string> defineMatchers = new Dictionary<string, string>();
        List<string> comments = new List<string>();
        Dictionary<string, string> fileLayouts = new Dictionary<string, string>();

        static Dictionary<string, string> ignoreTokens = new Dictionary<string, string>(){
            {"/*","*/"},  // /* */
            {"{\"","\"}"} // {" "}
        };

        int pindent = 0;  // Previous Indent
        int ignoreStartIndent = -1;
        bool debugOn = true;
        bool ignoreDebugOn = true;

        bool LeavePreprocessorDirectives = false;

        static ObjectTree()
        {
            Dictionary<string, string> nit = new Dictionary<string, string>(ignoreTokens);
            foreach (KeyValuePair<string, string> kvp in ignoreTokens)
            {
                nit[kvp.Value] = null;
            }
            ignoreTokens = nit;
        }

        public ObjectTree()
        {
            SetDefine<int?>("OPENBYOND", 1);
        }

        private void SetDefine<T>(string name, T value)
        {
            Defines[name] = new BYONDValue<T>(value);
        }

        public string[] SplitPath(string path)
        {
            var o = new List<string>();
            var buf = new List<string>();
            bool inProc = false;
            foreach (string chunk in path.Split('/'))
            {
                if (!inProc)
                {
                    if (chunk.Contains('(') && !chunk.Contains(')'))
                    {
                        inProc = true;
                        buf.Add(chunk);
                    }
                    else
                    {
                        o.Add(chunk);
                    }
                }
                else
                {
                    if (chunk.Contains(')'))
                    {
                        buf.Add(chunk);
                        o.Add(string.Join("/", buf));
                        inProc = true;
                    }
                    else
                    {
                        buf.Add(chunk);
                    }
                }
            }
            return o.ToArray();
        }
    }
}
