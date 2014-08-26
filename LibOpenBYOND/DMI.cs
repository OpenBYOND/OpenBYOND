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

namespace OpenBYOND
{
    public class DMI
    {
        public Dictionary<String, String> metadata = new Dictionary<String, String>();
        private string[] expectedkeys = new string[]{"sheight","swidth","dirs","frames"};
        private int height;
        private int width;

        public int Height
        {
            get { return height; }
        }

        public int Width
        {
            get { return width; }
        }


        public DMI(string icon, string icon_state)
        {
            string path = Directory.GetCurrentDirectory() + "//" + @icon;
            string value = string.Empty;
            try
            {
                PngReader pngr = FileHelper.CreatePngReader(path);
                width = pngr.ImgInfo.Rows;
                height = pngr.ImgInfo.Cols;
                ChunksList clist = pngr.GetChunksList();
                /*The File should only have one zTxt, this chunk stores our DMI information such as
                 *sprite width, height, iconstates, directionals and frames among other things.
                 */
                foreach (PngChunkZTXT desc in clist.GetById("zTXt"))
                {
                    value = desc.GetVal();
                    break;
                }
                pngr.ShouldCloseStream = true;
                pngr.End();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return;
            }
            string[] split = value.Split('\n');
            bool found = false;
            //Looping through each of the lines to find the sprite width, height and our icon_state starting and ending points.
            foreach (string s in split)
            {
                var S = s.Trim();
                if (S.StartsWith("width"))
                {
                    var sub = s.Substring(9);
                    metadata["swidth"] = sub;
                }
                if (S.StartsWith("height"))
                {
                    var sub = s.Substring(10);
                    metadata["sheight"] = sub;
                }
                if (S.StartsWith("state"))
                {
                    if (found)
                    {
                        break;
                    }
                    string sub = S.Substring(9);
                    string output = sub.Split(new char[] {'"', '"'})[0];
                    if (icon_state.Equals(output))
                    {
                        found = true;
                        continue;
                    }

                }
                else if(found)
                {
                    string[] splittwo = S.Split(' ');
                    if(splittwo.Length == 3)
                        metadata[splittwo[0]] = splittwo[2];
                }

            }

            string missingKey = string.Empty;
            foreach (string s in expectedkeys)
            {
                if (!metadata.ContainsKey(s))
                {
                    missingKey += (s + ", ");
                }
            }
            missingKey = missingKey.Trim(new char[]{' ', ','});
            if (!String.IsNullOrEmpty(missingKey))
            {
                throw new KeyNotFoundException("DMI ERROR: Missing one or more keys. Missing: " + missingKey);
            }
        }
    }
}
