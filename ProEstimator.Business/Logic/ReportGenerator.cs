using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.Models;
using ProEstimatorData.DataModel.Profiles;

using Telerik.Reporting;
using Telerik.Reporting.Processing;
using ProEstimator.Business.Model;
using System.Web.Mvc;

namespace ProEstimator.Business.Logic
{

    public class ReportGenerator
    {

        public async Task<ReportFunctionResult> GenerateReport(int activeLoginID, Estimate estimate, string reportType, string language, string extraParam)
        {
            PrintSettings printSettings = PrintSettings.GetForProfile(estimate.CustomerProfileID);

            List<Telerik.Reporting.Parameter> parameters = new List<Telerik.Reporting.Parameter>();
            parameters.Add(new Telerik.Reporting.Parameter("ContentDirectory", ConfigurationManager.AppSettings.Get("UserContentPath").ToString().Replace(@"\", "/")));

            DateTime timeStamp = DateTime.Now.AddHours(InputHelper.GetInteger(SiteSettings.Get(estimate.CreatedByLoginID, "TimeZone", "ReportOptions", "0").ValueString));
            parameters.Add(new Telerik.Reporting.Parameter("TimeStamp", timeStamp.ToShortDateString() + " " + timeStamp.ToShortTimeString()));

            ProEstimatorData.DataModel.Customer customer = ProEstimatorData.DataModel.Customer.Get(estimate.CustomerID);
            string customerName = "";
            if (customer != null)
            {
                customerName = customer.Contact.FirstName.Replace("&", "&amp;") + " " + customer.Contact.LastName.Replace("&", "&amp;");
            }
            bool onlyCheckedImages = false;

            if (reportType == "Estimate" || reportType == "Estimate_Images")
            {
                // Estimate extraParams:
                // hideHourlyRates:estimateDiscription:supplementVersion:reportHeader:includeImages:onlyIncludeCheckedImages:onlyIncludeSupplementImages
                string[] extraParams = extraParam.Split(":".ToCharArray());

                bool hideHourlyRates = InputHelper.GetBoolean(extraParams[0]);
                string estimateDescription = extraParams[1];
                int supplementVersion = InputHelper.GetInteger(extraParams[2], -1);
                string reportHeader = extraParams[3];
                bool includeImages = InputHelper.GetBoolean(extraParams[4]);
                onlyCheckedImages = InputHelper.GetBoolean(extraParams[5]);
                bool onlySupplementImages = InputHelper.GetBoolean(extraParams[6]);
                bool PdrOnly = InputHelper.GetBoolean(extraParams[7]);
                bool PrintPnLNotes = InputHelper.GetBoolean(extraParams[8]);
                bool PrintInspectionDate = InputHelper.GetBoolean(extraParams[9]);
                bool AttachImagesWithEmail = InputHelper.GetBoolean(extraParams[10]);
                bool PrintSupplementPhotosOnly = InputHelper.GetBoolean(extraParams[11]);
                bool printHeaderInfoOnEstimateReport = InputHelper.GetBoolean(extraParams[12]);
                bool PrintRepairDays = InputHelper.GetBoolean(extraParams[13]);
                bool IncludeLoanApplicationLink = InputHelper.GetBoolean(extraParams[14]);
                
                string selectedTechnicians = "";

                if (extraParams.Length > 15)
                {
                    string selectedTechniciansToSplit = extraParams[15]; // '~' + laborTypeText + '!' + technicianName

                    if (selectedTechniciansToSplit.Length > 1)
                    {
                        //selectedTechnicians = selectedTechnicians.Substring(1, selectedTechnicians.Length - 1);
                        //selectedTechnicians = selectedTechnicians.Replace("~", " ; ");
                        //selectedTechnicians = selectedTechnicians.Replace("!", " : ");

                        char splitCharTild = '~';
                        char splitCharPipe = '!';
                        string[] selectedTechniciansTildArr = selectedTechniciansToSplit.Split(splitCharTild);

                        foreach (string eachSelectedTechnician in selectedTechniciansTildArr)
                        {
                            string[] selectedTechniciansPipeArr = eachSelectedTechnician.Split(splitCharPipe);

                            if (selectedTechniciansPipeArr.Length >= 3)
                            {
                                if (!string.IsNullOrEmpty(InputHelper.GetString(selectedTechniciansPipeArr[3])))
                                {
                                    selectedTechnicians = selectedTechnicians + InputHelper.GetString(selectedTechniciansPipeArr[1]); // laborTypeText
                                    selectedTechnicians = selectedTechnicians + " : " + InputHelper.GetString(selectedTechniciansPipeArr[3]) + " <br /> "; // technicianName
                                }
                            }
                        }
                    }
                }

                estimate.Description = estimateDescription;
                estimate.ReportTextHeader = reportHeader;
                estimate.Save(activeLoginID);

                EstimateService estimateService = new EstimateService(null);
                ProEstimatorData.DataModel.Report estimateReport = await estimateService.CreateEstimateReport(estimate.EstimateID, reportType, estimate.CreatedByLoginID, activeLoginID, hideHourlyRates, supplementVersion, includeImages, onlySupplementImages, onlyCheckedImages, PdrOnly, PrintPnLNotes, PrintInspectionDate, AttachImagesWithEmail, PrintSupplementPhotosOnly, printHeaderInfoOnEstimateReport, PrintRepairDays, IncludeLoanApplicationLink, selectedTechnicians);

                return new ReportFunctionResult(estimateReport);
            }
            else if (reportType == "AuthorizationLetter")
            {
                parameters.Add(new Telerik.Reporting.Parameter("DateString", timeStamp.ToLongDateString() + ", " + timeStamp.ToShortTimeString()));
                parameters.Add(new Telerik.Reporting.Parameter("GrandTotal", InputHelper.GetDouble(estimate.GrandTotalString)));
                parameters.Add(new Telerik.Reporting.Parameter("ShowEstimateNumber", printSettings.EstimateNumber));
                parameters.Add(new Telerik.Reporting.Parameter("ShowID", printSettings.AdminInfoID));

                var customerInfo = new StringBuilder();
                if (customer != null && InputHelper.GetBoolean(extraParam))
                {
                    if (!string.IsNullOrEmpty(customer.Address?.Line1))
                        customerInfo.Append($"{customer.Address.Line1}<br />");
                    if (!string.IsNullOrEmpty(customer.Address?.Line2))
                        customerInfo.Append($"{customer.Address.Line2}<br />");
                    if (!string.IsNullOrEmpty(customer.Address?.City) || !string.IsNullOrEmpty(customer.Address?.State) || !string.IsNullOrEmpty(customer.Address?.Zip))
                        customerInfo.Append($"{customer.Address.City}, {customer.Address.State} {customer.Address.Zip}<br />");
                    if (!string.IsNullOrEmpty(customer.Contact?.Email))
                        customerInfo.Append($"Primary Email: {customer.Contact.Email}<br />");
                    if (!string.IsNullOrEmpty(customer.Contact?.SecondaryEmail))
                        customerInfo.Append($"Secondary Email: {customer.Contact.SecondaryEmail}<br />");
                    if (!string.IsNullOrEmpty(customer.Contact?.Phone))
                    {
                        var phoneType = PhoneType.GetByCode(customer.Contact.PhoneNumberType1);
                        customerInfo.Append($"{phoneType?.ScreenDisplay ?? "Phone"}: {InputHelper.FormatPhone(customer.Contact.Phone)}<br />");
                    }
                    if (!string.IsNullOrEmpty(customer.Contact?.Phone2))
                    {
                        var phoneType = PhoneType.GetByCode(customer.Contact.PhoneNumberType2);
                        customerInfo.Append($"{phoneType?.ScreenDisplay ?? "Phone"}: {InputHelper.FormatPhone(customer.Contact.Phone2)}<br />");
                    }
                    if (!string.IsNullOrEmpty(customer.Contact?.Phone3))
                    {
                        var phoneType = PhoneType.GetByCode(customer.Contact.PhoneNumberType3);
                        customerInfo.Append($"{phoneType?.ScreenDisplay ?? "Phone"}: {InputHelper.FormatPhone(customer.Contact.Phone3)}<br />");
                    }

                    customerInfo.Append("<br />");
                }

                if (language == "en")
                {
                    parameters.Add(new Telerik.Reporting.Parameter("TextCustomer", "Customer: " + customerName));
                    parameters.Add(new Telerik.Reporting.Parameter("TextGrandTotal", "Grand Total"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextMessage", $"{customerInfo}This agreement authorizes the body shop to perform the needed repairs to your vehicle. Needed repairs include parts, labor, paint, and diagnosis. After further inspection, if it is deemed that additional repairs are needed, you and your insurance company will be contacted for further authorization. The Body Shop assumes no responsibility for any damage caused by theft, fire, or any cause beyond the body shop's control. Any diagnostic made by our employees is done at your risk. If it is deemed that the Body Shop shall not complete any repairs and return the vehicle, a diagnostic charge may still be applied to the bill."));
                    parameters.Add(new Telerik.Reporting.Parameter("TextOral", "Oral (check)"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextOther", "Other (check)"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextSignature", "Authorized Signature"));
                }
                else
                {
                    parameters.Add(new Telerik.Reporting.Parameter("TextCustomer", "Cliente: " + customerName));
                    parameters.Add(new Telerik.Reporting.Parameter("TextGrandTotal", "Grand Total"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextMessage", $@"{customerInfo}Este acuerdo autoriza al taller de carrocerías realizar las reparaciones a su vehículo. Las reparaciones incluyen piezas, mano de obra, pintura y diagnóstico. Después, se le realizara otra inspección a su vehiculó y si se considera que se necesitan reparaciones adicionales, usted y su compañía de seguros serán contactados para autorización adicional. El taller de carrocería no asume ninguna responsabilidad por los daños causados por robo, incendio o cualquier causa más allá del control del taller de carrocería. Cualquier diagnóstico realizado por nuestros empleados se realiza bajo su riesgo. Si se considera que el taller de carrocería no deberá completar cualquier reparación y devolver el vehículo, un cargo de diagnóstico puede ser aplicado a su factura de cargos."));
                    parameters.Add(new Telerik.Reporting.Parameter("TextOral", "Oral (check)"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextOther", "Other (check)"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextSignature", "Firma de autorización"));
                }
            }
            else if (reportType == "CustomerLetter")
            {
                Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimate.EstimateID);
                LoginInfo loginInfo = LoginInfo.GetByID(estimate.CreatedByLoginID);

                parameters.Add(new Telerik.Reporting.Parameter("DateLong", timeStamp.ToLongDateString()));
                parameters.Add(new Telerik.Reporting.Parameter("DateShort", timeStamp.ToShortDateString()));
                parameters.Add(new Telerik.Reporting.Parameter("GrandTotal", InputHelper.GetDouble(estimate.GrandTotalString)));
                parameters.Add(new Telerik.Reporting.Parameter("JobNumber", estimate.EstimateID.ToString()));
                parameters.Add(new Telerik.Reporting.Parameter("MilesIn", (double)vehicle.MileageIn));
                parameters.Add(new Telerik.Reporting.Parameter("MilesOut", (double)vehicle.MileageOut));
                parameters.Add(new Telerik.Reporting.Parameter("CompanyName", loginInfo.CompanyName));
            }
            else if (reportType == "DealerLetter")
            {
                Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimate.EstimateID);

                parameters.Add(new Telerik.Reporting.Parameter("DateLong", timeStamp.ToLongDateString()));
                parameters.Add(new Telerik.Reporting.Parameter("DateShort", timeStamp.ToShortDateString()));
                parameters.Add(new Telerik.Reporting.Parameter("GrandTotal", InputHelper.GetDouble(estimate.GrandTotalString)));
                parameters.Add(new Telerik.Reporting.Parameter("JobNumber", estimate.EstimateID.ToString()));
                parameters.Add(new Telerik.Reporting.Parameter("MilesIn", (double)vehicle.MileageIn));
                parameters.Add(new Telerik.Reporting.Parameter("MilesOut", (double)vehicle.MileageOut));
            }
            else if (reportType == "FollowUpLetter")
            {
                parameters.Add(new Telerik.Reporting.Parameter("DateString", timeStamp.ToLongDateString()));
                parameters.Add(new Telerik.Reporting.Parameter("GrandTotal", InputHelper.GetDouble(estimate.GrandTotalString)));
            }
            else if (reportType == "EstimateApproval")
            {
                parameters.Add(new Telerik.Reporting.Parameter("ShowEstimateNumber", printSettings.EstimateNumber));
                parameters.Add(new Telerik.Reporting.Parameter("ShowID", printSettings.AdminInfoID));

                if (language == "es")
                {
                    reportType = "EstimateApprovalES";
                }
            }
            else if (reportType == "DirectionOfPaymentLetter")
            {
                parameters.Add(new Telerik.Reporting.Parameter("DateString", timeStamp.ToLongDateString()));
                parameters.Add(new Telerik.Reporting.Parameter("ShowEstimateNumber", printSettings.EstimateNumber));
                parameters.Add(new Telerik.Reporting.Parameter("ShowID", printSettings.AdminInfoID));

                string[] extraParams = extraParam.Split(":".ToCharArray());

                parameters.Add(new Telerik.Reporting.Parameter("PrintInspectionDate", InputHelper.GetBoolean(extraParams[0])));
                parameters.Add(new Telerik.Reporting.Parameter("PrintRepairDays", InputHelper.GetBoolean(extraParams[1])));
                parameters.Add(new Telerik.Reporting.Parameter("Supplement", estimate.GetSupplementForReport()));

                if (language == "en")
                {
                    parameters.Add(new Telerik.Reporting.Parameter("TextCustomer", "Customer: " + customerName));
                    parameters.Add(new Telerik.Reporting.Parameter("TextDate", "Date:"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextMessageHeader", "**POWER OF ATTORNEY**"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextMessage", "The undersigned hereby grants Power Of Attorney to the shop named above for the purpose of signing in my/our absence and any check or draft made to me/us in the benefit for the cost of repairs or portion thereof, as authorized by our signatures. I direct the paying insurance company to pay the repair shop direct for the full amount due for the repairs as detailed by the repair shop's estimate or supplement invoice, or by the adjuster's report, when an agreement has been made with the repair shop to accept the amount as determined by the adjuster."));
                    parameters.Add(new Telerik.Reporting.Parameter("TextSignature", "Authorized Signature:"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextName", "Name (please print):"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextClaimNumber", "Claim # "));
                    parameters.Add(new Telerik.Reporting.Parameter("TextPolicyNumber", "Policy #"));
                }
                else
                {
                    parameters.Add(new Telerik.Reporting.Parameter("TextCustomer", "Cliente: " + customerName));
                    parameters.Add(new Telerik.Reporting.Parameter("TextDate", "Fecha:"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextMessageHeader", "**Carta Poder**"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextMessage", @"Los firmantes le otorgan el poder al negocio cuyo nombre aparece en la parte superior con el fin de firmar en mi ausencia o nuestra ausencia cualquier cheque o tipo de pago hecho a mí o el taller de reparaciones en beneficio de los gastos de reparaciones o parte del mismo, como autorizado por nuestras firmas. Dirijo a la compañía de seguros responsable de pagar el neto total en la factura de acuerdo al estimado o suplemento realizado por el taller o el reporte del ajustador, cuando se ha llegado a un acuerdo con el taller de reparaciones a aceptar la cantidad determinada por el ajustador o regulador."));
                    parameters.Add(new Telerik.Reporting.Parameter("TextSignature", "Firma de Autorización:"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextName", "Nombre:"));
                    parameters.Add(new Telerik.Reporting.Parameter("TextClaimNumber", "Reclamación # "));
                    parameters.Add(new Telerik.Reporting.Parameter("TextPolicyNumber", "Póliza #"));
                }
            }
            else if (reportType == "ThankYouLetter")
            {
                parameters.Add(new Telerik.Reporting.Parameter("DateString", timeStamp.ToLongDateString()));

                if (language == "en")
                {
                    parameters.Add(new Telerik.Reporting.Parameter("TextCustomer", "Dear " + customerName));
                    parameters.Add(new Telerik.Reporting.Parameter("TextMessage", "We would like to thank you for your trust in allowing us to perform the repairs to your vehicle. We assure you that we took the utmost care and time to repair your vehicle to its pre-accident condition, and we will stand by our repairs. Our number one goal is to make sure the customer is happy with our work, knowing we did the highest quality work on their vehicle. And, if a problem should arise, we will rectify it immediately. If you have any concerns, questions or comments, please feel free to call us personally. <br /><br />Once again, thank you for allowing us the opportunity to serve you.<br /><br />Sincerely,"));
                }
                else
                {
                    parameters.Add(new Telerik.Reporting.Parameter("TextCustomer", "Estimado: " + customerName));
                    parameters.Add(new Telerik.Reporting.Parameter("TextMessage", @"Agradecemos el avernos dado la oportunidad de evaluar y realizar un estimado a su vehículo. Nos enorgullece ofrecer la mejor calidad en reparaciones y servicio al cliente. Para asegurarnos que usted está recibiendo el mejor servicio de colisión nosotros utilizamos equipo y materiales de primera calidad. Gracias nuevamente por la oportunidad de servirle a usted y no dude en contactarnos con cualquier pregunta. Sinceramente,"));
                }
            }
            else if (reportType == "FinalReport")
            {
                parameters.Add(new Telerik.Reporting.Parameter("DateString", timeStamp.ToLongDateString()));
                parameters.Add(new Telerik.Reporting.Parameter("ShowEstimateNumber", printSettings.EstimateNumber));
                parameters.Add(new Telerik.Reporting.Parameter("ShowID", printSettings.AdminInfoID));

                double total = 0;
                double deductable = 0;

                DBAccess db = new DBAccess();
                DBAccessTableResult table = db.ExecuteWithTable("GetEstimateTotalLines", new SqlParameter("AdminInfoID", estimate.EstimateID));
                if (table.Success)
                {
                    foreach (DataRow row in table.DataTable.Rows)
                    {
                        string lineDescription = row["LineDesc"].ToString();

                        if (lineDescription == "Grand Total")
                        {
                            total = InputHelper.GetDouble(row["Total"].ToString());
                        }
                        else if (lineDescription == "Less deductible")
                        {
                            deductable = InputHelper.GetDouble(row["Total"].ToString());
                        }
                    }
                }

                total -= deductable;

                // Set the Current Amount on the report to the total minus any payments made
                var totalMinusPayments = total;
                var payments = PaymentInfoData.GetAllPaymentInfo(estimate.EstimateID);
                if (payments != null)
                {
                    totalMinusPayments -= (double)payments.Sum(x => x.Amount);
                }

                parameters.Add(new Telerik.Reporting.Parameter("CurrentAmount", (float)totalMinusPayments));
                parameters.Add(new Telerik.Reporting.Parameter("RepairOrderAmount", (float)total));
                parameters.Add(new Telerik.Reporting.Parameter("SupplementAmount", 0));
                parameters.Add(new Telerik.Reporting.Parameter("TotalAmount", (float)total));
            }
            else if (reportType == "WorkOrderReport")
            {
                string[] extraParams = extraParam.Split(":".ToCharArray());
                bool showLaborTimes = InputHelper.GetBoolean(extraParams[4]);

                parameters.Add(new Telerik.Reporting.Parameter("LaborTypeTechnicianMapping", InputHelper.GetString(extraParams[0])));
                parameters.Add(new Telerik.Reporting.Parameter("HideCustomerData", InputHelper.GetBoolean(extraParams[1])));
                parameters.Add(new Telerik.Reporting.Parameter("ShowLabor", showLaborTimes));
                parameters.Add(new Telerik.Reporting.Parameter("PrintPnLNotes", InputHelper.GetBoolean(extraParams[2])));
                parameters.Add(new Telerik.Reporting.Parameter("PrintInspectionDate", InputHelper.GetBoolean(extraParams[3])));
                parameters.Add(new Telerik.Reporting.Parameter("PrintRepairDays", InputHelper.GetBoolean(extraParams[5])));
                parameters.Add(new Telerik.Reporting.Parameter("PrintLaborDiscount", InputHelper.GetBoolean(extraParams[6])));
                parameters.Add(new Telerik.Reporting.Parameter("Supplement", estimate.GetSupplementForReport()));
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeHeaders", InputHelper.GetInteger(SiteSettings.Get(estimate.CreatedByLoginID, "FontSizeHeaders", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeTotals", InputHelper.GetInteger(SiteSettings.Get(estimate.CreatedByLoginID, "FontSizeTotals", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeTableHeaders", InputHelper.GetInteger(SiteSettings.Get(estimate.CreatedByLoginID, "FontSizeTableHeaders", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));

                // Save the "print labor hours" setting
                LoginInfo loginInfo = LoginInfo.GetByID(estimate.CreatedByLoginID);
                if (loginInfo.ShowLaborTimeWO != showLaborTimes)
                {
                    loginInfo.ShowLaborTimeWO = showLaborTimes;
                    loginInfo.Save(activeLoginID);
                }

            }
            else if (reportType == "PDRWorkOrderReport")
            {
                string[] extraParams = extraParam.Split(":".ToCharArray());

                parameters.Add(new Telerik.Reporting.Parameter("HideCustomerData", InputHelper.GetBoolean(extraParams[0])));
                parameters.Add(new Telerik.Reporting.Parameter("PrintInspectionDate", InputHelper.GetBoolean(extraParams[1])));
                parameters.Add(new Telerik.Reporting.Parameter("PrintRepairDays", InputHelper.GetBoolean(extraParams[2])));
                parameters.Add(new Telerik.Reporting.Parameter("Technician", InputHelper.GetString(extraParams[3])));
                parameters.Add(new Telerik.Reporting.Parameter("Supplement", estimate.GetSupplementForReport()));
            }
            else if (reportType == "PartsOrder")
            {
                LoginInfo loginInfo = LoginInfo.GetByID(estimate.CreatedByLoginID);

                string[] extraParams = extraParam.Split(":".ToCharArray());

                parameters.Add(new Telerik.Reporting.Parameter("PartTypeFilter", extraParams[0]));
                parameters.Add(new Telerik.Reporting.Parameter("SupplementVersion", InputHelper.GetInteger(extraParams[1])));
                parameters.Add(new Telerik.Reporting.Parameter("FedTaxID", (loginInfo == null || string.IsNullOrEmpty(loginInfo.FederalTaxID)) ? string.Empty : loginInfo.FederalTaxID));
                parameters.Add(new Telerik.Reporting.Parameter("RepairOrderNumber", estimate.WorkOrderNumber.ToString()));
                parameters.Add(new Telerik.Reporting.Parameter("ReportHeader", extraParams[2]));
                parameters.Add(new Telerik.Reporting.Parameter("ShowVendors", printSettings.PrintVendors));
            }
            else if (reportType == "RepairNotesReport")
            {
                LoginInfo loginInfo = LoginInfo.GetByID(estimate.CreatedByLoginID);

                parameters.Add(new Telerik.Reporting.Parameter("HeaderText", string.IsNullOrEmpty(estimate.ReportTextHeader) ? "Estimate" : estimate.ReportTextHeader));
                parameters.Add(new Telerik.Reporting.Parameter("EstimatorOrAppraiser", loginInfo.Appraiser ? "Appraiser" : "Estimator"));

                int loginID = estimate.CreatedByLoginID;

                // Add font size setting values
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeDetails", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeDetails", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeHeader", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeHeader", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeHeaders", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeHeaders", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeLines", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeLines", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeTableHeaders", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeTableHeaders", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeTotals", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeTotals", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));

                parameters.Add(new Telerik.Reporting.Parameter("PrintShopInfo", InputHelper.GetBoolean(SiteSettings.Get(loginID, "PrintShopInfo", "ReportOptions", true.ToString()).ValueString)));

                // Get the rate profile for the current estimate to pass parameter values
                if (printSettings != null)
                {
                    bool showPhotosSetting = !printSettings.NoPhotos;
                    bool showDPFs = showPhotosSetting;

                    parameters.Add(new Telerik.Reporting.Parameter("ShowHeaderLogo", !printSettings.NoHeaderLogo));
                    parameters.Add(new Telerik.Reporting.Parameter("ShowInsuranceSection", !printSettings.NoInsuranceSection));
                    parameters.Add(new Telerik.Reporting.Parameter("ShowEstimateNumber", printSettings.EstimateNumber));
                    parameters.Add(new Telerik.Reporting.Parameter("ShowID", printSettings.AdminInfoID));
                    parameters.Add(new Telerik.Reporting.Parameter("HeaderLabels", printSettings.ContactOption == "Label" ? false : true));
                }
            }

            //parameters.Add(new Telerik.Reporting.Parameter("", ));

            return GenerateReport(estimate.EstimateID, reportType, parameters, false);
        }

        public async Task<ReportFunctionResult> GenerateReport(int loginID, int contractID, string reportType)
        {
            List<Telerik.Reporting.Parameter> parameters = new List<Telerik.Reporting.Parameter>();

            if (reportType == "ContractInvoices")
            {
                parameters.Add(new Telerik.Reporting.Parameter("LoginID", loginID));
                parameters.Add(new Telerik.Reporting.Parameter("ContentDirectory", ConfigurationManager.AppSettings.Get("UserContentPath").ToString().Replace(@"\", "/")));
                parameters.Add(new Telerik.Reporting.Parameter("ContractID", contractID));
                parameters.Add(new Telerik.Reporting.Parameter("IncludeAddons", 1));
                parameters.Add(new Telerik.Reporting.Parameter("HeaderText", "Contract"));

                // Add font size setting values
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeDetails", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeDetails", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeHeader", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeHeader", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeHeaders", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeHeaders", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeLines", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeLines", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeTableHeaders", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeTableHeaders", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
                parameters.Add(new Telerik.Reporting.Parameter("FontSizeTotals", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeTotals", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
            }

            return GenerateReport(contractID, reportType, parameters);
        }

        public ReportFunctionResult GenerateReport(int estimateID, string reportType, List<Telerik.Reporting.Parameter> parameters, Boolean onlyCheckedImages = false)
        {
            // Create a Report record
            ProEstimatorData.DataModel.Report report = new ProEstimatorData.DataModel.Report();
            report.EstimateID = estimateID;
            report.ReportType = ProEstimatorData.DataModel.ReportType.GetAll().FirstOrDefault(o => o.Tag == reportType);
            report.ImagesOnlyChecked = onlyCheckedImages;
            report.FileName = ProEstimatorData.DataModel.Report.GetUniqueReportName(estimateID, reportType);
            SaveResult reportRecordSave = report.Save();

            if (!reportRecordSave.Success)
            {
                return new ReportFunctionResult(reportRecordSave.ErrorMessage);
            }

            // The database record was saved, now create the PDF report

            try
            {
                // Set up a Report Source for the report file
                var uriReportSource = new Telerik.Reporting.UriReportSource();
                uriReportSource.Uri = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", reportType + ".trdx");

                // Pass the report parameters
                uriReportSource.Parameters.Add(new Telerik.Reporting.Parameter("AdminInfoID", estimateID));
                uriReportSource.Parameters.AddRange(parameters);

                // Generate the PDF
                ReportProcessor reportProcessor = new ReportProcessor();
                RenderingResult result = reportProcessor.RenderReport("PDF", uriReportSource, null);

                // Save the report to the disk
                string diskPath = report.GetDiskPath();

                // Make sure the folder exists
                string folderPath = Path.GetDirectoryName(diskPath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (FileStream fs = new FileStream(diskPath, FileMode.Create))
                {
                    fs.Write(result.DocumentBytes, 0, result.DocumentBytes.Length);
                }

                return new ReportFunctionResult(report);
            }
            catch (System.Exception ex)
            {
                return new ReportFunctionResult(ex.Message.Contains("Check the InnerException") ? ex.InnerException.Message : ex.Message);
            }
        }

        public ReportFunctionResult GenerateReport(int contractID, string reportType, List<Telerik.Reporting.Parameter> parameters)
        {
            // The database record was saved, now create the PDF report
            // Create a Report record
            ProEstimatorData.DataModel.Report report = new ProEstimatorData.DataModel.Report();
            report.ContractID = contractID;
            report.LoginID = InputHelper.GetInteger(Convert.ToString(parameters[0].Value));
            report.ReportType = new ReportType("ContractInvoices", "Contract Invoices", false);
            report.FileName = ProEstimatorData.DataModel.Report.GetUniqueContractReportName(contractID, reportType);
            //report.FileName = reportType + "_" + contractID + "_" + Guid.NewGuid();
            SaveResult reportRecordSave = report.Save();

            try
            {
                // Set up a Report Source for the report file
                var uriReportSource = new Telerik.Reporting.UriReportSource();
                uriReportSource.Uri = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", reportType + ".trdx");

                // Pass the report parameters
                uriReportSource.Parameters.AddRange(parameters);

                // Generate the PDF
                ReportProcessor reportProcessor = new ReportProcessor();
                RenderingResult result = reportProcessor.RenderReport("PDF", uriReportSource, null);

                // Save the report to the disk
                string diskPath = report.GetContractReportDiskPath();

                // Make sure the folder exists
                string folderPath = Path.GetDirectoryName(diskPath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (FileStream fs = new FileStream(diskPath, FileMode.Create))
                {
                    fs.Write(result.DocumentBytes, 0, result.DocumentBytes.Length);
                }

                return new ReportFunctionResult(report);
            }
            catch (System.Exception ex)
            {
                return new ReportFunctionResult(ex.Message.Contains("Check the InnerException") ? ex.InnerException.Message : ex.Message);
            }
        }

        public ReportFunctionResult GenerateImagePdfReport(string imagePathURL)
        {
            // Create a Report record

            try
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(imagePathURL);

                // Set up a Report Source for the report file
                var uriReportSource = new Telerik.Reporting.UriReportSource();
                uriReportSource.Uri = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", "PrintImage_PDF.trdx");

                // Pass the report parameters
                uriReportSource.Parameters.Add(new Telerik.Reporting.Parameter("ImagePath", imagePathURL));

                // Generate the PDF
                ReportProcessor reportProcessor = new ReportProcessor();
                RenderingResult result = reportProcessor.RenderReport("PDF", uriReportSource, null);

                // Save the report to the disk
                string diskPath = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "FrameImagePdf", fileNameWithoutExt + ".pdf");

                // Make sure the folder exists
                string folderPath = Path.GetDirectoryName(diskPath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (FileStream fs = new FileStream(diskPath, FileMode.Create))
                {
                    fs.Write(result.DocumentBytes, 0, result.DocumentBytes.Length);
                }

                ReportFunctionResult reportFunctionResult = new ReportFunctionResult();
                reportFunctionResult.ReportFullName = fileNameWithoutExt + ".pdf";

                return reportFunctionResult;
            }
            catch (System.Exception ex)
            {
                return new ReportFunctionResult(ex.Message.Contains("Check the InnerException") ? ex.InnerException.Message : ex.Message);
            }
        }

    }
}
