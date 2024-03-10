using ProEstimatorData.DataModel.ProAdvisor;
using ProEstimatorData.DataModel;
using ProEstimatorData;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ProEstimator.DataRepositories.ProAdvisor
{
    public class ProAdvisorPresetShellRepository : IProAdvisorPresetShellRepository
    {
        public FunctionResult Save(ProAdvisorPresetShell presetShell, int activeLoginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", presetShell.ID));
            parameters.Add(new SqlParameter("OperationType", presetShell.OperationType));
            parameters.Add(new SqlParameter("Sublet", presetShell.Sublet));
            parameters.Add(new SqlParameter("Name", presetShell.Name));
            parameters.Add(new SqlParameter("LaborType", presetShell.LaborType));
            parameters.Add(new SqlParameter("OtherType", presetShell.OtherType));
            parameters.Add(new SqlParameter("Notes", presetShell.Notes));
            parameters.Add(new SqlParameter("OnePerVehicle", presetShell.OnePerVehicle));
            parameters.Add(new SqlParameter("AccessLevel", presetShell.AccessLevel));
            parameters.Add(new SqlParameter("Deleted", presetShell.Deleted));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOnPresetShell_Save", parameters);

            if (result.Success)
            {
                ((IIDSetter)presetShell).ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "AddOnPresetShell", presetShell.ID, 0, parameters, presetShell.RowAsLoaded);
            }

            return new FunctionResult(result.ErrorMessage);
        }

        public ProAdvisorPresetShell Get(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return GetAll().FirstOrDefault(o => o.ID == id);
        }

        public List<ProAdvisorPresetShell> GetAll()
        {
            List<ProAdvisorPresetShell> presetShells = new List<ProAdvisorPresetShell>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("AddOnPresetShell_GetAll");

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                presetShells.Add(Instantiate(row));
            }

            return presetShells;
        }

        private ProAdvisorPresetShell Instantiate(DataRow row)
        {
            ProAdvisorPresetShell shell = new ProAdvisorPresetShell();

            ((IIDSetter)shell).ID = InputHelper.GetInteger(row["ID"].ToString());
            shell.OperationType = InputHelper.GetString(row["OperationType"].ToString());
            shell.Sublet = InputHelper.GetBoolean(row["Sublet"].ToString());
            shell.Name = InputHelper.GetString(row["Name"].ToString());
            shell.LaborType = InputHelper.GetString(row["LaborType"].ToString());
            shell.OtherType = InputHelper.GetString(row["OtherType"].ToString());
            shell.Notes = InputHelper.GetString(row["Notes"].ToString());
            shell.OnePerVehicle = InputHelper.GetBoolean(row["OnePerVehicle"].ToString());
            shell.AccessLevel = InputHelper.GetInteger(row["AccessLevel"].ToString());
            shell.Deleted = InputHelper.GetBoolean(row["Deleted"].ToString());

            shell.RowAsLoaded = row;

            return shell;
        }
    }
}
