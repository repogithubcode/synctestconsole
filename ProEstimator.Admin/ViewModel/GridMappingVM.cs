using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProEstimator.Business.ILogic;

using ProEstimatorData.DataModel;

namespace ProEstimator.Admin.ViewModel
{
    public class GridMappingVM
    {
        public int? GridColumnID { get; set; }
        public Boolean Visible { get; set; }

        public string Name { get; set; }
        public string HeaderText { get; set; }

        public int SortOrderIndex { get; set; }

        public GridMappingVM()
        {
            
        }

        public GridMappingVM(GridColumnInfoUserMapping mapping)
        {
            GridColumnID = mapping.GridColumnInfo.ID;
            Visible = mapping.Visible;
            Name = mapping.GridColumnInfo.Name;
            HeaderText = mapping.GridColumnInfo.HeaderText;
            SortOrderIndex = mapping.SortOrderIndex;
        }
    }
}