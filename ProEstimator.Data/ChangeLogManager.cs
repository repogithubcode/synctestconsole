using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;
using System.Data;
using ProEstimatorData.Helpers;
using ProEstimatorData.DataModel;

namespace ProEstimatorData
{
    public class ChangeLogManager
    {

        private static string GetValue(string input)
        {
            string trimmed = input.ToString().Trim();

            if (string.IsNullOrEmpty(trimmed))
            {
                return "";
            }

            double doubleTest;
            if (double.TryParse(input, out doubleTest))
            {
                doubleTest = Math.Round(doubleTest, 2);
                return doubleTest.ToString();
            }

            return trimmed;
        }

        public static void LogChange(int activeLoginID, string itemType, int itemID, int loginID, string before, string after)
        {
            if (activeLoginID == 0)
            {
                activeLoginID = HttpContextCurrentItemsHelper.GetItemValue("ActiveLoginID", 0);
            }

            if (activeLoginID == 0)
            {
                return;
            }

            try
            {
                if (before != after)
                {
                    List<SqlParameter> insertParameters = new List<SqlParameter>();
                    insertParameters.Add(new SqlParameter("LoginID", loginID));
                    insertParameters.Add(new SqlParameter("ActiveLoginID", activeLoginID));
                    insertParameters.Add(new SqlParameter("ItemType", itemType));
                    insertParameters.Add(new SqlParameter("ItemID", itemID));

                    DBAccess db = new DBAccess(DatabaseName.ChangeLog);
                    DBAccessIntResult intResult = db.ExecuteWithIntReturn("ChangeLog_Insert", insertParameters);

                    if (intResult.Success)
                    {
                        List<SqlParameter> itemParameters = new List<SqlParameter>();
                        itemParameters.Add(new SqlParameter("FieldName", "Value"));
                        itemParameters.Add(new SqlParameter("Before", before));
                        itemParameters.Add(new SqlParameter("After", after));
                        itemParameters.Add(new SqlParameter("ChangeLogID", intResult.Value));

                        db.ExecuteNonQuery("ChangeLogItem_Insert", itemParameters);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "ChangeLogManager");
            }
        }

        public static void LogChange(int activeLoginID, string itemType, int itemID, int loginID, List<SqlParameter> parameters, DataRow rowFromLoad, string name = "")
        {
            if (activeLoginID == 0)
            {
                activeLoginID = HttpContextCurrentItemsHelper.GetItemValue("ActiveLoginID", 0);
            }

            if (activeLoginID == 0 || rowFromLoad == null)
            {
                return;
            }

            try
            {
                // Check for changes before inserting
                List<List<SqlParameter>> itemChanges = new List<List<SqlParameter>>();

                foreach (DataColumn column in rowFromLoad.Table.Columns)
                {
                    string originalValue = GetValue(rowFromLoad[column.ColumnName].ToString());

                    SqlParameter newParameter = parameters.FirstOrDefault(o => o.ParameterName == column.ColumnName);
                    if (newParameter != null && newParameter.Value != null)
                    {
                        string newValue = GetValue(newParameter.Value.ToString());

                        if (originalValue != newValue)
                        {
                            List<SqlParameter> itemParameters = new List<SqlParameter>();
                            itemParameters.Add(new SqlParameter("FieldName", column.ColumnName));
                            itemParameters.Add(new SqlParameter("Before", originalValue));
                            itemParameters.Add(new SqlParameter("After", newValue));

                            itemChanges.Add(itemParameters);
                        }
                    }
                }

                // If there are changes, add the log
                if (itemChanges.Count > 0)
                {
                    if (name == "")
                    {
                        SqlParameter parameter = parameters.FirstOrDefault(o => o.ParameterName == "Name");
                        if (parameter != null && parameter.Value != null)
                        {
                            name = parameter.Value.ToString().Trim();
                        }
                    }

                    List<SqlParameter> insertParameters = new List<SqlParameter>();
                    insertParameters.Add(new SqlParameter("LoginID", loginID));
                    insertParameters.Add(new SqlParameter("ActiveLoginID", activeLoginID));
                    insertParameters.Add(new SqlParameter("ItemType", itemType));
                    insertParameters.Add(new SqlParameter("ItemID", itemID));
                    insertParameters.Add(new SqlParameter("Name", name));

                    DBAccess db = new DBAccess(DatabaseName.ChangeLog);
                    DBAccessIntResult intResult = db.ExecuteWithIntReturn("ChangeLog_Insert", insertParameters);

                    if (intResult.Success)
                    {
                        foreach(List<SqlParameter> parametersGroup in itemChanges)
                        {
                            parametersGroup.Add(new SqlParameter("ChangeLogID", intResult.Value));

                            db.ExecuteNonQuery("ChangeLogItem_Insert", parametersGroup);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "ChangeLogManager");
            }
        }

        public static int GetLoginIDFromEstimate(int estimateID)
        {
            if (!_estimatesToLogins.ContainsKey(estimateID))
            {
                Estimate estimate = new Estimate(estimateID);
                if (estimate != null)
                {
                    _estimatesToLogins.Add(estimateID, estimate.CreatedByLoginID);
                }
            }

            if (_estimatesToLogins.ContainsKey(estimateID))
            {
                return _estimatesToLogins[estimateID];
            }

            return 0;
        }

        public static List<string> GetAllChangeLogItemTypes()
        {
            List<string> itemTypes = new List<string>();

            DBAccess db = new DBAccess(DatabaseName.ChangeLog);

            DBAccessTableResult tableResult = db.ExecuteWithTable("ChangeLog_GetAllTypes");

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                string itemType = InputHelper.GetString(row["ItemType"].ToString());
                itemTypes.Add(itemType);
            }

            return itemTypes;
        }

        private static Dictionary<int, int> _estimatesToLogins = new Dictionary<int, int>();
    }
}
