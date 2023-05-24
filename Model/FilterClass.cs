using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColumnDimensionRevit.Model
{
    public class FilterClass : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category != null)
            {
                if (elem.Category.Name == "Grids" || elem.Category.Name == "Structural Columns")
                {
                    return true;
                }
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
