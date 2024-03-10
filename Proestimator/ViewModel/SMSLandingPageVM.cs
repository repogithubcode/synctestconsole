using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class SMSLandingPageVM
    {

        public string Subject { get; set; }
        public string MessageBody { get; set; }

        public List<MessageAttachmentMappingVM> Attachments { get; set; }
        public List<ImageAttachmentVM> Images { get; set; }

        public bool IsGood { get; set; }
        public string ErrorMessage { get; set; }

        public SMSLandingPageVM(string code)
        {
            ReportEmail email = ReportEmail.GetForSMSCode(code);
            if (email != null)
            {
                Estimate estimate = new Estimate(email.EstimateID);

                IsGood = true;

                Subject = email.Subject;
                MessageBody = email.Body.Replace("\n", "<br />");

                // Load attachments and fill the list of vms
                Attachments = new List<MessageAttachmentMappingVM>();

                List<MessageAttachmentMapping> mappings = MessageAttachmentMapping.GetByUniqueID(code);
                foreach (MessageAttachmentMapping mapping in mappings)
                {
                    Attachments.Add(new MessageAttachmentMappingVM(mapping));
                }

                // Load images and fill the list of vms
                Images = new List<ImageAttachmentVM>();

                foreach(MessageAttachmentMapping mapping in mappings)
                {
                    if (mapping.ReportType == "Estimate")
                    {
                        List<ImageAttachment> imageAttachments = ImageAttachment.GetForReport(mapping.ReportID);
                        foreach(ImageAttachment imageAttachment in imageAttachments)
                        {
                            string diskPath = imageAttachment.EstimationImage.GetDiskPath(estimate.CreatedByLoginID, true);
                            string thumbPath = imageAttachment.EstimationImage.GetWebPath(estimate.CreatedByLoginID, true);
                            string largePath = imageAttachment.EstimationImage.GetWebPath(estimate.CreatedByLoginID, false);

                            if (System.IO.File.Exists(diskPath))
                            {
                                Images.Add(new ImageAttachmentVM(imageAttachment.EstimationImage.ID, largePath, thumbPath));
                            }
                        }
                    }
                }

                MiscTracking.Insert(estimate.CreatedByLoginID, estimate.EstimateID, "SMSLandingGood", code);
            }
            else
            {
                IsGood = false;
                ErrorMessage = "Invalid code";
                MiscTracking.Insert(0, 0, "SMSLandingBad", code);
            }
        }
    }

    public class ImageAttachmentVM
    {
        public int ImageID { get; set; }
        public string Path { get; set; }
        public string ThumbPath { get; set; }
        public ImageAttachmentVM(int imageID, string path, string thumbPath)
        {
            ImageID = imageID;
            Path = path;
            ThumbPath = thumbPath;
        }
    }
}