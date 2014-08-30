/**
 * YES I REALIZE THIS IS AN UNHOLY CLUSTERFUCK.
 * 
 * PLEASE FEEL FREE TO TIDY UP.
 *  - N3XYPOO
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Text.RegularExpressions;
using System.IO;

namespace OpenBYOND.VM
{
    public class ObjectTree
    {
        Regex REGEX_TABS = new Regex(@"^(?<tabs>[\t\s]*)");
        Regex REGEX_ATOMDEF = new Regex(@"^(?<tabs>\t*)(?<atom>[a-zA-Z0-9_/]+)\\{?\\s*$");
        Regex REGEX_ABSOLUTE_PROCDEF = new Regex(@"^(?<tabs>\t*)(?<atom>[a-zA-Z0-9_/]+)/(?<proc>[a-zA-Z0-9_]+)\((?<args>.*)\)\\{?\s*$");
        Regex REGEX_RELATIVE_PROCDEF = new Regex(@"^(?<tabs>\t*)(?<proc>[a-zA-Z0-9_]+)\((?<args>.*)\)\\{?\\s*$");
        Regex REGEX_LINE_COMMENT = new Regex(@"//.*?$");
        private static readonly ILog log = LogManager.GetLogger(typeof(Utils));

        // Can only be modified by the VM.
        public static readonly string[] ReservedSubtrees = new string[]{
            "/reserved",
        };

        private static readonly string[] reservedWords = new string[] { "else", "break", "return", "continue", "spawn" };

        /// <summary>
        /// All atoms, as a Dictionary.
        /// </summary>
        public Dictionary<string, Atom> Atoms = new Dictionary<string, Atom>();

        /// <summary>
        /// All atoms, as a tree.
        /// </summary>
        public Atom Tree = new Atom("");

        /// <summary>
        /// Standard library has been loaded.
        /// </summary>
        public bool LoadedStdLib = false;

        /// <summary>
        /// Values of //defines.
        /// </summary>
        Dictionary<string, BYONDValue> Defines = new Dictionary<string, BYONDValue>();

        /// <summary>
        /// Components of the path.
        /// </summary>
        List<string> cpath = new List<string>();

        /// <summary>
        /// How many path chunks to slice off when we go down an indent.
        /// </summary>
        Stack<int> popLevels = new Stack<int>();

        List<string> InProc = new List<string>();
        Stack<string> ignoreLevel = new Stack<string>();  // Block Comments
        Dictionary<string, Regex> defineMatchers = new Dictionary<string, Regex>();
        List<string> comments = new List<string>();
        Dictionary<string, string> fileLayouts = new Dictionary<string, string>();

        static Dictionary<string, string> ignoreTokens = new Dictionary<string, string>(){
            {"/*","*/"},  // /* */
            {"{\"","\"}"} // {" "}
        };

        int pindent = 0;  // Previous Indent
        int ignoreStartIndent = -1;

        public bool debugOn = true;
        public bool ignoreDebugOn = true;

        bool LeavePreprocessorDirectives = false;
        Proc loadingProc = null;
        private string comment;
        private string lineBeforePreprocessing;
        private string current_filename;

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
            SetDefine("OPENBYOND", 1);
        }

        public void SetDefine(string name, int value)
        {
            Defines[name] = new BYONDValue<int?>(value);
        }

        // Indent debugging.
        private void debug(string filename, int line, List<string> path, string message)
        {
            log.DebugFormat("{0}:{1}: {2} - {3}", filename, line, "/".join(path), message);
        }

        private Atom ProcessAtom(string filename, int ln, string line, string atom, List<string> atom_path, int numtabs, string[] procArgs = null)
        {
            // Reserved words that show up on their own
            if (reservedWords.Contains(atom))
                return null;

            // Other things to ignore (false positives, comments)
            if (atom.StartsWith("var/") || atom.StartsWith("//"))
                return null;

            // Things part of a string or list.
            if (numtabs > 0 && atom.Trim().StartsWith("/"))
                return null;

            if (debugOn)
                log.DebugFormat("{} > {}", numtabs, line.TrimEnd());

            // Global scope (no tabs)
            if (numtabs == 0)
            {
                this.cpath = atom_path;
                // Ensure cpath has a slash at the front (empty first entry)
                if (this.cpath.Count == 0)
                    this.cpath.Add("");
                else if (this.cpath[0] != "")
                    this.cpath.Insert(0, "");

                // Create new poplevel stack, add current path length to it.
                this.popLevels = new Stack<int>();
                this.popLevels.Push(this.cpath.Count);

                if (this.debugOn)
                    debug(filename, ln, this.cpath, "0 - " + string.Join("/", atom_path));

            }
            else if (numtabs > pindent) // Going up a tab level.
            {
                // Add path to cpath.
                this.cpath.AddRange(atom_path);

                // Add new poplevel with current path length.
                this.popLevels.Push(atom_path.Count);

                if (this.debugOn)
                    debug(filename, ln, this.cpath, ">");

            }
            else if (numtabs < pindent) // Going down.
            {
                if (this.debugOn)
                    log.DebugFormat("({0} - {1})={2}: {3}", this.pindent, numtabs, this.pindent - numtabs, string.Join("/", this.cpath));

                // This is complex as fuck, so bear with me.
                // For every poplevel we've lost, we need to slice off some path chunks or we fuck up our context.
                //
                // /butt
                //   ugly
                //     dirty    // /butt/ugly/dirty, 2 poplevels both with content 1 (number of things we added to path)
                //   nice       // Here, we slice off 1 path segment, and add "nice", getting /butt/nice.
                for (int i = 0; i < (this.pindent - numtabs + 1); i++)
                {
                    // Pop a poplevel out, find out how many path chunks we need to remove.
                    var popsToDo = this.popLevels.Pop();

                    if (this.debugOn)
                        log.DebugFormat(" pop {0} {1}", popsToDo, this.popLevels);

                    // Now pop off the path segments.
                    for (int j = 0; j < popsToDo; j++)
                    {
                        this.cpath.RemoveAt(this.cpath.Count - 1);

                        if (this.debugOn)
                            log.DebugFormat("  pop {0}/{1}: {2}", i + 1, popsToDo, "/".join(this.cpath));
                    }
                }

                // Add new stuff.
                this.cpath.AddRange(atom_path);

                // Add new poplevel for the new stuff.
                this.popLevels.Push(atom_path.Count);

                if (this.debugOn)
                    debug(filename, ln, this.cpath, "<");

            }
            else if (numtabs == this.pindent) // Same level.
            {
                // Same as above, but we're only going down one indent.
                var levelsToPop = this.popLevels.Pop();

                // Pop off path segments.
                for (int i = 0; i < levelsToPop; i++)
                    this.cpath.RemoveAt(this.cpath.Count - 1);

                // New stuff
                this.cpath.AddRange(atom_path);

                // New poplevel.
                this.popLevels.Push(atom_path.Count);

                if (this.debugOn)
                    log.DebugFormat("popLevels: {0}", this.popLevels.Count);
                if (this.debugOn)
                    debug(filename, ln, this.cpath, ">");
            }

            var origpath = string.Join("/", this.cpath);

            // definition?
            List<string> defs = new List<string>();

            // Trim off /proc or /var, if needed.
            List<string> prep_path = new List<string>(this.cpath);

            foreach (string special in new string[] { "proc" })
            {
                if (prep_path.Contains(special))
                {
                    defs.Add(special);
                    prep_path.Remove(special);
                }
            }

            var npath = "/".join(prep_path);

            if (!Atoms.ContainsKey(npath))
            {
                if (procArgs != null)
                {
                    //assert npath.endswith(')')

                    // if origpath != npath:
                    //    print(origpath,proc_def)

                    //var proc = new Proc(npath, procArgs, filename, ln);
                    //proc.origpath = origpath;
                    //proc.definition = defs.Contains("proc");
                    //this.Atoms[npath] = proc;
                }
                else
                {
                    this.Atoms[npath] = new Atom(npath, filename, ln);
                }
                // if this.debugOn: print('Added ' + npath)
            }
            this.pindent = numtabs;
            return this.Atoms[npath];
        }

        private void AddCodeToProc(int startIndent, string code)
        {
            if (code.Contains('\n'))
            {
                foreach (string line in code.Split('\n'))
                {
                    AddCodeToProc(startIndent, line);
                }
            }
            else
            {
                Match m = REGEX_TABS.Match(code);
                int numtabs = 0;
                if (m != null)
                {
                    numtabs = m.Groups["tabs"].Length;
                }
                int i = Math.Max(1, numtabs - startIndent);
                //loadingProc.AddCode(i, string.format("/* {0} */ {1}",i,code.Trim())
                // loadingProc.AddCode(i, code.TrimEnd());
            }
        }
        private void finishComment(string cleansed_line = ""/* .NET 4.5: ,[CallerFilePath] string filename="", [CallerLineNumber] int line=0*/)
        {
            comments.Add(comment);
            //this.fileLayout += [('COMMENT', len(this.comments) - 1)]
            /*
            if (ignoreDebugOn)
                log.DebugFormat("finishComment({0}): {1}", line, comment);
            */
            if (loadingProc != null && cleansed_line == "" && !comment.Trim().StartsWith("//"))
                AddCodeToProc(ignoreStartIndent, comment);
            comment = "";
        }
        private void handleOBToken(string name, string context, params object[] p)
        {
            //Atom cAtom;
            //if(context!=null)
            //    cAtom = Atoms[context];
            name = string.Format("ob_{0}", name.ToLower());
            GetType().GetMethod(name).Invoke(this, p);
        }

        /// <summary>
        /// Like Peek(), but for context.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="ln"></param>
        /// <param name="line"></param>
        /// <param name="numtabs"></param>
        /// <param name="atom_prefix"></param>
        /// <returns></returns>
        private string DetermineContext(string filename, int ln, string line, int numtabs, List<string> atom_prefix = null)
        {
            if (atom_prefix == null) atom_prefix = new List<string>();

            // Global context
            if (numtabs == 0)
            {
                return null;

            }
            else if (numtabs > this.pindent)
            {
                return "/".join(Utils.ConcatLists<string>(this.cpath, atom_prefix));

            }
            else if (numtabs < this.pindent)
            {
                var cpath_copy = new List<string>(this.cpath);
                // Chop off path segments.
                for (int i = 0; i < (this.pindent - numtabs + 1); i++)
                {
                    var popsToDo = this.popLevels.Pop();
                    for (int j = 0; j < popsToDo; j++)
                        cpath_copy.RemoveAt(cpath_copy.Count - 1);
                }
                cpath_copy.AddRange(atom_prefix);
                return "/".join(cpath_copy);

            }
            else if (numtabs == this.pindent)
            {
                var cpath_copy = new List<string>(this.cpath);
                // Pop one level.
                var popsToDo = this.popLevels.Pop();
                for (int j = 0; j < popsToDo; j++)
                    cpath_copy.RemoveAt(cpath_copy.Count - 1);
                cpath_copy.AddRange(atom_prefix);
                return "/".join(cpath_copy);
            }
            return null;
        }

        private string[] SplitPath(string path)
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

        /////////////////////////
        // HAIRINESS BELOW
        public void ProcessFile(string filename)
        {
            this.cpath.Clear();
            this.popLevels.Clear();
            this.pindent = 0;
            this.ignoreLevel.Clear();
            this.debugOn = false;
            this.ignoreDebugOn = false;
            this.ignoreStartIndent = -1;
            this.loadingProc = null;
            this.comment = "";
            //this.fileLayout = []
            this.lineBeforePreprocessing = "";
            this.current_filename = filename;

            Match m;
            using (TextReader f = File.OpenText(filename))
            {
                int ln = 0;
                ignoreLevel.Clear();

                while (f.Peek() != -1)
                {
                    string line = f.ReadLine();

                    ln++;

                    bool skipNextChar = false;
                    string nl = "";

                    line = line.TrimEnd();

                    this.lineBeforePreprocessing = line;
                    var line_len = line.Length;
                    for (int i = 0; i < line_len; i++)
                    {
                        string c = line.Substring(i, 1);
                        string nc = "";
                        if (line_len > i + 1)
                            nc = line.Substring(i + 1, 1);
                        string tok = c + nc;

                        if (skipNextChar)
                        {
                            ignoreDebug(string.Format("Skipping {0}.", tok));
                            skipNextChar = false;
                            // this.comment += c
                            ignoreDebug(string.Format("this.comment = {0}.", this.comment));
                            continue;
                        }
                        if (tok == "//")
                        {
                            //if(this.ignoreDebugOn) debug(filename,ln,this.cpath,'{} ({})'.format(tok,len(ignoreLevel)))
                            if (ignoreLevel.Count == 0)
                            {
                                this.comment = line.Substring(i);
                                // if this.ignoreDebugOn: print('this.comment = {}.'.format(repr(this.comment)))
                                // print('Found '+this.comment)
                                this.finishComment(cleansed_line: nl);
                                break;
                            }
                        }
                        if (ignoreTokens.ContainsKey(tok))
                        {
                            string pc = "";
                            if (i > 0)
                                pc = line.Substring(i - 1, 1);
                            if (tok == "{\"" && pc == "\"")
                            {
                                this.comment += c;
                                continue;
                            }
                            // if this.ignoreDebugOn: print(repr(this.ignoreTokens[tok]))
                            string stop = ignoreTokens[tok];
                            // End comment
                            if (stop == null)
                            {
                                if (ignoreLevel.Count > 0)
                                {
                                    if (ignoreLevel.Peek() == tok)
                                    {
                                        skipNextChar = true;
                                        this.comment += tok;
                                        ignoreLevel.Pop();
                                        if (ignoreLevel.Count == 0)
                                            this.finishComment();
                                        continue;
                                    }
                                    else
                                    {
                                        this.comment += c;
                                        continue;
                                    }
                                }
                            }
                            else  // Start comment
                            {
                                skipNextChar = true;
                                ignoreLevel.Push(stop);
                                this.comment = tok;
                                continue;
                            }
                            if (this.ignoreDebugOn)
                                debug(filename, ln, this.cpath, string.Format("{0} ({1})", tok, ignoreLevel.Count));
                        }
                        if (ignoreLevel.Count == 0)
                            nl += c;
                        else
                            this.comment += c;
                    }
                    if (line != nl)
                    {
                        ignoreDebug("IN : " + line);
                        line = nl;
                        ignoreDebug("OUT: " + line);
                        ignoreDebug(string.Format("this.comment = {0}.", comment));
                    }
                    if (ignoreLevel.Count > 0)
                    {
                        this.comment += "\n";
                        continue;
                    }

                    line = REGEX_LINE_COMMENT.Replace(line, "");

                    if (line.Trim() == "")
                    {
                        //if (loadingProc != null)
                        //    loadingProc.AddBlankLine();
                        continue;
                    }

                    /////////////////////////////
                    // Preprocessing defines.
                    if (line.Trim().StartsWith("#"))
                    {
                        if (line.EndsWith("\\")) continue;
                        var tokenChunks = line.Split('#');
                        tokenChunks = tokenChunks[1].Split();
                        var directive = tokenChunks[0];

                        if (directive == "define")
                        {
                            // //define SOMETHING Value
                            var defineChunks = new List<string>(line.Split(new char[] { ' ', '\t' }, 3));
                            if (defineChunks.Count == 2)
                                defineChunks.Add("1");
                            else if (defineChunks.Count == 3)
                                defineChunks[2] = this.PreprocessLine(defineChunks[2]);
                            // print(repr(defineChunks))
                            try
                            {
                                Defines[defineChunks[1]] = new BYONDNumber(float.Parse(defineChunks[2]), filename, ln);
                            }
                            catch
                            {
                                Defines[defineChunks[1]] = new BYONDString(defineChunks[2], filename, ln);
                            }
                            //this.fileLayout += [("DEFINE", defineChunks[1], defineChunks[2])]
                        }
                        else if (directive == "undef")
                        {
                            var undefChunks = line.Split(new char[] { ' ', '\t' }, 2);
                            if (Defines.ContainsKey(undefChunks[1]))
                                Defines.Remove(undefChunks[1]);
                            //this.fileLayout += [("UNDEF", undefChunks[1])]

                            // OpenBYOND tokens.
                        }
                        else if (directive.StartsWith("__OB_"))
                        {
                            int numtabs = 0;
                            m = REGEX_TABS.Match(line);
                            if (m != null)
                                numtabs = m.Groups["tabs"].Length;
                            string atom = this.DetermineContext(filename, ln, line, numtabs);
                            // if atom is None: continue
                            // print("OBTOK {0}".format(repr(tokenChunks)))
                            this.handleOBToken(tokenChunks[0].Replace("__OB_", ""), atom, tokenChunks.Skip(1).ToArray());
                            // this.fileLayout += [("OBTOK", atom.path)]
                            continue;
                        }
                        else
                        {
                            var chunks = line.Split(' ');
                            //this.fileLayout += [("PP_TOKEN", line)]
                            log.WarnFormat("BUG: Unhandled preprocessor directive #{0} in {1}:{2}", directive, filename, ln);
                        }
                        continue;
                    }

                    // Preprocessing
                    line = this.PreprocessLine(line);

                    m = REGEX_TABS.Match(this.lineBeforePreprocessing);
                    if (m != null)
                    {
                        var numtabs = m.Groups["tabs"].Length;
                        if (this.ignoreStartIndent > -1 && this.ignoreStartIndent < numtabs)
                        {
                            if (loadingProc != null)
                            {
                                // this.loadingProc.AddCode(numtabs - this.ignoreStartIndent, this.lineBeforePreprocessing.strip())
                                this.AddCodeToProc(this.ignoreStartIndent, this.lineBeforePreprocessing);
                            }
                            if (debugOn) log.DebugFormat("TABS: {0} ? {1} - {2}: {3}", numtabs, this.ignoreStartIndent, this.loadingProc, line);
                            continue;
                        }
                        else
                        {
                            if (this.debugOn && this.ignoreStartIndent > -1) log.DebugFormat("BREAK ({0} -> {1}): {2}", this.ignoreStartIndent, numtabs, line);
                            this.ignoreStartIndent = -1;
                            this.loadingProc = null;
                        }
                    }
                    else
                    {
                        if (this.debugOn && this.ignoreStartIndent > -1) log.Debug("BREAK " + line);
                        this.ignoreStartIndent = -1;
                        this.loadingProc = null;
                    }
                    if (!line.Trim().StartsWith("var/"))
                    {
                        m = REGEX_ATOMDEF.Match(line);
                        if (m != null)
                        {
                            var numtabs = m.Groups["tabs"].Length;
                            var atom_str = m.Groups["atom"].Value;
                            var atom_path = new List<string>(this.SplitPath(atom_str));
                            var atom = this.ProcessAtom(filename, ln, line, atom_str, atom_path, numtabs);
                            //if(atom==null) continue;
                            //this.fileLayout += [("ATOMDEF", atom.path)];
                            continue;
                        }
                        m = REGEX_ABSOLUTE_PROCDEF.Match(line);
                        if (m != null)
                        {
                            var numtabs = m.Groups["tabs"].Length;
                            var atom = string.Format("{0}/{1}({2})", m.Groups["atom"].Value, m.Groups["proc"].Value, m.Groups["args"].Value);
                            var atom_path = new List<string>(this.SplitPath(atom));
                            // print("PROCESSING ABS PROC AT INDENT > " + str(numtabs) + " " + atom+" -> "+repr(atom_path))
                            var proc = this.ProcessAtom(filename, ln, line, atom, atom_path, numtabs, m.Groups["args"].Value.Split(','));
                            if (proc == null)
                                continue;
                            this.ignoreStartIndent = numtabs;
                            this.loadingProc = (Proc)proc;
                            //this.fileLayout += [("PROCDEF", proc.path)]
                            continue;
                        }
                        m = REGEX_RELATIVE_PROCDEF.Match(line);
                        if (m != null)
                        {
                            var numtabs = m.Groups["tabs"].Length;
                            var atom = string.Format("{}({})", m.Groups["proc"].Value, m.Groups["args"].Value);
                            var atom_path = new List<string>(this.SplitPath(atom));
                            // print("IGNORING RELATIVE PROC AT INDENT > " + str(numtabs) + " " + line)
                            var proc = this.ProcessAtom(filename, ln, line, atom, atom_path, numtabs, m.Groups["args"].Value.Split(','));
                            if (proc == null)
                                continue;
                            this.ignoreStartIndent = numtabs;
                            this.loadingProc = (Proc)proc;
                            //this.fileLayout += [("PROCDEF", proc.path)]
                            continue;
                        }
                    }
                    var path = "/".join(this.cpath);
                    // if len(this.cpath) > 0 and "proc" in this.cpath:
                    //    continue
                    // if "proc" in this.cpath:
                    //    continue
                    if (line.Contains("=") /*||  line.Trim().StartsWith("var/")*/)
                    {
                        if (!Atoms.ContainsKey(path))
                            this.Atoms[path] = new Atom(path);
                        string name;
                        Atom prop;
                        this.consumeVariable(line, filename, ln, out name, out prop);
                        this.Atoms[path].Properties[name] = prop;
                        //this.fileLayout += [("VAR", path, name)];
                    }
                }
                //this.fileLayouts[filename] = this.fileLayout
            }
        }

        private void ignoreDebug(string p)
        {
            if (this.ignoreDebugOn) log.Debug(p);
        }

        private void consumeVariable(string line, string filename, int ln, out string name, out Atom prop)
        {
            name = "";
            prop = null;

            bool declaration = false;
            string value = null;
            int size = -1;

            string decl = "";

            if (LeavePreprocessorDirectives)
                line = decl = this.lineBeforePreprocessing.Trim();
            else
                decl = line.Trim();
            if (line.Contains("[") && !line.Contains("list("))
            {
                string[] arrparts = line.Split(new char[] { '[' }, 2);
                string line_split = arrparts[0];
                string arr_decl = arrparts[1];
                string str_size = "";

                var idx = arr_decl.IndexOf(']');
                if (idx >= 0)
                {
                    str_size = arr_decl.Substring(0, idx);
                }
                else
                {
                    log.WarnFormat("{0}:{1}: MALFORMED CODE: Unable to find ]", filename, ln);
                    log.Warn(line);
                }

                if (str_size != "")
                {
                    size = int.Parse(str_size);
                }
                // print(repr({"size":size,"line":line_split}))
                line = line_split;
            }
            if (line.Contains("="))
            {
                var chunks = line.Split(new char[] { '=' }, 2);
                decl = chunks[0].Trim();
                value = chunks[1].Trim();
            }
            else
            {
                decl = line.Trim();
                value = null;
            }
            // print(repr({"decl":decl,"value":value}))

            // (var)(/global|const|tmp)(/type/fragment)name
            if (decl.StartsWith("var/"))
            {
                declaration = true;
                decl = decl.Substring(4);
            }
            List<string> pathchunks = decl.Split('/').ToList();
            name = pathchunks.Last();
            string special = null;
            var typepath = "/";
            if (declaration)
            {
                switch (pathchunks[0])
                {
                    case "tmp":
                    case "global":
                    case "const":
                        special = pathchunks[0];
                        pathchunks = pathchunks.Skip(1).ToList();
                        break;
                }
                if (!pathchunks.Contains("list") && size != -1)
                {
                    pathchunks.Insert(0, "list");
                }
                typepath = "/" + ("/".join(pathchunks.Take(pathchunks.Count - 1)));
            }

            float parseval;
            if (typepath != "/")
            {
                if (value == "null")
                {
                    prop = new BYONDNull(filename, ln) { type = new BYONDType(typepath), declarative = declaration, special = special, size = size };
                }
                else
                {
                    prop = new Atom(typepath, filename, ln);//{declarative=declaration, special=special,size=size};
                }
            }
            else if (value != null && value[0] == '"')
            {
                prop = new BYONDString(value.Substring(1, value.Length - 1), filename, ln) { declarative = declaration, special = special, size = size };
            }
            else if (value != null && value[0] == '\'')
            {
                prop = new BYONDFileRef(value.Substring(1, value.Length - 1), filename, ln) { declarative = declaration, special = special, size = size };
            }
            else if (value == "null" || value == null)
            {
                prop = new BYONDNull(filename, ln) { type = new BYONDType(typepath), declarative = declaration, special = special, size = size };
            }
            else if (value != null && float.TryParse(value,out parseval))
            {
                prop = new BYONDNumber(parseval, filename, ln) { declarative = declaration, special = special, size = size };
            }
            else
            {
                prop = new BYONDUnhandledValue(value, filename, ln) { type = new BYONDType(typepath), declarative = declaration, special = special, size = size };
            }
        }

        private void MakeTree()
        {
            // TODO: Rework this to use small structs instead of actual Atoms.
            log.Info("Generating Tree...");
            this.Tree = new Atom("/");
            using (TextWriter f = File.CreateText("objtree.txt"))
            {
                List<string> keys = this.Atoms.Keys.ToList();
                keys.Sort();
                foreach (string key in keys)
                {
                    f.WriteLine(key);
                    Atom atom = this.Atoms[key];
                    cpath.Clear();
                    Atom cNode = this.Tree;
                    BYONDType fullpath = atom.Type;//this.SplitPath(atom.Type.ToString()).ToList();
                    string[] truncatedPath = fullpath.GetPathSegments();
                    foreach (string path_item in truncatedPath)
                    {
                        cpath.Add(path_item);
                        string cpath_str = "/" + ("/".join(cpath));
                        // if path_item == "var":
                        //    if path_item not in cNode.properties:
                        //        cNode.properties[fullpath[-1]]="???"
                        if (!cNode.Children.ContainsKey(path_item))
                        {
                            if (!Atoms.ContainsKey(cpath_str))
                                cNode.Children[path_item] = this.Atoms[cpath_str];
                            else
                            {
                                if (path_item.Contains('('))
                                    cNode.Children[path_item] = new Proc(cpath_str, new string[0], atom.Filename, atom.Line);
                                else
                                    cNode.Children[path_item] = new Atom(cpath_str, atom.Filename, atom.Line);
                            }
                            cNode.Children[path_item].Parent = cNode;
                            string parent_type = cNode.Children[path_item].GetProperty<string>("parent_type", null);
                            if (parent_type != null)
                            {
                                log.InfoFormat(" - Parent of {0} forced to be {1}", cNode.Children[path_item].Type.ToString(), parent_type);
                                cNode.Children[path_item].Parent = this.Atoms[parent_type];
                            }
                        }
                        cNode = cNode.Children[path_item];
                    }
                }
            }
            this.Tree.InheritProperties();
            log.InfoFormat("Processed {0} atoms.", this.Atoms.Count);
            // this.Atoms = {}
        }

        public Atom GetAtom(string path)
        {
            if (Atoms.ContainsKey(path))
                return this.Atoms[path];

            cpath.Clear();
            Atom cNode = this.Tree;
            string[] fullpath = path.Split('/');
            string[] truncatedPath = fullpath.Skip(1).ToArray();
            foreach (string path_item in truncatedPath)
            {
                cpath.Add(path_item);
                if (!cNode.Children.ContainsKey(path_item))
                {
                    log.ErrorFormat("Unable to find {0} (lost at {1})", path, cNode.Type.ToString());
                    log.Error(", ".join(cNode.Children.Keys));
                    return null;
                }
                cNode = cNode.Children[path_item];
            }
            // print("Found {0}!".format(path))
            this.Atoms[path] = cNode;
            return cNode;
        }


        private string PreprocessLine(string line)
        {
            foreach (KeyValuePair<string, BYONDValue> kvp in Defines)
            {
                string key = kvp.Key;
                BYONDValue value = kvp.Value;
                if (line.Contains(key))
                {
                    if (!defineMatchers.ContainsKey(key))
                        this.defineMatchers[key] = new Regex(@"\b" + key + @"\b");

                    string newline = this.defineMatchers[key].Replace(value._value.ToString(), line);
                    if (newline != line)
                    {
                        /*
                        if(filename.EndsWith("pipes.dm")) {
                            log.DebugFormat("OLD: {0}",line);
                            log.DebugFormat("PPD: {0}",newline);
                        }
                        */
                        line = newline;
                    }
                }
            }
            return line;
        }
    }
}
