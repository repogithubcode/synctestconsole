using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Profiles
{
    public class PaintSettings : ProEstEntity
    {
        public int ID { get; private set;}
        public int CustomerProfilesID { get; set; }
        public bool AllowDeductions	{ get; set; }
        public double AdjacentDeduction	{ get; set; }
        public double NonAdjacentDeduction	{ get; set; }
        public bool EdgeInteriorTimes	{ get; set; }
        public double MajorClearCoat	{ get; set; }
        public double MajorThreeStage	{ get; set; }
        public double MajorTwoTone	{ get; set; }
        public double OverlapClearCoat	{ get; set; }
        public double OverlapThreeStage	{ get; set; }
        public double OverLapTwoTone	{ get; set; }
        public double ClearCoatCap	{ get; set; }
        public bool DeductFinishOverlap	{ get; set; }
        public bool TotalClearcoatWithPaint	{ get; set; }
        public bool NoClearcoatCap	{ get; set; }
        public bool ThreeStageInner	{ get; set; }
        public bool ThreeStagePillars	{ get; set; }
        public bool ThreeStateInterior	{ get; set; }
        public bool TwoToneInner	{ get; set; }
        public bool TwoTonePillars	{ get; set; }
        public bool TwoToneInterior	{ get; set; }
        public double Blend	{ get; set; }
        public double ThreeTwoBlend	{ get; set; }
        public double Underside	{ get; set; }
        public double Edging	{ get; set; }
        public bool ManualPaintOverlap	{ get; set; }
        public bool AutomaticOverlap { get; set; }

        public PaintSettings() { }

        public PaintSettings(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            CustomerProfilesID = InputHelper.GetInteger(row["CustomerProfilesID"].ToString());
            AllowDeductions = InputHelper.GetBoolean(row["AllowDeductions"].ToString());
            AdjacentDeduction = InputHelper.GetDouble(row["AdjacentDeduction"].ToString());
            NonAdjacentDeduction = InputHelper.GetDouble(row["NonAdjacentDeduction"].ToString());
            EdgeInteriorTimes = InputHelper.GetBoolean(row["EdgeInteriorTimes"].ToString());
            MajorClearCoat = InputHelper.GetDouble(row["MajorClearCoat"].ToString());
            MajorThreeStage = InputHelper.GetDouble(row["MajorThreeStage"].ToString());
            MajorTwoTone = InputHelper.GetDouble(row["MajorTwoTone"].ToString());
            OverlapClearCoat = InputHelper.GetDouble(row["OverlapClearCoat"].ToString());
            OverlapThreeStage = InputHelper.GetDouble(row["OverlapThreeStage"].ToString());
            OverLapTwoTone = InputHelper.GetDouble(row["OverLapTwoTone"].ToString());
            ClearCoatCap = InputHelper.GetDouble(row["ClearCoatCap"].ToString());
            DeductFinishOverlap = InputHelper.GetBoolean(row["DeductFinishOverlap"].ToString());
            TotalClearcoatWithPaint = InputHelper.GetBoolean(row["TotalClearcoatWithPaint"].ToString());
            NoClearcoatCap = InputHelper.GetBoolean(row["NoClearcoatCap"].ToString());
            ThreeStageInner = InputHelper.GetBoolean(row["ThreeStageInner"].ToString());
            ThreeStagePillars = InputHelper.GetBoolean(row["ThreeStagePillars"].ToString());
            ThreeStateInterior = InputHelper.GetBoolean(row["ThreeStateInterior"].ToString());
            TwoToneInner = InputHelper.GetBoolean(row["TwoToneInner"].ToString());
            TwoTonePillars = InputHelper.GetBoolean(row["TwoTonePillars"].ToString());
            TwoToneInterior = InputHelper.GetBoolean(row["TwoToneInterior"].ToString());
            Blend = InputHelper.GetDouble(row["Blend"].ToString());
            ThreeTwoBlend = InputHelper.GetDouble(row["ThreeTwoBlend"].ToString());
            Underside = InputHelper.GetDouble(row["Underside"].ToString());
            Edging = InputHelper.GetDouble(row["Edging"].ToString());
            ManualPaintOverlap = InputHelper.GetBoolean(row["ManualPaintOverlap"].ToString());
            AutomaticOverlap = InputHelper.GetBoolean(row["AutomaticOverlap"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("CustomerProfilesID", CustomerProfilesID));
            parameters.Add(new SqlParameter("AllowDeductions", AllowDeductions));
            parameters.Add(new SqlParameter("AdjacentDeduction", AdjacentDeduction));
            parameters.Add(new SqlParameter("NonAdjacentDeduction", NonAdjacentDeduction));
            parameters.Add(new SqlParameter("EdgeInteriorTimes", EdgeInteriorTimes));
            parameters.Add(new SqlParameter("MajorClearCoat", MajorClearCoat));
            parameters.Add(new SqlParameter("MajorThreeStage", MajorThreeStage));
            parameters.Add(new SqlParameter("MajorTwoTone", MajorTwoTone));
            parameters.Add(new SqlParameter("OverlapClearCoat", OverlapClearCoat));
            parameters.Add(new SqlParameter("OverlapThreeStage", OverlapThreeStage));
            parameters.Add(new SqlParameter("OverLapTwoTone", OverLapTwoTone));
            parameters.Add(new SqlParameter("ClearCoatCap", ClearCoatCap));
            parameters.Add(new SqlParameter("DeductFinishOverlap", DeductFinishOverlap));
            parameters.Add(new SqlParameter("TotalClearcoatWithPaint", TotalClearcoatWithPaint));
            parameters.Add(new SqlParameter("NoClearcoatCap", NoClearcoatCap));
            parameters.Add(new SqlParameter("ThreeStageInner", ThreeStageInner));
            parameters.Add(new SqlParameter("ThreeStagePillars", ThreeStagePillars));
            parameters.Add(new SqlParameter("ThreeStateInterior", ThreeStateInterior));
            parameters.Add(new SqlParameter("TwoToneInner", TwoToneInner));
            parameters.Add(new SqlParameter("TwoTonePillars", TwoTonePillars));
            parameters.Add(new SqlParameter("TwoToneInterior", TwoToneInterior));
            parameters.Add(new SqlParameter("Blend", Blend));
            parameters.Add(new SqlParameter("ThreeTwoBlend", ThreeTwoBlend));
            parameters.Add(new SqlParameter("Underside", Underside));
            parameters.Add(new SqlParameter("Edging", Edging));
            parameters.Add(new SqlParameter("ManualPaintOverlap", ManualPaintOverlap));
            parameters.Add(new SqlParameter("AutomaticOverlap", AutomaticOverlap));


            DBAccess db = new DBAccess();
            FunctionResult result = db.ExecuteNonQuery("CustomerProfilesPaint_Update", parameters);

            if(result.Success)
            {
                RateProfile profile = RateProfile.Get(CustomerProfilesID);
                ChangeLogManager.LogChange(activeLoginID, "PaintSettings", ID, profile.LoginID, parameters, RowAsLoaded, profile.Name);
            }

            return new SaveResult(result);
        }

        public static PaintSettings GetForProfile(int profileID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomerProfilesPaint_GetByProfile", new SqlParameter("CustomerProfilesID", profileID));

            if (tableResult.Success)
            {
                return new PaintSettings(tableResult.DataTable.Rows[0]);
            }

            return null;
        }
    }
}