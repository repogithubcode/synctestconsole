using System;
using System.Collections.Generic;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.LkqService;
using service = ProEstimator.Business.LkqService;

namespace ProEstimator.Business.Model
{
    public class QuoteRequest : IMapTo<UserRequest>, IMapTo<Estimate>  // IMapTo<QuoteRequest, UserRequest>, IMapTo<QuoteRequest, Estimate>
    {
        /// <summary>
        /// Required: Aftermarket or Salvage
        /// </summary>
        public AccountNumberBusinessType BusinessType { get; set; }

        /// <summary>
        /// Required: Unique ID per partner per environment. Assigned by LKQ.
        /// </summary>
        public Guid VerificationCode { get; set; }

        /// <summary>
        /// Conditional: Required for insurance quotes
        /// </summary>
        public string ClaimNumber { get; set; }

        /// <summary>
        /// Required
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Required: LKQ has agreements with many major insurers regarding the use of our parts on insurance claims. This field must be populated with one of the matching LKQ insurance company names to ensure these rules are followed. Omitting a name or using one not on the LKQ list will bypass all these rules. See the appendix for a list of these insurance company names. 
        /// </summary>
        public string InsuranceCompanyParent { get; set; }

        /// <summary>
        /// Required:  True for insurance quotes / False for shop (repair facility) quotes
        /// </summary>
        public bool InsuranceQuote { get; set; }
        
        /// <summary>
        /// Required parts fields: AlternatePartNumber, AlternatePartType, EstimateLineID, InterchangeNumber, OEPartNumber, OEPartType, Quantity, ROListPrice
        /// </summary>
        public List<LineItem> Parts { get; set; }

        /// <summary>
        /// Required: assigned by LKQ
        /// </summary>
        public string PartnerKey { get; set; }

        /// <summary>
        /// Conditional: Required for insurance quotes
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Conditional: Required for non-insurance quotes
        /// </summary>
        public string RepairFacility { get; set; }

        /// <summary>
        /// Requires VIN OR Model AND Year
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Requires VIN OR Model AND Year
        /// </summary>
        public string Vin { get; set; }

        /// <summary>
        /// Requires VIN OR Model AND Year
        /// </summary>
        public string Year { get; set; }

        public UserRequest ToModel(UserRequest item)
        {
            var result = new UserRequest();
            var userRequest = new UserInformation();
            userRequest.AccountNumber = PartnerKey;
            userRequest.BusinessTypeForAccountNumber = BusinessType;
            userRequest.VerificationCode = VerificationCode;

            result.UserRequestInfo = userRequest;

            return result;
        }

        public Estimate ToModel(Estimate item)
        {
            var result = new Estimate();
            result.ClaimNumber = ClaimNumber;
            result.CreatedDate = CreatedDate;
            result.InsuranceCompanyParent = InsuranceCompanyParent;
            result.IsInsuranceQuote = InsuranceQuote;
            result.LineItems = Parts;
            result.PartnerKey = PartnerKey;
            result.PostalCode = PostalCode;
            result.RepairFacilityId = RepairFacility;

            var vehicle = new Vehicle();
            vehicle.Year = Year;
            vehicle.Model = Model;
            vehicle.VIN = Vin;

            result.Vehicle = vehicle;

            return result;
        }
    }
}