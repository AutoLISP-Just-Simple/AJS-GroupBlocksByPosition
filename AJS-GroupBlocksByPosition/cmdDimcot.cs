using System.Data;
using System.Linq;
using AJS_GroupBlocksByPosition.AJS_Clone;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(AJS_GroupBlocksByPosition.MyCommands))]
namespace AJS_GroupBlocksByPosition
{
    public partial class MyCommands
    {
        [CommandMethod("GroupBlocksByPosition", CommandFlags.Modal | CommandFlags.Redraw | CommandFlags.UsePickSet)]
        public static void cmd_GroupBlocksByPosition()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            if (doc == null) return;
            Editor ed = doc.Editor;
            ed.WriteMessage("Hello, this is your first command.");

            var ids = ed.Select(0, "*INSERT");
            if (ids.Count == 0) return;

            var dbs = ids.Clone<Entity>();

            var brs = dbs.Where(x => x.GetType().Name.ToUpper().Contains("BLOCK"))
                .Cast<BlockReference>()
                .OrderBy(x => x.Position.X)
                .ThenBy(x => x.Position.Y)
                .ToList();

            double hwtb = 0.5d * (brs.Average(x => x.Bounds.Value.Width()) + brs.Average(x => x.Bounds.Value.Height()));

            var brss = GroupByDistanceExts.GroupByDist(ref brs, hwtb * 3.0d);
            foreach (var bs in brss)
            {
                var ext = new Extents3d();

                foreach (var br in bs)
                    ext.AddExtents(br.Bounds.Value);

                

                var pts = new Point3dCollection() { ext.MinPoint, new Point3d(ext.MinPoint.X, ext.MaxPoint.Y, 0), ext.MaxPoint, new Point3d(ext.MaxPoint.X, ext.MinPoint.Y, 0), ext.MinPoint };

                var pl = pts.ToPolyline();
                pl.ColorIndex = 1;
                pl.ToDatabase();
            }
        }
    }
}