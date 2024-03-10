using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.Models;
using ProEstimatorData.DataModel.Contracts;
using Proestimator.ViewModel.Contracts;
using Proestimator.ViewModelMappers.Contracts;

namespace Proestimator.ViewModel.Contracts
{
    public class PickAddOnsVM
    {
        public int LoginID { get; set; }
        public bool GoodData { get; set; }
        public int ContractID { get; set; }
        public string ContractMessage { get; set; }
        public List<AddOnDetailsVM> AddOnDetails { get; set; } = new List<AddOnDetailsVM>();
        public string Errors { get; set; }
        public bool HasBundle { get; set; }
    }
}