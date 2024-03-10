using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Admin.ViewModel.Contracts;
using ProEstimator.Business.Logic;

namespace Proestimator.Admin.ViewModelMappers.Contracts
{
    public class ContractVMMapper : IVMMapper<ContractVM>
    {
        public ContractVM Map(MappingConfiguration mappingConfiguration)
        {
            ContractVMMapperConfiguration config = mappingConfiguration as ContractVMMapperConfiguration;
            Contract contract = config.Contract;

            ContractVM vm = new ContractVM();

            try
            {
                vm.ErrorMessage = "";
                vm.ContractID = contract.ID;

                vm.ContractPriceLevel = contract.ContractPriceLevel.PriceLevel;
                vm.TermDescription = contract.ContractPriceLevel.ContractTerms.TermDescription;
                vm.Notes = contract.Notes;

                vm.PromoCode = "";
                if (contract.PromoID > 0)
                {
                    PromoCode promoCode = ProEstimatorData.DataModel.Contracts.PromoCode.GetByID(contract.PromoID);
                    if (promoCode != null)
                    {
                        vm.PromoCode = promoCode.Code;
                    }
                }

                vm.AvailablePromoCodes = new List<PromoCodeVM>();

                if (config.IncludeAvailablePromoCodes)
                {
                    List<PromoCode> promoCodes = ProEstimatorData.DataModel.Contracts.PromoCode.GetAvailableForContract(contract.ID).OrderByDescending(o => o.EndDate).ToList();
                    PromoCodeVMMapper mapper = new PromoCodeVMMapper();
                    foreach (PromoCode code in promoCodes)
                    {
                        vm.AvailablePromoCodes.Add(mapper.Map(new PromoCodeVMMapperConfiguration() { PromoCode = code }));
                    }
                }

                vm.CreatedDate = contract.DateCreated.ToLongDateString();
                vm.EffectiveDate = contract.EffectiveDate.ToShortDateString();
                vm.ExpirationDate = contract.ExpirationDate.ToShortDateString();

                ContractDigitalSignature digitalSignature = ContractDigitalSignature.GetForContract(contract.ID);
                if (digitalSignature != null)
                {
                    string diskPath = DigitalSignaturePrintManager.GetDiskPath(digitalSignature);
                    if (System.IO.File.Exists(diskPath))
                    {
                        vm.DigitalSignatureID = digitalSignature.ID;
                    }
                }

                List<ContractAddOn> addOns = ContractAddOn.GetForContract(contract.ID).Where(o => o.HasPayment).ToList();
                vm.HasEMS = addOns.FirstOrDefault(o => o.AddOnType.ID == 5 && o.Active) != null;
                vm.HasMU = addOns.FirstOrDefault(o => o.AddOnType.ID == 8 && o.Active) != null;
                vm.HasFrameData = addOns.FirstOrDefault(o => o.AddOnType.ID == 2 && o.Active) != null;
                vm.HasQB = addOns.FirstOrDefault(o => o.AddOnType.ID == 9 && o.Active) != null;
                vm.HasProAdvisor = addOns.FirstOrDefault(o => o.AddOnType.ID == 10 && o.Active) != null;
                vm.HasCustomReports = addOns.FirstOrDefault(o => o.AddOnType.ID == 13 && o.Active) != null;

                vm.HasImages = addOns.FirstOrDefault(o => o.AddOnType.ID == 11 && o.Active) != null;
                vm.HasBundle = addOns.FirstOrDefault(o => o.AddOnType.ID == 12 && o.Active) != null;

                vm.ShowPickAddonButton = contract.ExpirationDate >= DateTime.Now.Date && (!vm.HasEMS || !vm.HasMU || !vm.HasFrameData || !vm.HasQB || !vm.HasProAdvisor || !vm.HasImages || !vm.HasBundle);

                vm.HasPayment = contract.HasPayment;

                vm.IsSigned = contract.IsSigned;
                vm.IgnoreAutoPay = contract.IgnoreAutoPay;
                vm.EarlyRenewal = contract.EarlyRenewal;
                vm.IsActive = contract.Active;
                vm.IsDeleted = contract.IsDeleted;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, contract.LoginID, "ContractVM load");
            }

            return vm;
        }
    }

    public class ContractVMMapperConfiguration : MappingConfiguration
    {
        public Contract Contract { get; set; }
        public bool IncludeAvailablePromoCodes { get; set; }

    }
}