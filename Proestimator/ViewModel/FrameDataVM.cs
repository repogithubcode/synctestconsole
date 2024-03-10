using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;

using ProEstimatorData;
using ProEstimatorData.Models;

namespace Proestimator.ViewModel
{
    public class FrameDataVM
    {
        public int LoginID { get; set; }
        public int EstimateID { get; set; }
        public bool IsTrial { get; set; }

        public bool ContractDeactivated { get; set; }

        public List<FrameDetailsVM> Results { get; set; }

        public int Year { get; set; }
        public int MakeID { get; set; }
        public int ModelID { get; set; }

        public List<int> Years { get; set; }
        public List<VehicleMakeVM> Makes { get; set; }
        public List<ModelVM> Models { get; set; }

        public SelectList YearList { get; set; }
        public SelectList MakeList { get; set; }
        public SelectList ModelList { get; set; }

        public string ImagePath { get; set; }
        public string SelectedImage { get; set; }

        public bool DisplayPrint { get; set; }

        public FrameDataVM()
        {
            Makes = new List<VehicleMakeVM>();
            Models = new List<ModelVM>();
            ContractDeactivated = false;
        }
    }

    public class FrameDetailsVM
    {
        public int CarID { get; set; }
        public string CarMakes { get; set; }
        public string CarModels { get; set; }
        public Nullable<int> Year { get; set; }
        public string CarFrameDetails { get; set; }
        public string dwf { get; set; }
        public string jpg { get; set; }
    }

    public class VehicleMakeVM
    {
        public int makId { get; set; }
        public string makDescription { get; set; }

        public VehicleMakeVM(DataRow row)
        {
            makId = InputHelper.GetInteger(row["makId"].ToString());
            makDescription = row["makDescription"].ToString();
        }
    }

    public class ModelVM
    {
        public int vehModelId { get; set; }
        public string makDescription { get; set; }
        public string model { get; set; }

        public ModelVM(DataRow row)
        {
            vehModelId = InputHelper.GetInteger(row["vehModelId"].ToString());
            makDescription = row["makDescription"].ToString();
            model = row["model"].ToString();
        }
    }
}