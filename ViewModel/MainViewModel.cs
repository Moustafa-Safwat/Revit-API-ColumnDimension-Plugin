using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ColumnDimensionRevit.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace ColumnDimensionRevit.ViewModel
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class MainViewModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            using (Transaction tran = new Transaction(doc, "Column Dimension"))
            {
                try
                {
                    tran.Start();
                    ISelectionFilter filterElement = new FilterClass();
                    IList<Reference> references = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, filterElement, "Select Structural Columns And Grids");
                    IList<Reference> grids = new List<Reference>();
                    IList<Element> columnConcreteRect = new List<Element>();
                    IList<Element> columnConcreteCir = new List<Element>();
                    IList<Element> columnSteel = new List<Element>();
                    foreach (Reference referenceItem in references)
                    {
                        Element element = doc.GetElement(referenceItem);
                        if (element.Category.Name == "Grids")
                        {
                            grids.Add(referenceItem);
                        }
                        else
                        {
                            if ((element as FamilyInstance).StructuralMaterialType == Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete)
                            {
                                if (CirColumnConcrete.IsCircularColumn(element))
                                {
                                    columnConcreteCir.Add(element);
                                }
                                else
                                {
                                    columnConcreteRect.Add(element);
                                }
                            }
                            else if ((element as FamilyInstance).StructuralMaterialType == Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel)
                            {
                                columnSteel.Add(element);
                            }
                        }
                    }
                    List<MyGrid> listOfGridIntersection = MyGrid.GetGridIntersections(grids, doc);
                    //
                    #region Dimension For Concrete Rectangle Columns
                    List<RecColumnConcrete> listOfColumnConcreteRectObject = new List<RecColumnConcrete>();
                    foreach (Element columnElement in columnConcreteRect)
                    {
                        RecColumnConcrete item = new RecColumnConcrete();
                        RecColumnConcrete.GetColumnCenterPoint(columnElement, item);
                        RecColumnConcrete.GetColumnRotation(columnElement, item);
                        RecColumnConcrete.GetColumnCornerPoints(columnElement, item);
                        RecColumnConcrete.GetColumnEdgeReference(columnElement, item);
                        RecColumnConcrete.GetColumnAxisReference(item, listOfGridIntersection);
                        listOfColumnConcreteRectObject.Add(item);
                    }
                    RecColumnConcrete.CreateShort1ColumnDimension(listOfColumnConcreteRectObject, doc);
                    #endregion
                    //
                    #region Dimension For Concrete CircularColumns
                    List<CirColumnConcrete> listOfColumnConcreteCirObject = new List<CirColumnConcrete>();
                    foreach (Element columnElement in columnConcreteCir)
                    {
                        CirColumnConcrete item = new CirColumnConcrete();
                        CirColumnConcrete.GetColumnCenterPoint(columnElement, item);
                        CirColumnConcrete.GetColumnCenterPointReference(columnElement, item);
                        CirColumnConcrete.GetColumnAxisReference(item, listOfGridIntersection);
                        listOfColumnConcreteCirObject.Add(item);
                    }
                    CirColumnConcrete.GetCircularColumnDimension(listOfColumnConcreteCirObject, doc);
                    #endregion
                    //
                    #region Dimension For Steel Columns
                    List<GenColumnSteel> listOfColumnSteelObject = new List<GenColumnSteel>();
                    foreach (Element columnElement in columnSteel)
                    {
                        GenColumnSteel item = new GenColumnSteel();
                        GenColumnSteel.GetColumnCenterPoint(columnElement, item);
                        GenColumnSteel.GetColumnRotation(columnElement, item);
                        GenColumnSteel.GetColumnCenterPointReference(columnElement, item);
                        GenColumnSteel.GetColumnAxisReference(item, listOfGridIntersection);
                        listOfColumnSteelObject.Add(item);
                    }
                    GenColumnSteel.GetSteelColumnDimension(listOfColumnSteelObject, doc);
                    #endregion

                    tran.Commit();
                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    tran.RollBack();
                    return Result.Failed;
                }
            }
        }
    }
}
