using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.Models.SubModel;

using ProEstimator.Business.Logic;

namespace Proestimator.ViewModel
{
    public class VehicleVM
    {
        public int LoginID { get; set; }
        public int EstimateID { get; set; }
        public int UserID { get; set; }
        public bool EstimateIsLocked { get; set; }
        public string Vin { get; set; }
        public string Year { get; set; }
        public string MileageIn { get; set; }
        public string MileageOut { get; set; }
        public string License { get; set; }
        public string LicenseDecode { get; set; }
        [AllowHtml]
        public string VehicleNotes { get; set; }
        public string ExteriorColor { get; set; }
        public string ExteriorColor2 { get; set; }
        public string InteriorColor { get; set; }
        public int DriveType { get; set; }
        public string EstimatedValue { get; set; }
        public int Make { get; set; }
        public int Model { get; set; }
        public int Trim { get; set; }
        public string ServiceBarcode { get; set; }
        public int Engine { get; set; }
        public int Trans { get; set; }
        public int Body { get; set; }
        public int PaintType { get; set; }
        public string PaintCode { get; set; }
        public string PaintCode2 { get; set; }
        public string LicenseState { get; set; }
        public string LicenseStateDecode { get; set; }
        public string ProductionYear { get; set; }
        public string ProductionMonth { get; set; }
        public string ProductionDate { get; set; }
        public string StockNumber { get; set; }

        public int POILabel1 { get; set; }
        public int POIOption1 { get; set; }
        public string POICustom1 { get; set; }

        public int POILabel2 { get; set; }
        public int POIOption2 { get; set; }
        public string POICustom2 { get; set; }

        public string ErrorMessage { get; set; }

        public SelectList YearDDL { get; set; }
        public SelectList POIDDL { get; set; }
        public SelectList StateDDL { get; set; }
        public SelectList MakeDDL { get; set; }
        public SelectList ModelDDL { get; set; }
        public SelectList SubModelDDL { get; set; }
        public SelectList BodyTypeDDL { get; set; }
        public SelectList DriveTypeDDL { get; set; }
        public SelectList EngineDDL { get; set; }
        public SelectList TransmissionDDL { get; set; }
        public SelectList PaintCodeDDL { get; set; }
        public SelectList PaintCodeDDL2 { get; set; }
        public SelectList PaintTypesDLL { get; set; }
        public SelectList ProductionMonthDDL { get; set; }

        public bool ManualSelection { get; set; }
        public string ManualCountry { get; set; }
        public string ManualManufacturer { get; set; }
        public string ManualMake { get; set; }
        public string ManualModel { get; set; }
        public string ManualModelYear { get; set; }
        public string ManualSubModel { get; set; }
        public string ManualServiceBarcode { get; set; }
        public bool UpdatingExistingLineItems { get; set; } = false;
        public bool Carfax { get; set; }
        public bool CarfaxInfo { get; set; }

        public bool EstimateLineItemsExist { get; set; }
        public bool AllowEstimateVehicleModsWithLineItems { get; set; }

        public Dictionary<int, string> POISectionsArray
        {
            get
            {
                Dictionary<int, string> sections = new Dictionary<int, string>();

                sections.Add(0, "");

                foreach(POISection section in POISection.GetAll())
                {
                    sections.Add(section.ID, section.Name);
                }

                return sections;
            }
        }

        public List<AccessoryInfo> Accessories { get; set; }

        public VehicleVM()
        {
            YearDDL = new SelectList("");
            POIDDL = new SelectList("");
            StateDDL = new SelectList("");
            MakeDDL = new SelectList("");
            ModelDDL = new SelectList("");
            SubModelDDL = new SelectList("");
            BodyTypeDDL = new SelectList("");
            DriveTypeDDL = new SelectList("");
            EngineDDL = new SelectList("");
            TransmissionDDL = new SelectList("");
            PaintCodeDDL = new SelectList("");
            PaintCodeDDL2 = new SelectList("");
            PaintTypesDLL = new SelectList(LookupService.PaintTypes(), "Value", "Text");
            Accessories = new List<AccessoryInfo>();
            ProductionMonthDDL = new SelectList(LookupService.GetMonthSelectList(), "Value", "Text");
        }

