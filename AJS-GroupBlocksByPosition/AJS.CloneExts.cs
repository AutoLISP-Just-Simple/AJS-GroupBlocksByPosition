using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AJS_GroupBlocksByPosition.AJS_Clone
{
    internal static class AJS
    {
        public static ObjectId ToDatabase(this DBObject o)
        {
            if (o == null) return ObjectId.Null;
            ObjectId id = ObjectId.Null;
            Database db = HostApplicationServices.WorkingDatabase;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                Entity e = o as Entity;
                if (e != null)
                {
                    try
                    {
                        id = btr.AppendEntity(e);
                        tr.AddNewlyCreatedDBObject(e, true);
                    }
                    catch { }
                }

                tr.Commit();
            }
            return id;
        }

        public static Polyline ToPolyline(this Point3dCollection pts)
        {
            Polyline pl = null;
            if (pts.Count > 1)
            {
                pl = new Polyline(pts.Count);
                int i = 0;
                foreach (Point3d p in pts)
                {
                    pl.AddVertexAt(i++, new Point2d(p.X, p.Y), 0, 0, 0);
                }
            }
            return pl;
        }
        public static List<ObjectId> Select(this Editor ed, params object[] pairs)
        {
            PromptSelectionResult psr = pairs == null || pairs.Length < 2 ? ed.GetSelection() : ed.GetSelection(pairs.ToSelectionFilter());

            List<ObjectId> IDS = new List<ObjectId>();
            if (psr.Status == PromptStatus.OK)
            {
                IDS = psr.Value.GetObjectIds().ToList();
            }
            return IDS;
        }
        private static SelectionFilter ToSelectionFilter(this object[] pairs)
        {
            var tps = new List<TypedValue>();
            for (int i = 0; i < pairs.Length - 1; i += 2)
            {
                if ((int)pairs[i] == -3)
                {
                    tps.Add(new TypedValue((int)pairs[i]));
                    i--;
                }
                else
                    tps.Add(new TypedValue((int)pairs[i], pairs[i + 1]));
            }
            return new SelectionFilter(tps.ToArray());
        }

        public static DBObject Clone(this ObjectId id)
        {
            DBObject d = null;
            using (var tr = id.Database.TransactionManager.StartTransaction())
            {
                var db = tr.GetObject(id, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                d = db.Clone() as DBObject;
                tr.Commit();
            }
            return d;
        }
        public static T Clone<T>(this ObjectId id) where T : DBObject
        {
            T d = null;
            using (var tr = id.Database.TransactionManager.StartTransaction())
            {
                var db = tr.GetObject(id, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                d = db.Clone() as T;
                tr.Commit();
            }
            return d;
        }

        public static List<T> Clone<T>(this IEnumerable<ObjectId> ids, Func<T, bool> predicate = null) where T : DBObject
        {
            if (ids.Count() == 0)
                return new List<T>();
            var dbs = new List<T>();
            using (var tr = ids.FirstOrDefault().Database.TransactionManager.StartTransaction())
            {
                dbs = ids.Clone(tr, predicate);
                tr.Commit();
            }
            return predicate is null ? dbs : dbs.Where(predicate).ToList();
        }

        public static List<T> Clone<T>(this IEnumerable<ObjectId> ids, Transaction tr, Func<T, bool> predicate = null) where T : DBObject
        {
            if (ids.Count() == 0)
                return new List<T>();

            var dbs = new List<T>();

            foreach (var id in ids)
            {
                T tx = tr.GetObject(id, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead) as T;
                if (tx != null)
                    dbs.Add(tx);
            }
            return predicate is null ? dbs : dbs.Where(predicate).ToList();
        }
    }
}