using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework;

namespace OpenBYOND
{
    /// <summary>
    /// Singleton.  Used for caching and simplifying DMI accesses.
    /// </summary>
    public class DMIManager
    {
        private Dictionary<string, DMI> dmis = new Dictionary<string, DMI>();

        private static DMIManager instance;

        public static DMIManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new DMIManager();
                return instance;
            }
        }

        public static DMI GetDMI(string file)
        {
            return Instance._GetDMI(file);
        }

        private DMI _GetDMI(string file)
        {
            file = Path.GetFullPath(file);
            Instance.Load(file);
            return dmis[file];
        }

        public static void Reload()
        {
            Instance.dmis.Clear();
        }

        public static SpriteBatch GetSpriteBatch(Game game, string icon, string state, Direction dir = Direction.SOUTH, uint frame = 0, bool movement = false)
        {
            DMI dmi = GetDMI(icon);
            return dmi.GetSpriteBatch(state, game, dir, frame, movement);
        }

        public static void Preload(string file)
        {
            file = Path.GetFullPath(file);
            Instance.Load(file);
        }

        private void Load(string file)
        {
            if (!dmis.ContainsKey(file))
                dmis[file] = new DMI(file);
        }
    }
}
