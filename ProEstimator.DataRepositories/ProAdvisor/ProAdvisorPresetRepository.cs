using ProEstimatorData.DataModel.ProAdvisor;
using ProEstimatorData.DataModel;
using ProEstimatorData;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace ProEstimator.DataRepositories.ProAdvisor
{
    public class ProAdvisorPresetRepository : IProAdvisorPresetRepository
    {

        private IProAdvisorPresetShellRepository _shellRepository;

        public ProAdvisorPresetRepository(IProAdvisorPresetShellRepository shellRepository)
        {
            _shellRepository = shellRepository;
        }

        public FunctionResult Save(ProAdvisorPreset preset, int activeLoginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", preset.ID));
            parameters.Add(new SqlParameter("ProfileID", preset.ProfileID));
            parameters.Add(new SqlParameter("PresetShellID", preset.PresetShell == null ? 0 : preset.PresetShell.ID));
            parameters.Add(new SqlParameter("Labor", preset.Labor));
            parameters.Add(new SqlParameter("Refinish", preset.Refinish));
            parameters.Add(new SqlParameter("Charge", preset.Charge));
            parameters.Add(new SqlParameter("OtherTypeOverride", preset.OtherTypeOverride));
            parameters.Add(new SqlParameter("OtherCharge", preset.OtherCharge));
            parameters.Add(new SqlParameter("Active", preset.Active));
            parameters.Add(new SqlParameter("AutoSelect", preset.AutoSelect));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("AddOnPreset_Save", parameters);

            if (intResult.Success)
            {
                ((IIDSetter)preset).ID = intResult.Value;

                // TODO
                //ProAdvisorPresetProfile profile = ProAdvisorPresetProfile.Get(preset.ProfileID);
                //ChangeLogManager.LogChange(activeLoginID, "AddOnPreset", preset.ID, profile.LoginID, parameters, preset.RowAsLoaded, profile.Name + " " + preset.PresetShell.Name);
            }

            return new FunctionResult(intResult.Success, intResult.ErrorMessage);
        }

        public ProAdvisorPreset Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("AddOnPreset_Get", new SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return Initialize(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public List<ProAdvisorPreset> GetForRateProfile(int profileID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("AddOnPreset_GetForProfile", new SqlParameter("ProfileID", profileID));

            List<ProAdvisorPreset> returnList = new List<ProAdvisorPreset>();

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                returnList.Add(Initialize(row));
            }

            return returnList;
        }

        public List<ProAdvisorPreset> GetForRuleAndProfile(int estimateID, int ruleID, int profileID, string action)
        {
            List<ProAdvisorPreset> returnList = new List<ProAdvisorPreset>();

            if (string.IsNullOrEmpty(action))
            {
                return returnList;
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ProfileID", profileID));
            parameters.Add(new SqlParameter("RuleID", ruleID));
            parameters.Add(new SqlParameter("EstimateID", estimateID));
            parameters.Add(new SqlParameter("Action", action));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("AddOnPreset_GetForProfileAndLinkRule", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                returnList.Add(Initialize(row));
            }

            return returnList;
        }

        private ProAdvisorPreset Initialize(DataRow row)
        {
            ProAdvisorPreset preset = new ProAdvisorPreset();

            ((IIDSetter)preset).ID = InputHelper.GetInteger(row["ID"].ToString());
            preset.ProfileID = InputHelper.GetInteger(row["ProfileID"].ToString());
            preset.PresetShell = _shellRepository.Get(InputHelper.GetInteger(row["PresetShellID"].ToString()));
            preset.Labor = InputHelper.GetDouble(row["Labor"].ToString());
            preset.Refinish = InputHelper.GetDouble(row["Refinish"].ToString());
            preset.Charge = InputHelper.GetDouble(row["Charge"].ToString());
            preset.OtherTypeOverride = InputHelper.GetString(row["OtherTypeOverride"].ToString());
            preset.OtherCharge = InputHelper.GetDouble(row["OtherCharge"].ToString());
            preset.Active = InputHelper.GetBoolean(row["Active"].ToString());
            preset.AutoSelect = InputHelper.GetBoolean(row["AutoSelect"].ToString());

            preset.RowAsLoaded = row;

            return preset;
        }
    }
}
