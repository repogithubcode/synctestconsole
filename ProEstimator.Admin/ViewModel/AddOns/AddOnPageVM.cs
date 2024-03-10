using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.AddOns;

namespace ProEstimator.Admin.ViewModel.AddOns
{
    public class AddOnPageVM
    {

        public SelectList MatchTypes { get; set; }

        public AddOnPageVM()
        {
            // Add the Match Types drop down
            List<SelectListItem> matchTypes = new List<SelectListItem>();
            matchTypes.Add(new SelectListItem() { Value = "1", Text = "Include" });
            matchTypes.Add(new SelectListItem() { Value = "2", Text = "Do Not Include" });
            MatchTypes = new SelectList(matchTypes, "Value", "Text");
        }

    }

    public class SectionVM
    {
        public int Header { get; set; }
        public int Section { get; set; }

        public string Name { get; set; }

        public SectionVM()
        { }

        public SectionVM(DataRow row)
        {
            Header = InputHelper.GetInteger(row["nheader"].ToString());
            Section = InputHelper.GetInteger(row["nsection"].ToString());
            Name = InputHelper.GetString(row["SectionName"].ToString());
        }
    }

    public class PartDetailsVM
    {
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public double Price { get; set; }
        public string Barcode { get; set; }
        public string PartText { get; set; }
        public string Notes { get; set; }

        public PartDetailsVM()
        { }

        public PartDetailsVM(DataRow row)
        {
            PartNumber = InputHelper.GetString(row["PartNumber"].ToString());
            Description = InputHelper.GetString(row["Description"].ToString());
            Comment = InputHelper.GetString(row["comment"].ToString());
            Price = InputHelper.GetDouble(row["Price"].ToString());
            Barcode = InputHelper.GetString(row["Barcode"].ToString());
            PartText = InputHelper.GetString(row["Part_Text"].ToString());
            Notes = InputHelper.GetString(row["Notes"].ToString());
        }
    }
}