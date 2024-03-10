using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Business.Logic.Admin;

namespace ProEstimator.Business.Logic
{
    public static class DigitalSignaturePrintManager
    {

        public static FunctionResult CreateReport(ContractDigitalSignature digitalSignature)
        {
            // Generate the PDF
            string processedTemplate = digitalSignature.GetContractContent(true);

            // Save the report to the disk
            string diskPath = GetDiskPath(digitalSignature);

            // Make sure the folder exists
            string folderPath = Path.GetDirectoryName(diskPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return HtmlToPdfSaver.SavePdf("License Agreement", diskPath, processedTemplate);
        }

        public static void CreateAndSendReport(ContractDigitalSignature digitalSignature)
        {
            FunctionResult pdfResult = CreateReport(digitalSignature);

            if (pdfResult.Success)
            {
                try
                {
                    Contact contact = Contact.GetContactForLogins(digitalSignature.LoginID);

                    if (contact != null && !string.IsNullOrEmpty(contact.Email))
                    {
                        LoginInfo loginInfo = LoginInfo.GetByID(digitalSignature.LoginID);

                        string emailSubject = "ProEstimator Signed Agreement - ";
                        if (ContractManager.DoesAccountHaveAnyPaidContracts(digitalSignature.LoginID))
                        {
                            emailSubject += "Contract Renewal";
                        }
                        else
                        {
                            emailSubject += "New Contract";
                        }
                        emailSubject += " - " + loginInfo.ID.ToString() + " - " + loginInfo.Organization;

                        SalesRep salesRep = null;

                        if (loginInfo != null)
                        {
                            emailSubject += " - " + loginInfo.ID + " " + loginInfo.Organization;

                            salesRep = SalesRep.Get(loginInfo.SalesRepID);
                        }

                        Email customerCopy = new Email();
                        customerCopy.LoginID = loginInfo.ID;
                        customerCopy.AddToAddress(contact.Email);
                        customerCopy.Subject = emailSubject;
                        customerCopy.AddAttachmentPath(GetDiskPath(digitalSignature));
                        customerCopy.Save(0);
                        EmailSender.SendEmail(customerCopy);

                        Email ourCopy = new Email();
                        ourCopy.LoginID = loginInfo.ID;
                        ourCopy.AddToAddress("bnabors@web-est.com");

                        // Get CC addresses from the permissions
                        List<string> emailAddresses = SalesRepPermissionManager.GetSalesRepEmailAddressForPermissionTag("IsEmailSentSignContract");

                        foreach (string emailAddress in emailAddresses)
                        {
                            ourCopy.AddCCAddress(emailAddress);
                        }

                        if (salesRep != null && !string.IsNullOrEmpty(salesRep.Email))
                        {
                            ourCopy.AddCCAddress(salesRep.Email);
                        }

                        ourCopy.Subject = emailSubject;
                        ourCopy.AddAttachmentPath(GetDiskPath(digitalSignature));
                        ourCopy.Save(0);
                        EmailSender.SendEmail(ourCopy);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, digitalSignature.LoginID, "Send Digital Signature");
                }
            }
        }

        public static bool DoesPrintExist(ContractDigitalSignature digitalSignature)
        {
            string path = GetDiskPath(digitalSignature);
            return File.Exists(path);
        }

        public static string GetDiskPath(ContractDigitalSignature digitalSignature)
        {
            int uniqueID = digitalSignature.ContractID;

            // We used to use the contract ID in the name, but should use the signature ID so we can have multiple PDFs per contract
            string signatureIDSwitch = ConfigurationManager.AppSettings["SignatureIDSwitch"];
            if (!string.IsNullOrEmpty(signatureIDSwitch))
            {
                int idSwitch = InputHelper.GetInteger(signatureIDSwitch);
                if (digitalSignature.ID > idSwitch)
                {
                    uniqueID = digitalSignature.ID;
                }
            }

            return Path.Combine(ConfigurationManager.AppSettings["UserContentPath"], digitalSignature.LoginID.ToString(), "Reports", "Contract-" + uniqueID + ".pdf").Replace("\\", "/");
        }

    }
}