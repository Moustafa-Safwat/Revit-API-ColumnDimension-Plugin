using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ColumnDimensionRevit.Model
{
    public class MyGrid
    {
        #region Automatic Property
        //public double Length { get; set; }
        public Curve Element1 { get; set; }
        public Curve Element2 { get; set; }
        public Reference Reference1 { get; set; }
        public Reference Reference2 { get; set; }
        public XYZ PointOfIntersection { get; set; }
        #endregion

        #region Constructor
        public MyGrid()
        {
        }
        public MyGrid(XYZ p)
        {
            PointOfIntersection = p;
        }
        public MyGrid(Reference ref1, Reference ref2)
        {
            Reference1 = ref1;
            Reference2 = ref2;
        }
        public MyGrid(Reference ref1, Reference ref2, XYZ p)
        {
            Reference1 = ref1;
            Reference2 = ref2;
            PointOfIntersection = p;
        }
        #endregion

        #region Methods
        public static List<MyGrid> GetGridIntersections(IList<Reference> grids, Document doc)
        {
            List<MyGrid> myGridsList = new List<MyGrid>();
            List<MyGrid> myGridsListDist = new List<MyGrid>();
            IList<Element> gridsElement = new List<Element>();
            foreach (Reference item in grids)
            {
                gridsElement.Add(doc.GetElement(item));
            }
            IList<Curve> curveArray1 = new List<Curve>();
            IList<Curve> curveArray2 = new List<Curve>();
            IntersectionResultArray ira = new IntersectionResultArray();
            foreach (Element gridElement in gridsElement)
            {
                curveArray1.Add((gridElement as Grid).Curve);
                curveArray2.Add((gridElement as Grid).Curve);
            }
            int i1 = 0;
            foreach (Curve curve1 in curveArray1)
            {
                int i2 = 0;
                foreach (Curve curve2 in curveArray2)
                {
                    SetComparisonResult result = curve1.Intersect(curve2, out ira);
                    if (result == SetComparisonResult.Overlap)
                    {
                        foreach (IntersectionResult ir in ira)
                        {
                            MyGrid myGrid = new MyGrid();
                            myGrid.PointOfIntersection = ir.XYZPoint;
                            myGrid.Reference1 = grids[i1];
                            myGrid.Reference2 = grids[i2];
                            myGrid.Element1 = curveArray1[i1];
                            myGrid.Element2 = curveArray2[i2];

                            //myGrid.Length = Math.Round(Math.Sqrt(ir.XYZPoint.X * ir.XYZPoint.X + ir.XYZPoint.Y * ir.XYZPoint.Y), 5);
                            myGridsList.Add(myGrid);
                        }
                    }
                    i2++;
                }
                i1++;
            }
            return myGridsList.Distinct().ToList();
        }
        #endregion

    }
}
