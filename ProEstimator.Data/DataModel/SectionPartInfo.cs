using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class SectionPartInfo
    {
        public string Reference { get; set; }
        public int RepairID { get; set; }
        public Single LaborTime { get; set; }
        public string ExName { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public string comment { get; set; }
        public decimal Price { get; set; }
        public string Reference2 { get; set; }
        public int LaborType { get; set; }
        public string LaborName { get; set; }
        public int LaborNameID { get; set; }
        public int PaintType { get; set; }
        public string PaintName { get; set; }
        public decimal PaintTime { get; set; }
        public decimal RRTime { get; set; }
        public decimal RITime { get; set; }
        public string VehiclePosition { get; set; }
        public int QtyOrdered { get; set; }
        public string Action { get; set; }
        public int ID { get; set; }
        public int LineNumber { get; set; }
        public int DetailLineCallout { get; set; }
        public string Service_Barcode { get; set; }
        public string Barcode { get; set; }
        public string Part_Text { get; set; }
        public string Notes { get; set; }
        public decimal AddTime { get; set; }
        public decimal AlignTime { get; set; }
        public decimal OHTime { get; set; }
        public decimal AITime { get; set; }
        public decimal CATime { get; set; }
        public Single LaborTimeBlend { get; set; }
        public Single LaborPaintTime { get; set; }
        public int LaborPaintType { get; set; }
        public int LaborMainType { get; set; }

        public int SectionID { get; private set; }

        public SectionPartInfo(DataRow row, int nheader, int nsection)
        {
            Reference = row["Reference"].ToString();
            RepairID  = InputHelper.GetInteger(row["RepairID"].ToString());
            LaborTime = InputHelper.GetSingle(row["LaborTime"].ToString());
            ExName = row["ExName"].ToString();
            PartNumber = row["PartNumber"].ToString();
            Description = row["Description"].ToString();
            comment = row["comment"].ToString();
            Price = InputHelper.GetDecimal(row["Price"].ToString());
            Reference2 = row["Reference2"].ToString();
            LaborType = InputHelper.GetInteger(row["LaborType"].ToString());
            LaborName = row["LaborName"].ToString();
            LaborNameID = InputHelper.GetInteger(row["LaborNameID"].ToString());
            PaintType = InputHelper.GetInteger(row["PaintType"].ToString());
            PaintName = row["PaintName"].ToString();
            PaintTime = InputHelper.GetDecimal(row["PaintTime"].ToString());
            RRTime = InputHelper.GetDecimal(row["RRTime"].ToString());
            RITime  = InputHelper.GetDecimal(row["RITime"].ToString());
            VehiclePosition= row["VehiclePosition"].ToString();
            QtyOrdered  = InputHelper.GetInteger(row["QtyOrdered"].ToString());
            Action = row["Action"].ToString();
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LineNumber= InputHelper.GetInteger(row["LineNumber"].ToString());
            DetailLineCallout  = InputHelper.GetInteger(row["DetailLineCallout"].ToString());
            Service_Barcode  = row["Service_Barcode"].ToString();
            Barcode = row["Barcode"].ToString();
            Part_Text = row["Part_Text"].ToString();
            Notes = row["Notes"].ToString();
            AddTime = InputHelper.GetDecimal(row["AddTime"].ToString());
            AlignTime = InputHelper.GetDecimal(row["AlignTime"].ToString());
            OHTime = InputHelper.GetDecimal(row["OHTime"].ToString());
            AITime= InputHelper.GetDecimal(row["AITime"].ToString());
            CATime = InputHelper.GetDecimal(row["CATime"].ToString());
            LaborTimeBlend = InputHelper.GetSingle(row["LaborTimeBlend"].ToString());
            LaborPaintTime = InputHelper.GetSingle(row["LaborPaintTime"].ToString());
            LaborPaintType = InputHelper.GetInteger(row["LaborPaintType"].ToString());
            LaborMainType = InputHelper.GetInteger(row["LaborMainType"].ToString());

            SectionID = (nheader * 256) + nsection;
        }
    }
}
