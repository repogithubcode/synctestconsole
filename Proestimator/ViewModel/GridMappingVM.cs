using System;
using ProEstimatorData.DataModel;
using Proestimator.Resources;

namespace Proestimator.ViewModel
{
    public class GridMappingVM
    {
        public int? GridColumnID { get; set; }
        public Boolean Visible { get; set; }

        public string Name { get; set; }
        public string HeaderText { get; set; }

        public GridMappingVM()
        {
            
        }

        public GridMappingVM(GridColumnInfoUserMapping mapping, string languagePreference)
        {
            this.GridColumnID = mapping.GridColumnInfo.ID;
            this.Name = mapping.GridColumnInfo.Name;
            this.HeaderText = mapping.GridColumnInfo.HeaderText;

            string cultureName = languagePreference;
            if (cultureName == "es")
            {
                this.HeaderText = ProStrings.ResourceManager.GetString(Name) ?? Name;
            }
            
            this.Visible = mapping.Visible;
        }
    }
}