        public void LoadFromModel(Vehicle vehicle)
        {
            if (vehicle != null)
            {
                Vin = vehicle.Vin;
                Year = vehicle.Year.ToString();
                MileageIn = vehicle.MileageIn.ToString();
                MileageOut = vehicle.MileageOut.ToString();
                License = vehicle.License;
                LicenseDecode = vehicle.License;
                VehicleNotes = vehicle.VehicleNotes;
                ExteriorColor = vehicle.ExteriorColor;
                ExteriorColor2 = vehicle.ExteriorColor2;
                InteriorColor = vehicle.InteriorColor;
                DriveType = vehicle.DriveType;
                EstimatedValue = vehicle.EstimatedValue.ToString();
                Make = vehicle.MakeID;
                Model = vehicle.ModelID;
                Trim = vehicle.TrimID;
                ServiceBarcode = vehicle.ServiceBarcode;
                Engine = vehicle.EngineID;
                Trans = vehicle.TransID;
                Body = vehicle.BodyID;
                //POI = vehicle.PointOfImpactID;
                PaintType = vehicle.DefaultPaintType;
                PaintCode = vehicle.PaintCode;
                PaintCode2 = vehicle.PaintCode2;
                LicenseState = vehicle.LicenseState;
                LicenseStateDecode = LicenseState;
                ProductionYear = GetProductionYear(vehicle.ProductionDate);
                ProductionMonth = GetProductionMonth(vehicle.ProductionDate);
                ProductionDate = GetProductionDate();
                StockNumber = vehicle.StockNumber;

                POILabel1 = vehicle.POILabel1;
                POIOption1 = vehicle.POIOption1;
                POICustom1 = vehicle.POICustom1;

                POILabel2 = vehicle.POILabel2;
                POIOption2 = vehicle.POIOption2;
                POICustom2 = vehicle.POICustom2;
            }
        }

        public void LoadFromModel(VehicleInfoManual vehicleManual)
        {
            if (vehicleManual != null)
            {
                ManualSelection = vehicleManual.UseManualSelection;

                ManualCountry = vehicleManual.Country;
                ManualMake = vehicleManual.Make;
                ManualManufacturer = vehicleManual.Manufacturer;
                ManualModel = vehicleManual.Model;
                ManualModelYear = vehicleManual.ModelYear;
                ManualServiceBarcode = vehicleManual.ServiceBarcode;
                ManualSubModel = vehicleManual.SubModel;
            }
        }

        public void CopyToModel(Vehicle vehicle)
        {
            vehicle.Vin = Vin;
            vehicle.Year = InputHelper.GetInteger(Year);
            vehicle.MileageIn = InputHelper.GetDecimal(MileageIn);
            vehicle.MileageOut = InputHelper.GetDecimal(MileageOut);
            vehicle.License = License;
            vehicle.VehicleNotes = VehicleNotes;
            vehicle.ExteriorColor = ExteriorColor;
            vehicle.ExteriorColor2 = ExteriorColor2;
            vehicle.InteriorColor = InteriorColor;
            vehicle.DriveType = DriveType;
            vehicle.EstimatedValue = InputHelper.GetDecimal(EstimatedValue);
            vehicle.MakeID = Make;
            vehicle.ModelID = Model;
            vehicle.TrimID = Trim;
            vehicle.ServiceBarcode = ServiceBarcode;
            vehicle.EngineID = Engine;
            vehicle.TransID = Trans;
            vehicle.BodyID = Body;
            //vehicle.PointOfImpactID = POI;
            vehicle.DefaultPaintType = PaintType;
            vehicle.PaintCode = PaintCode;
            vehicle.PaintCode2 = PaintCode2;
            vehicle.LicenseState = LicenseState;
            vehicle.ProductionDate = GetProductionDate();
            vehicle.StockNumber = StockNumber;

            vehicle.POILabel1 = POILabel1;
            vehicle.POIOption1 = POIOption1;
            vehicle.POICustom1 = InputHelper.GetString(POICustom1);

            vehicle.POILabel2 = POILabel2;
            vehicle.POIOption2 = POIOption2;
            vehicle.POICustom2 = InputHelper.GetString(POICustom2);
        }
       
        public string GetProductionDate()
        {
            if (!String.IsNullOrEmpty(ProductionYear) && ProductionYear == "0")
                ProductionYear= string.Empty;
             string productionDate = (ProductionMonth + "-" + ProductionYear).Trim();
             if (productionDate.StartsWith("-") || productionDate.EndsWith("-"))
                 productionDate = productionDate.Replace("-", "");
             return productionDate.ToUpper();
        }

        public string GetProductionYear(string date)
        {
            string prodDate = date ?? string.Empty;
            return prodDate.Split('-').FirstOrDefault(x => x.Trim().Length == 4 && x.Trim().All(char.IsDigit));

        }
        public string GetProductionMonth(string date)
        {
            string prodDate = date ?? string.Empty;
            return prodDate.Split('-').FirstOrDefault(x => x.Trim().Length == 3);

        }
        public void CopyToModel(VehicleInfoManual vehicleManual)
        {
            vehicleManual.Country = ManualCountry;
            vehicleManual.Make = ManualMake;
            vehicleManual.Manufacturer = ManualManufacturer;
            vehicleManual.Model = ManualModel;
            vehicleManual.ModelYear = ManualModelYear;
            vehicleManual.ServiceBarcode = ManualServiceBarcode;
            vehicleManual.SubModel = ManualSubModel;
        }
    }
}