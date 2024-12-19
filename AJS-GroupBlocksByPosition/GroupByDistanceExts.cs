using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

namespace AJS_GroupBlocksByPosition
{
    public static partial class GroupByDistanceExts
    {
        public static List<List<BlockReference>> GroupByDist(ref List<BlockReference> brs, double distance)
        {
            var brss = new List<List<BlockReference>>();

            var bsdrawn = new List<BlockReference>();
            foreach (var br in brs)
            {
                if (bsdrawn.Contains(br))
                    continue;

                var matchbrs = new List<BlockReference>() { br };

                var bs = brs.Except(bsdrawn).Where(x => matchbrs.Any(x1 => x.Position.DistanceTo(x1.Position) < distance)).ToList();
                while (bs.Count > 0)
                {
                    matchbrs.AddRange(bs);
                    bsdrawn.AddRange(bs);

                    bs = brs.Except(bsdrawn).Where(x => matchbrs.Any(x1 => x.Position.DistanceTo(x1.Position) < distance)).ToList();
                }

                brss.Add(matchbrs);
            }

            brs = brs.Except(bsdrawn).ToList();

            return brss;
        }

        public static double Width(this Extents3d ext)
        {
            return ext.MaxPoint.X - ext.MinPoint.X;
        }

        public static double Height(this Extents3d ext)
        {
            return ext.MaxPoint.Y - ext.MinPoint.Y;
        }
    }

}