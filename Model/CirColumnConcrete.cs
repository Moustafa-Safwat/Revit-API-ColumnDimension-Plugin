using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColumnDimensionRevit.Model
{
    public class CirColumnConcrete
    {
        #region Automatic Property
        public XYZ CenterPoint { get; set; }
        public Reference CenterPointReference { get; set; }
        public MyGrid GridInColumn { get; set; }
        #endregion

        #region Constructor
        public CirColumnConcrete()
        {
        }
        #endregion

        #region Methods
        public static bool IsCircularColumn(Element element)
        {
            Options opt = new Options();
            opt.ComputeReferences = true;
            foreach (var internalItem in element.get_Geometry(opt))
            {
                foreach (Solid item in (internalItem as GeometryInstance).SymbolGeometry)
                {
                    foreach (Edge edge in item.Edges)
                    {
                        if (edge.AsCurve() as Arc!=null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static void GetColumnCenterPoint(Element element, CirColumnConcrete item)
        {
            item.CenterPoint = (element.Location as LocationPoint).Point;
        }
        public static void GetColumnAxisReference(CirColumnConcrete item, List<MyGrid> listOfGridIntersection)
        {
            List<double> len = new List<double>();
            foreach (var item2 in listOfGridIntersection)
            {
                len.Add(item.CenterPoint.DistanceTo(item2.PointOfIntersection));
            }
            item.GridInColumn = listOfGridIntersection[len.IndexOf(len.Min())];
        }
        public static void GetColumnCenterPointReference(Element element, CirColumnConcrete item)
        {
            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.DetailLevel = ViewDetailLevel.Fine;
            opt.IncludeNonVisibleObjects = true;
            foreach (var internalItem in element.get_Geometry(opt))
            {

                item.CenterPointReference = (internalItem as Point).Reference;
                break;
            }
        }
        public static void GetCircularColumnDimension(List<CirColumnConcrete> listOfColumnSteelObject, Document doc)
        {
            foreach (CirColumnConcrete element in listOfColumnSteelObject)
            {

                #region First Dimension
                ReferenceArray referenceArray1 = new ReferenceArray();
                referenceArray1.Append(element.CenterPointReference);
                referenceArray1.Append(element.GridInColumn.Reference1);
                Line l1;
                XYZ pc = element.CenterPoint;
                //double distval = 0.5;
                if (Math.Round((element.GridInColumn.Element1 as Line).Direction.X, 1) == 0)
                {
                    XYZ p2 = new XYZ(pc.X + 1, pc.Y, 0);
                    l1 = Line.CreateBound(pc, p2);
                }
                else
                {
                    XYZ p2 = new XYZ(pc.X, pc.Y + 1, 0);
                    l1 = Line.CreateBound(pc, p2);
                }
                Dimension dim1 = doc.Create.NewDimension(doc.ActiveView, l1, referenceArray1);
                if (dim1.ValueString == "0")
                {
                    IList<ElementId> elementIds = new List<ElementId>();
                    elementIds.Add(dim1.Id);
                    doc.ActiveView.HideElements(elementIds);
                }
                #endregion

                #region Secound Dimension
                ReferenceArray referenceArray2 = new ReferenceArray();
                referenceArray2.Append(element.CenterPointReference);
                referenceArray2.Append(element.GridInColumn.Reference2);
                Line l2;
                if (Math.Round((element.GridInColumn.Element2 as Line).Direction.X, 1) == 0)
                {
                    XYZ p3 = new XYZ(pc.X + 1, pc.Y, 0);
                    l2 = Line.CreateBound(pc, p3);
                }
                else
                {

                    XYZ p3 = new XYZ(pc.X, pc.Y + 1, 0);
                    l2 = Line.CreateBound(pc, p3);
                }
                Dimension dim2 = doc.Create.NewDimension(doc.ActiveView, l2, referenceArray2);
                if (dim2.ValueString == "0")
                {
                    IList<ElementId> elementIds = new List<ElementId>();
                    elementIds.Add(dim2.Id);
                    doc.ActiveView.HideElements(elementIds);
                }
                #endregion

            }
        }
        #endregion

    }
}
