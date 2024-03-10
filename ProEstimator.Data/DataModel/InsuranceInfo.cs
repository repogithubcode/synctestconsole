using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class InsuranceInfo : ProEstEntity
    {
        public int EstimateID { get; set; }

        public string ClaimNumber { get; set; }
        public int CoverageType { get; set; }
        public DateTime? DateOfLoss { get; set; }
        public decimal Deduction { get; set; }
        public string PolicyNumber { get; set; }
        public bool ClaimantSameAsOwner { get; set; }
        public bool InsuredSameAsOwner { get; set; }
        public string InsuranceCompanyName { get; set; }

        public Contact Agent
        {
            get
            {
                if (_agent == null)
                    _agent = Contact.GetContact(EstimateID, ContactSubType.InsuranceAgent);
                return _agent;
            }
        }

        public Contact Adjuster
        {
            get
            {
                if (_adjuster == null)
                    _adjuster = Contact.GetContact(EstimateID, ContactSubType.Adjuster);
                return _adjuster;
            }
        }

        public Contact ClaimRep
        {
            get
            {
                if (_claimRep == null)
                    _claimRep = Contact.GetContact(EstimateID, ContactSubType.ClaimRepresentative);
                return _claimRep;
            }
        }

        public Contact Claimant
        {
            get
            {
                if (_claimant == null)
                    _claimant = Contact.GetContact(EstimateID, ContactSubType.Claimant);
                return _claimant;
            }
        }

        public Contact Insured
        {
            get
            {
                if (_insured == null)
                    _insured = Contact.GetContact(EstimateID, ContactSubType.Insured);
                return _insured;
            }
        }

        public Address ClaimantAddress
        {
            get
            {
                if (_claimantAddress == null)
                    _claimantAddress = Address.GetForContact(Claimant != null ? Claimant.ContactID : 0);
                    
                return _claimantAddress;
            }
        }

        public Address InsuredAddress
        {
            get
            {
                if (_insuredAddress == null)
                    _insuredAddress = Address.GetForContact(Insured != null ? Insured.ContactID : 0);

                return _insuredAddress;
            }
        }

        private Contact _agent;
        private Contact _adjuster { get; set; }
        private Contact _claimRep { get; set; }
        private Contact _claimant { get; set; }
        private Contact _insured { get; set; }

        private Address _claimantAddress { get; set; }
        private Address _insuredAddress { get; set; }

        public InsuranceInfo()
        {
            CoverageType = 255;
        }

        public InsuranceInfo(DataRow row)
        {
            EstimateID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            ClaimNumber = row["ClaimNumber"].ToString();
            CoverageType = InputHelper.GetInteger(row["CoverageType"].ToString(), 255);

            string dateOfLossString = row["DateOfLoss"].ToString();
            if (!string.IsNullOrEmpty(dateOfLossString))
            {
                DateOfLoss = InputHelper.GetDateTime(dateOfLossString);
            }
            else
            {
                DateOfLoss = null;
            }

            Deduction = InputHelper.GetDecimal(row["Deductible"].ToString());
            PolicyNumber = row["PolicyNumber"].ToString();
            ClaimantSameAsOwner = InputHelper.GetBoolean(row["ClaimantSameAsOwner"].ToString());
            InsuredSameAsOwner = InputHelper.GetBoolean(row["InsuredSameAsOwner"].ToString());
            InsuranceCompanyName = row["InsuranceCompanyName"].ToString();

            RowAsLoaded = row;
        }

        public static InsuranceInfo Get(int estimateID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("GetOtherContactInformation", new SqlParameter("AdminInfoID", estimateID));

            if (tableResult.Success)
            {
                return new InsuranceInfo(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public void Save(int activeLoginID, int estimateID, int loginID)
        {
            if (_agent != null)
                _agent.Save(activeLoginID, loginID);

            if (_adjuster != null)
                _adjuster.Save(activeLoginID, loginID);

            if (_claimRep != null)
                _claimRep.Save(activeLoginID, loginID);

            if (_claimant != null)
                _claimant.Save(activeLoginID, loginID);

            if (_claimantAddress != null)
            {
                _claimantAddress.EstimateID = estimateID;
                _claimantAddress.ContactID = _claimant.ContactID;
                _claimantAddress.Save(activeLoginID, loginID);
            }

            if (_insured != null)
                _insured.Save(activeLoginID, loginID);

            if (_insuredAddress != null)
            {
                _insuredAddress.EstimateID = estimateID;
                _insuredAddress.ContactID = _insured.ContactID;
                _insuredAddress.Save(activeLoginID, loginID);
            } 

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));
            parameters.Add(new SqlParameter("InsuranceCompanyName", string.IsNullOrEmpty(InsuranceCompanyName) ? "" : InsuranceCompanyName));
            parameters.Add(new SqlParameter("InsuranceCompanyName2", string.IsNullOrEmpty(InsuranceCompanyName) ? "" : InsuranceCompanyName));
            parameters.Add(new SqlParameter("PolicyNumber", string.IsNullOrEmpty(PolicyNumber) ? "" : PolicyNumber));
            parameters.Add(new SqlParameter("ClaimNumber", string.IsNullOrEmpty(ClaimNumber) ? "" : ClaimNumber));
            parameters.Add(new SqlParameter("DateOfLoss", DateOfLoss));
            parameters.Add(new SqlParameter("CoverageType", CoverageType));
            parameters.Add(new SqlParameter("LoginsID", loginID));
            parameters.Add(new SqlParameter("ClaimantSameAsOwner", ClaimantSameAsOwner));
            parameters.Add(new SqlParameter("InsuredSameAsOwner", InsuredSameAsOwner));
            parameters.Add(new SqlParameter("Deductible", Deduction));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("UpdateOtherContactInformation", parameters);

            ChangeLogManager.LogChange(activeLoginID, "InsuranceInfo", estimateID, loginID, parameters, RowAsLoaded);
        }
    }
}