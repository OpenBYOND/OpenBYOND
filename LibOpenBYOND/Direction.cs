using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBYOND
{
    [Flags]
    public enum Direction : uint
    {
        NORTH = 1,
        SOUTH = 2,
        EAST = 4,
        WEST = 8,

        SOUTHEAST = SOUTH | EAST,
        SOUTHWEST = SOUTH | WEST,
        NORTHEAST = NORTH | EAST,
        NORTHWEST = NORTH | WEST
    }

    public class DirUtils
    {
        // For DMIs.
        public static Direction[] IMAGE_INDICES = new Direction[]
        {
            Direction.SOUTH,
            Direction.NORTH,
            Direction.EAST,
            Direction.WEST,
            Direction.SOUTHEAST,
            Direction.SOUTHWEST,
            Direction.NORTHEAST,
            Direction.NORTHWEST
        };

        public static Direction GetDirFromString(string dirname)
        {
            return (Direction)Enum.Parse(typeof(Direction), dirname);
        }

        public static string GetNameFromDir(Direction dir)
        {
            return Enum.GetName(dir.GetType(), dir);
        }

        public static int GetDirIndex(Direction dir)
        {
            return Array.IndexOf(IMAGE_INDICES, dir);
        }
    }
}
