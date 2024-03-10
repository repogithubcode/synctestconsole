using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProEstimatorData.Models.SubModel
{
    public class VINDecodeResult
    {
        public int Year { get; set; }
        public int MakeID { get; set; }
        public int ModelID { get; set; }
        public int SubModelID { get; set; }
        public int BodyID { get; set; }
        public int EngineID { get; set; }
        public int TransmissionID { get; set; }
        public int DriveID { get; set; }
        public List<string> Years { get; set; }
        public List<MakeDropDown> Makes { get; set; }
        public List<ModelDropDown> Models { get; set; }
        public List<SubModelDropDown> SubModels { get; set; }
        public List<BodyTypeDropDown> BodyTypes { get; set; }
        public List<EngineTypeDropDown> EngineTypes {get; set;}
        public List<TransmissionTypeDropDown> TransmissionTypes {get;set;}
        public List<DriveTypeDropDown> DriveTypes {get;set;}
        public VehicleInfo VehicleInfo { get; set; }
        public bool Success { get; set; }
        public string ErrorMessages { get; set; }
        public List<VehicleInfo> SimilarVins { get; set; }
    }

    public class VehicleInfo
    {
        public string Vin { get; set; }
        public string General { get; set; }
        public string Body { get; set; }
        public string Engine { get; set; }
        public string Drive { get; set; }
        public string Country { get; set; }
        public string Tire { get; set; }
        public string Transmission { get; set; }
        public string VehicleID { get; set; }

        public VehicleInfo(string vin, string general, string body, string engine, string drive, string country, string tire, string trans)
        {
            Vin = vin;
            General = general;
            Body = body;
            Engine = engine;
            Drive = drive;
            Country = country;
            Tire = tire;
            Transmission = trans;
        }
        public VehicleInfo(string vin, string general)
        {
            Vin = vin;
            General = general;
        }
    }
}