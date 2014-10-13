using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenBYOND.VM;
using log4net;
using OpenBYOND.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OpenBYOND.World.Format
{
    public class DMMLoader : WorldLoader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DMMLoader));

        internal override List<string> GetExtensions()
        {
            return new List<string>(){ 
                "dmm"
            };
        }
        private enum DMMSection
        {
            AtomList,
            Map
        }

        private Dictionary<string, Tile> Tiles = new Dictionary<string, Tile>();

        private static CharMatcher ATOM_NAME_CHARS;
        private static CharMatcher WHITESPACE;
        private static Regex REGEX_FLOAT;
        private static Regex REGEX_INTEGER;

        // STATIC CONSTRUCTOR
        static DMMLoader()
        {
            // A-Za-z0-9_/
            ATOM_NAME_CHARS = CharRange.Build('A', 'Z') | CharRange.Build('a', 'z') | CharRange.Build('0', '9') | "_/";
            WHITESPACE = new ConstCharMatcher("\r\n");
            REGEX_FLOAT = new Regex(@"^[0-9\.]+([Ee][\+\-][0-9]+)?$");
            REGEX_INTEGER = new Regex(@"^\d+$");
        }

        DMMSection section = DMMSection.AtomList;
        private World world;
        int idlen;
        public override bool Load(World w, string filename)
        {
            section = DMMSection.AtomList;
            world = w;
            using (TextReader rdr = File.OpenText(filename))
            {
                try
                {
                    while (rdr.Peek() > -1)
                    {
                        switch (section)
                        {
                            case DMMSection.AtomList: LoadAtom(rdr); break;
                            case DMMSection.Map: LoadMap(rdr); break;
                        }
                    }
                }
                catch (EndOfStreamException)
                {
                    // Do nothing (EOF)
                }
            }
            return true;
        }

        private void LoadMap(TextReader rdr)
        {
            Console.WriteLine("Reading z-level #{0}", world.Levels.Count);

            // (1,1,1) = {"
            ReaderUtils.ReadUntil(rdr, '"');

            // This is awful.
            // In order to load up our array of tiles, we need to know how big the z-level is.
            // Unfortunately, the map does not fucking tell us what it is immediately, so we
            //  have to figure it out on our own.
            // Which is where this fucking monstrosity comes in.
            List<List<string>> idmap = new List<List<string>>();
            uint size_x = 0;
            uint size_y = 0;
            int x = 0;
            int y = 0;
            string idbuf;
            while (true)
            {
                idmap.Add(new List<string>());
                string line = rdr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                //Console.WriteLine(line);
                if (line.StartsWith("\"}"))
                {
                    break;
                }
                line = line.Trim();
                size_x = (uint)(line.Length / idlen);
                for (x = 0; x < line.Length; x += idlen)
                {
                    idbuf = line.Substring(x, idlen);
                    idmap[y].Add(idbuf);
                }
                if (idmap[y].Count == 0)
                {
                    throw new InvalidDataException(string.Format("idmap[{0}] is empty.", y));
                }
                y++;
            }
            size_y = (uint)y;

            // And now we build the god damned z-level.
            ZLevel zlevel = world.CreateZLevel(size_y, size_x);
            for (y = 0; y < size_y; y++)
                for (x = 0; x < size_x; x++)
                    zlevel.Tiles[x, y] = Tiles[idmap[y][x]].CopyNew((uint)x, (uint)y, zlevel.Z);
            Console.WriteLine(" -> Loaded {0}x{1} tilemap.", size_x, size_y);
        }

        private void LoadAtom(TextReader rdr)
        {
            // "aai" = (/obj/structure/sign/securearea{desc = "A warning sign which reads 'HIGH VOLTAGE'"; icon_state = "shock"; name = "HIGH VOLTAGE"; pixel_y = -32},/turf/space,/area)
            // Move to ID.

            ReaderUtils.ReadUntil(rdr, '"');

            Tile t = new Tile();

            // Get ID contents
            t.origID = ReaderUtils.ReadUntil(rdr, '"');
            idlen = t.origID.Length;
            //log.InfoFormat("Reading tile {0}", t.origID);

            ReaderUtils.ReadUntil(rdr, '(');

            uint atomID = 0; // Which atom we're currently on IN THIS TILEDEF.

            // Read atomdefs.
            bool finishedReadingAtoms = false;
            while (!finishedReadingAtoms)
            {
                char nextChar = ReaderUtils.GetNextChar(rdr);
                if (nextChar == ')')
                    break;

                ReaderUtils.ReadUntil(rdr, '/');
                
                string atomType = ReaderUtils.ReadCharRange(rdr, ATOM_NAME_CHARS);
                if (string.IsNullOrWhiteSpace(atomType))
                    throw new InvalidDataException(string.Format("atomType is \"{0}\"", atomType));

                Atom a = new Atom(atomType, false);

                nextChar = ReaderUtils.GetNextChar(rdr);
                switch (nextChar)
                {
                    case '{':
                        // We're in a propertygroup.  Read the properties.
                        rdr.Read();
                        LoadPropertyGroup(rdr, a);
                        //rdr.Read();
                        break;
                    case ',':
                        rdr.Read();
                        break;
                    case ')':
                        finishedReadingAtoms = true;
                        rdr.Read();
                        break;
                    default:
                        //log.FatalFormat(
                        Console.WriteLine("UNKNOWN CHARACTER {0} IN TILEDEF {1}, ATOMDEF #{2}. EXPECTING: '{{),'", nextChar, t.origID, atomID);
                        break;
                }
                t.Atoms.Add(a);
                atomID++;
            }

            Tiles[t.origID] = t;

            string read = ReaderUtils.ReadCharRange(rdr, WHITESPACE);
            int numLineReturns = read.Count((char c) => { 
                return c == '\n'; 
            });
            //Console.WriteLine("{1}",read,numLineReturns);
            if (numLineReturns > 1)
            {
                section = DMMSection.Map;
                Console.WriteLine("Finished reading Tile List section:  {0} tiles loaded.", Tiles.Count);
            }
        }

        private void LoadPropertyGroup(TextReader rdr, Atom a)
        {
            while (true)
            {
                string propName = ReaderUtils.ReadUntil(rdr, '=').Trim(); // blah =
                char lc;
                string propValue = ReaderUtils.ReadScriptUntil(rdr, "\"'", "\\", ";}", out lc).Trim();

                if (propValue.Contains('"'))
                    a.SetProperty<string>(propName, propValue.Substring(1, propValue.Length - 2));
                else if (REGEX_FLOAT.IsMatch(propValue))
                    a.SetProperty<double>(propName, double.Parse(propValue,System.Globalization.NumberStyles.Any));
                else if (propValue == "null")
                    a.Properties[propName]=new BYONDNull();
                else if (REGEX_INTEGER.IsMatch(propValue))
                    a.SetProperty<int>(propName, int.Parse(propValue));
                else // lolidk
                    a.Properties[propName] = new BYONDUnhandledValue(propValue);

                if (lc == '}') 
                    return;
            }
        }

    }
}
