using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{
    public class RenewalGoal : ProEstEntity
    {
        public int ID { get; private set; }
        public int SalesRepId { get; set; }
        public int BonusMonth { get; set; }
        public int BonusYear { get; set; }
        public int RenewalGoal1Yr { get; set; }
        public int RenewalGoal2Yr { get; set; }
        public int SalesGoal { get; set; }
        public int SalesBonus100 { get; set; }
        public int SalesBonus110 { get; set; }
        public int SalesBonus120 { get; set; }
        public int SalesBonus130 { get; set; }
        public int RenewalBonus1Yr100 { get; set; }
        public int RenewalBonus1Yr110 { get; set; }
        public int RenewalBonus120 { get; set; }
        public int RenewalBonus130 { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int Forecast { get; set; }
        public int ActualSales { get; set; }

        public RenewalGoal()
        {

        }

        public RenewalGoal(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            SalesRepId = InputHelper.GetInteger(row["SalesRepId"].ToString());
            BonusMonth = InputHelper.GetInteger(row["BonusMonth"].ToString());
            BonusYear = InputHelper.GetInteger(row["BonusYear"].ToString());
            RenewalGoal1Yr = InputHelper.GetInteger(row["RenewalGoal1Yr"].ToString());
            RenewalGoal2Yr = InputHelper.GetInteger(row["RenewalGoal2Yr"].ToString());
            SalesGoal = InputHelper.GetInteger(row["SalesGoal"].ToString());
            SalesBonus100 = InputHelper.GetInteger(row["SalesBonus100"].ToString());
            SalesBonus110 = InputHelper.GetInteger(row["SalesBonus110"].ToString());
            SalesBonus120 = InputHelper.GetInteger(row["SalesBonus120"].ToString());
            SalesBonus130 = InputHelper.GetInteger(row["SalesBonus130"].ToString());
            RenewalBonus1Yr100 = InputHelper.GetInteger(row["RenewalBonus1Yr100"].ToString());
            RenewalBonus1Yr110 = InputHelper.GetInteger(row["RenewalBonus1Yr110"].ToString());
            RenewalBonus120 = InputHelper.GetInteger(row["RenewalBonus120"].ToString());
            RenewalBonus130 = InputHelper.GetInteger(row["RenewalBonus130"].ToString());
            Forecast = InputHelper.GetInteger(row["SalesForcast"].ToString());
            CreateDate = InputHelper.GetDateTimeNullable(row["CreateDate"].ToString());
            ModifiedDate = InputHelper.GetDateTimeNullable(row["ModifiedDate"].ToString());
            ActualSales = InputHelper.GetInteger(row["ActualSales"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("SalesRepId", SalesRepId));
            parameters.Add(new SqlParameter("BonusMonth", BonusMonth));
            parameters.Add(new SqlParameter("BonusYear", BonusYear));
            parameters.Add(new SqlParameter("RenewalGoal1Yr", RenewalGoal1Yr));
            parameters.Add(new SqlParameter("RenewalGoal2Yr", RenewalGoal2Yr));
            parameters.Add(new SqlParameter("SalesGoal", SalesGoal));
            parameters.Add(new SqlParameter("SalesBonus100", SalesBonus100));
            parameters.Add(new SqlParameter("SalesBonus110", SalesBonus110));
            parameters.Add(new SqlParameter("SalesBonus120", SalesBonus120));
            parameters.Add(new SqlParameter("SalesBonus130", SalesBonus130));
            parameters.Add(new SqlParameter("RenewalBonus1Yr100", RenewalBonus1Yr100));
            parameters.Add(new SqlParameter("RenewalBonus1Yr110", RenewalBonus1Yr110));
            parameters.Add(new SqlParameter("RenewalBonus120", RenewalBonus120));
            parameters.Add(new SqlParameter("RenewalBonus130", RenewalBonus130));
            parameters.Add(new SqlParameter("SalesForcast", Forecast));
            parameters.Add(new SqlParameter("CreateDate", CreateDate));
            parameters.Add(new SqlParameter("ModifiedDate", ModifiedDate));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("RenewalGoal_Update", parameters);

            if (intResult.Success)
            {
                ID = intResult.Value;
                ChangeLogManager.LogChange(activeLoginID, "RenewalGoal", ID, 0, parameters, RowAsLoaded);
                return new SaveResult();
            }
            else
            {
                return new SaveResult(intResult.ErrorMessage);
            }
        }

        public static RenewalGoal Get(int salesRepID, int month, int year)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("SalesRepId", salesRepID));
            parameters.Add(new SqlParameter("BonusMonth", month));
            parameters.Add(new SqlParameter("BonusYear", year));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("RenewalGoal_Get", parameters);
            if (tableResult.Success)
            {
                return new RenewalGoal(tableResult.DataTable.Rows[0]);
            }

            return null;
        }
    }
}
