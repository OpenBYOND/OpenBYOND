using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using log4net;

namespace OpenBYOND
{
    public struct IconFrame
    {
        public Rectangle rect;
        public Direction dir;
        public int frame;
    }

    public class IconState
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(IconState));

        /// <summary>
        /// Array of frames.  Index is math.
        /// </summary>
        public IconFrame[] Frames;

        public string Name = "";
        public uint NumDirections = 1;
        public uint NumFrames = 1;
        public bool Movement = false;

        public static string CollectionKey(string Name, bool Movement)
        {
            string tags = "";
            if (Movement)
                tags += 'M';

            if (tags.Length > 0)
                return Name + '\t' + tags;
            else
                return Name;
        }

        public void Initialize()
        {
            // Array init.
            Frames = new IconFrame[NumFrames * NumDirections];
        }

        public void SetFrame(uint frame, Direction dir, IconFrame f)
        {
            Frames[GetFrameIndex(frame, dir)] = f;
        }

        public IconFrame GetFrame(uint frame, Direction dir)
        {
            return Frames[GetFrameIndex(frame, dir)];
        }

        public uint GetFrameIndex(uint frame, Direction dir, bool NoLengthChecks = false)
        {
            // Either 1, 4, or 8 directions, so validate.
            uint _dir = 0;
            if (NumDirections == 4 || NumDirections == 8)
            {
                // And convert Direction to dirindex.
                _dir = (uint)DirUtils.GetDirIndex(dir);
                if (NumDirections == 4 && _dir > 3)
                    _dir = 0;
            }
            //log.DebugFormat("{0} directions, _dir={1}", NumDirections, _dir);
            uint _frame = _dir + (frame * NumDirections);
            if (!NoLengthChecks && _frame > Frames.Length - 1)
            {
                log.WarnFormat("Only {0} icons in state, args {{dir:{1}, frame:{2}}}", Frames.Length, DirUtils.GetNameFromDir(dir), frame);
            }
            return _frame;
        }

        public uint GetTotalNumIcons()
        {
            return NumDirections * NumFrames;
        }

        internal string GetCollKey()
        {
            return IconState.CollectionKey(Name, Movement);
        }
    }
    public class DMI
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DMI));

        public Dictionary<string, IconState> states = new Dictionary<string, IconState>();

        private int height;
        private int width;
        public int iconWidth = 0;
        public int iconHeight = 0;
        private int columns = 0;
        private int rows = 0;
        private int frameNumber = 0;
        public string FileName;
        private Texture2D texture;


        public int Height
        {
            get { return height; }
        }

        public int Width
        {
            get { return width; }
        }

        public int Frames
        {
            get { return frameNumber; }
        }

        public int Columns
        {
            get { return columns; }
        }

        public int Rows
        {
            get { return rows; }
        }

        public DMI(string icon)
        {
            Load(icon);
        }

        void loadFramesForState(ref int x, ref int y, ref int total, ref IconState currentState)
        {
            // Init array size.
            currentState.Initialize();

            // Grab the icons we need, operating like a typewriter.
            // Loop X times, incrementing i from 0 to <X, where X = GetTotalNumIcons()
            for (int i = 0; i < currentState.GetTotalNumIcons(); i++)
            {
                // Make a new frame.
                IconFrame frame = new IconFrame();

                // Icon #
                frame.frame = i;

                // Store our position and rectangle.
                frame.rect = new Rectangle(x * iconWidth, y * iconHeight, iconWidth, iconHeight);
                log.DebugFormat(" Frame {0}: {1}, {2}", i, x, y);

                // Store frame in the state.
                currentState.Frames[i] = frame;

                // Move over 1 icon X position.
                x++;

                // Bump total # of frames loaded.
                total++;

                // If we're over the number of columns, go to the next line.
                if (x >= columns)
                {
                    x = 0;
                    y++;
                    log.DebugFormat("NEXT LINE: x={0}, y={1}", x, y);
                }
            }
            // Debug spam.
            log.DebugFormat("Loaded {0} icons for state {1}.", currentState.Frames.Length, currentState.Name);

            // Store state.
            states[currentState.GetCollKey()] = currentState;
        }

        public void Load(string icon)
        {
            FileName = Path.GetFullPath(icon);

            string ztxt = GetHeader(icon);

            ////////////////////////////
            // Current state of things.
            IconState currentState = null; // IconState being operated on

            // Current X/Y position (ICON, not pixel)
            int x = 0;
            int y = 0;

            // Total icons made
            int total = 0;

            // Any unknown keys we run into.
            List<string> UnknownKeys = new List<string>();

            // Breaking up into lines.
            string[] lines = ztxt.Split('\n');

            // Loop over each line
            foreach (string line in lines)
            {
                // Skip comments and empty lines.
                if (String.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("#")) continue;

                // Break each key = value into chunks of (key,value)
                String[] chunks = line.Split('=');

                // Trim.
                chunks = chunks.Select(chunk => { return chunk.Trim(); }).ToArray();

                // Separate into key and val.
                string key = chunks[0];
                string val = chunks[1];

                // Spam debug log.
                log.DebugFormat("{0} = {1}", key, val);

                switch (key)
                {
                    case "width":
                        iconWidth = int.Parse(val);
                        columns = (int)Math.Ceiling((double)width / (double)iconWidth);
                        log.DebugFormat("Columns: {0}/{1} = {2}", width, iconWidth, columns);
                        break;
                    case "height":
                        iconHeight = int.Parse(val);
                        rows = height / iconHeight;
                        log.DebugFormat("Rows: {0}/{1} = {2}", height, iconHeight, rows);
                        break;
                    case "state":
                        // Already have a state?  Let's wrap it up and store it.
                        if (currentState != null)
                        {
                            loadFramesForState(ref x, ref y, ref total, ref currentState);
                        }

                        // New state.
                        currentState = new IconState();
                        currentState.Name = val.Substring(1, val.Length - 2);
                        break;
                    case "dirs":
                        currentState.NumDirections = uint.Parse(val);
                        break;
                    case "movement":
                        currentState.Movement = (int.Parse(val) == 1);
                        break;
                    case "frames":
                        currentState.NumFrames = uint.Parse(val);
                        break;
                    default:
                        if (!UnknownKeys.Contains(key))
                            UnknownKeys.Add(key);
                        break;
                }

            }

            if (currentState != null)
            {
                loadFramesForState(ref x, ref y, ref total, ref currentState);
            }

            if (UnknownKeys.Count > 0)
            {
                log.WarnFormat("DMI ERROR: Unhandled keys: {0}", string.Join(", ", UnknownKeys));
            }

            log.DebugFormat("Sprites per row: {0}, Rows of sprites: {1}, Total frames: {2}", columns, rows, total);

        }

        public string GetHeader(string icon)
        {
            string ztxt = string.Empty;

            PngReader pngr = FileHelper.CreatePngReader(FileName);
            width = pngr.ImgInfo.Cols;
            height = pngr.ImgInfo.Rows;
            ChunksList clist = pngr.GetChunksList();

            /*The File should only have one zTxt, this chunk stores our DMI information such as
             *sprite width, height, iconstates, directionals and frames among other things.
             */
            foreach (PngChunkZTXT desc in clist.GetById("zTXt"))
            {
                ztxt = desc.GetVal();
                break;
            }
            pngr.ShouldCloseStream = true;
            pngr.End();

            return ztxt;
        }

        public IconState GetIconState(string icon_state, bool movement = false)
        {
            return states[IconState.CollectionKey(icon_state, movement)];
        }

        public SpriteBatch GetSpriteBatch(string state, Game game, Vector2 offset, Direction dir = Direction.SOUTH, uint frame = 0, bool movement = false)
        {
            IconFrame f = GetIconState(state, movement).GetFrame(frame, dir);

            using (SpriteBatch sb = new SpriteBatch(game.GraphicsDevice))
            {
                //Console.WriteLine(f.rect.X + " " + f.rect.Y);
                //game.GraphicsDevice.Clear(Color.White);
                sb.Begin();
                if (texture == null)
                {
                    texture = Texture2D.FromStream(game.GraphicsDevice, new FileStream(FileName, FileMode.Open));
                    log.DebugFormat("Loaded Texture2D {0}", FileName);
                }
                sb.Draw(texture, offset, f.rect, Color.White);
                sb.End();
                return sb;
            }
            
        }

        public void DrawSpriteBatch(SpriteBatch sb, string state, Game game, Vector2 offset, Direction dir = Direction.SOUTH, uint frame = 0, bool movement = false)
        {
            IconFrame f = GetIconState(state, movement).GetFrame(frame, dir);
            if (texture == null)
            {
                texture = Texture2D.FromStream(game.GraphicsDevice, new FileStream(FileName, FileMode.Open));
                log.DebugFormat("Loaded Texture2D {0}", FileName);
            }
            sb.Draw(texture, offset, f.rect, Color.White);
        }
    }
}
