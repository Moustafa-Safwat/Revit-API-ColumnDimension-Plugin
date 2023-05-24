using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ColumnDimensionRevit.Model
{
    public class RecColumnConcrete
    {
        #region Automatic Property
        public XYZ CenterPoint { get; set; }
        public IList<Reference> EdgeReference { get; set; }
        public IList<XYZ> EdgePoints { get; set; }
        public MyGrid GridInColumn { get; set; }
        public double Rotation { get; set; }
        #endregion

        #region Constructor
        public RecColumnConcrete()
        {

        }
        #endregion

        #region Methods
        public static void GetColumnEdgeReference(Element element, RecColumnConcrete item)
        {
            int i = 0;
            item.EdgeReference = new List<Reference>();
            Options opt = new Options();
            opt.ComputeReferences = true;
            GeometryElement geometryElement = element.get_Geometry(opt);
            foreach (GeometryInstance geometryInstance in geometryElement)
            {
                if (i == 0)
                {
                    foreach (Solid solid in geometryInstance.SymbolGeometry)
                    {
                        foreach (Edge edge in solid.Edges)
                        {
                            if ((edge.AsCurve() as Line).Direction.X == 0 && (edge.AsCurve() as Line).Direction.Y == 0)
                            {

                                item.EdgeReference.Add(edge.Reference);
                            }
                        }
                    }
                }
                i++;
            }
        }
        public static void GetColumnCornerPoints(Element element, RecColumnConcrete item)
        {
            int i = 0;
            item.EdgePoints = new List<XYZ>();
            Options opt = new Options();
            opt.ComputeReferences = true;
            GeometryElement geometryElement = element.get_Geometry(opt);
            foreach (GeometryInstance geometryInstance in geometryElement)
            {
                if (i == 0)
                {
                    foreach (Solid solid in geometryInstance.GetInstanceGeometry())
                    {
                        foreach (Edge edge in solid.Edges)
                        {
                            if ((edge.AsCurve() as Line).Direction.X == 0 && (edge.AsCurve() as Line).Direction.Y == 0)
                            {

                                item.EdgePoints.Add((edge.AsCurve() as Line).Origin);
                            }
                        }
                    }
                }
                i++;
            }
        }
        public static void GetColumnCenterPoint(Element element, RecColumnConcrete item)
        {
            item.CenterPoint = (element.Location as LocationPoint).Point;
        }
        public static void GetColumnRotation(Element element, RecColumnConcrete item)
        {
            item.Rotation = (element.Location as LocationPoint).Rotation;
        }
        public static void GetColumnAxisReference(RecColumnConcrete item, List<MyGrid> listOfGridIntersection)
        {
            List<double> len = new List<double>();
            foreach (var item2 in listOfGridIntersection)
            {
                len.Add(item.CenterPoint.DistanceTo(item2.PointOfIntersection));
            }
            item.GridInColumn = listOfGridIntersection[len.IndexOf(len.Min())];
        }
        public static void CreateShort1ColumnDimension(List<RecColumnConcrete> listOfColumnObject, Document doc)
        {
            foreach (RecColumnConcrete element in listOfColumnObject)
            {
                List<double> length = new List<double>();
                foreach (XYZ item2 in element.EdgePoints)
                {
                    length.Add(item2.DistanceTo(element.GridInColumn.PointOfIntersection));
                }
                int index = length.IndexOf(length.Min());

                #region Create First Dimension
                ReferenceArray referenceArray1 = new ReferenceArray();
                referenceArray1.Append(element.EdgeReference[index]);
                referenceArray1.Append(element.GridInColumn.Reference1);
                Line l1;
                XYZ pE = element.EdgePoints[index];
                XYZ pI = element.GridInColumn.PointOfIntersection;
                double lE = pE.DistanceTo(element.CenterPoint);
                double lI = pI.DistanceTo(element.CenterPoint);
                double distval = 0.5;
                if (Math.Round((element.GridInColumn.Element1 as Line).Direction.X, 1) == 0)
                {
                    XYZ p2 = new XYZ(pE.X + 1 + distval, pE.Y - distval, 0);
                    if ((pE.X < pI.X && pE.Y > pI.Y && lE > lI) || (pE.X > pI.X && pE.Y > pI.Y && lE > lI) || (pE.X < pI.X && pE.Y > pI.Y && lE < lI) || (pE.X > pI.X && pE.Y > pI.Y && lE < lI))
                    {
                        p2 = new XYZ(pE.X, pE.Y + distval, 0);
                    }
                    XYZ p1 = new XYZ(p2.X + 1, p2.Y, 0);
                    l1 = Line.CreateBound(p1, p2);
                }
                else
                {
                    XYZ p2 = new XYZ(pE.X + distval, pE.Y + 1, 0);
                    if ((pE.X < pI.X && pE.Y > pI.Y && lE > lI) || (pE.X < pI.X && pE.Y < pI.Y && lE > lI) || (pE.X > pI.X && pE.Y > pI.Y && lE < lI) || (pE.X > pI.X && pE.Y < pI.Y && lE < lI))
                    {
                        p2 = new XYZ(pE.X - distval, pE.Y + 1, 0);
                    }
                    XYZ p1 = new XYZ(p2.X, p2.Y + 1, 0);
                    l1 = Line.CreateBound(p1, p2);
                }
                Dimension dim1 = doc.Create.NewDimension(doc.ActiveView, l1, referenceArray1);
                if (dim1.ValueString == "0")
                {
                    IList<ElementId> elementIds = new List<ElementId>();
                    elementIds.Add(dim1.Id);
                    doc.ActiveView.HideElements(elementIds);
                }
                #endregion

                #region Create Secound Dimension
                ReferenceArray referenceArray2 = new ReferenceArray();
                referenceArray2.Append(element.EdgeReference[index]);
                referenceArray2.Append(element.GridInColumn.Reference2);
                Line l2;
                if (Math.Round((element.GridInColumn.Element2 as Line).Direction.X, 1) == 0)
                {
                    XYZ p3 = new XYZ(pE.X + 1 + distval, pE.Y - distval, 0);
                    if ((pE.X < pI.X && pE.Y > pI.Y && lE > lI) || (pE.X > pI.X && pE.Y > pI.Y && lE > lI) || (pE.X < pI.X && pE.Y > pI.Y && lE < lI) || (pE.X > pI.X && pE.Y > pI.Y && lE < lI))
                    {
                        p3 = new XYZ(pE.X, pE.Y + distval, 0);
                    }
                    XYZ p1 = new XYZ(p3.X + 1, p3.Y, 0);
                    l2 = Line.CreateBound(p3, p1);
                }
                else
                {
                    XYZ p3 = new XYZ(pE.X + distval, pE.Y + 1, 0);
                    if ((pE.X < pI.X && pE.Y > pI.Y && lE > lI) || (pE.X < pI.X && pE.Y < pI.Y && lE > lI) || (pE.X > pI.X && pE.Y > pI.Y && lE < lI) || (pE.X > pI.X && pE.Y < pI.Y && lE < lI))
                    {
                        p3 = new XYZ(pE.X - distval, pE.Y + 1, 0);
                    }
                    XYZ p1 = new XYZ(p3.X, p3.Y + 1, 0);
                    l2 = Line.CreateBound(p1, p3);
                }
                Dimension dim2 = doc.Create.NewDimension(doc.ActiveView, l2, referenceArray2);
                if (dim2.ValueString == "0")
                {
                    IList<ElementId> elementIds2 = new List<ElementId>();
                    elementIds2.Add(dim2.Id);
                    doc.ActiveView.HideElements(elementIds2);
                }
                #endregion

            }
        }
        public static void CreateShort2ColumnDimension(List<RecColumnConcrete> listOfColumnObject, Document doc)
        {
            foreach (RecColumnConcrete element in listOfColumnObject)
            {
                List<double> length = new List<double>();
                foreach (XYZ item2 in element.EdgePoints)
                {
                    length.Add(item2.DistanceTo(element.GridInColumn.PointOfIntersection));
                }
                int index = length.IndexOf(length.Min());

                #region Create First Dimension
                ReferenceArray referenceArray1 = new ReferenceArray();
                referenceArray1.Append(element.EdgeReference[index]);
                referenceArray1.Append(element.GridInColumn.Reference1);
                Line l1;
                XYZ pE = element.EdgePoints[index];
                XYZ pI = element.GridInColumn.PointOfIntersection;
                double lE = pE.DistanceTo(element.CenterPoint);
                double lI = pI.DistanceTo(element.CenterPoint);
                double distval = 0.5;
                if (Math.Round((element.GridInColumn.Element1 as Line).Direction.X, 1) == 0)
                {
                    XYZ p2 = new XYZ(pE.X + 1 + distval, pE.Y - distval, 0);
                    if ((pE.X < pI.X && pE.Y > pI.Y && lE > lI) || (pE.X > pI.X && pE.Y > pI.Y && lE > lI) || (pE.X < pI.X && pE.Y > pI.Y && lE < lI) || (pE.X > pI.X && pE.Y > pI.Y && lE < lI))
                    {
                        p2 = new XYZ(pE.X, pE.Y + distval, 0);
                    }
                    XYZ p1 = new XYZ(p2.X + 1, p2.Y, 0);
                    l1 = Line.CreateBound(p1, p2);
                }
                else
                {
                    XYZ p2 = new XYZ(pE.X + distval, pE.Y + 1, 0);
                    if ((pE.X < pI.X && pE.Y > pI.Y && lE > lI) || (pE.X < pI.X && pE.Y < pI.Y && lE > lI) || (pE.X > pI.X && pE.Y > pI.Y && lE < lI) || (pE.X > pI.X && pE.Y < pI.Y && lE < lI))
                    {
                        p2 = new XYZ(pE.X - distval, pE.Y + 1, 0);
                    }
                    XYZ p1 = new XYZ(p2.X, p2.Y + 1, 0);
                    l1 = Line.CreateBound(p1, p2);
                }
                Dimension dim1 = doc.Create.NewDimension(doc.ActiveView, l1, referenceArray1);
                if (dim1.ValueString == "0")
                {
                    IList<ElementId> elementIds = new List<ElementId>();
                    elementIds.Add(dim1.Id);
                    doc.ActiveView.HideElements(elementIds);
                }
                #endregion

                #region Create Secound Dimension
                ReferenceArray referenceArray2 = new ReferenceArray();
                referenceArray2.Append(element.EdgeReference[index]);
                referenceArray2.Append(element.GridInColumn.Reference2);
                Line l2;
                if (Math.Round((element.GridInColumn.Element2 as Line).Direction.X, 1) == 0)
                {
                    XYZ p3 = new XYZ(pE.X + 1 + distval, pE.Y - distval, 0);
                    if ((pE.X < pI.X && pE.Y > pI.Y && lE > lI) || (pE.X > pI.X && pE.Y > pI.Y && lE > lI) || (pE.X < pI.X && pE.Y > pI.Y && lE < lI) || (pE.X > pI.X && pE.Y > pI.Y && lE < lI))
                    {
                        p3 = new XYZ(pE.X, pE.Y + distval, 0);
                    }
                    XYZ p1 = new XYZ(p3.X + 1, p3.Y, 0);
                    l2 = Line.CreateBound(p3, p1);
                }
                else
                {
                    XYZ p3 = new XYZ(pE.X + distval, pE.Y + 1, 0);
                    if ((pE.X < pI.X && pE.Y > pI.Y && lE > lI) || (pE.X < pI.X && pE.Y < pI.Y && lE > lI) || (pE.X > pI.X && pE.Y > pI.Y && lE < lI) || (pE.X > pI.X && pE.Y < pI.Y && lE < lI))
                    {
                        p3 = new XYZ(pE.X - distval, pE.Y + 1, 0);
                    }
                    XYZ p1 = new XYZ(p3.X, p3.Y + 1, 0);
                    l2 = Line.CreateBound(p1, p3);
                }
                Dimension dim2 = doc.Create.NewDimension(doc.ActiveView, l2, referenceArray2);
                if (dim2.ValueString == "0")
                {
                    IList<ElementId> elementIds2 = new List<ElementId>();
                    elementIds2.Add(dim2.Id);
                    doc.ActiveView.HideElements(elementIds2);
                }
                #endregion

            }




        }

        #endregion
    }
}
