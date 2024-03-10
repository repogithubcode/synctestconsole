using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.Models;
using ProEstimatorData.DataModel;
using ProEstimatorData;

namespace Proestimator.Helpers
{
    public static class ImageAttachmentHelper
    {
        public static List<string> GetAttachmentPaths(int loginID, List<int> reportIDs, string imagesOnlyChecked = "")
        {
            List<string> attachmentPaths = new List<string>();

            List<ImageAttachment> imageAttachments = GetImageAttachments(reportIDs);

            foreach (ImageAttachment eachImageAttachment in imageAttachments)
            {
                EstimationImage estimationImage = eachImageAttachment.EstimationImage;

                bool useImage = true;

                if (eachImageAttachment.ImagesOnlyChecked && !estimationImage.Include)
                {
                    useImage = false;
                }

                if (useImage)
                {
                    if (estimationImage.FileType == "jpg" || estimationImage.FileType == "jpeg" || estimationImage.FileType == "png")
                    {
                        string editPath = estimationImage.GetEditedDiskPath(loginID, false);
                        if (System.IO.File.Exists(editPath))
                        {
                            attachmentPaths.Add(editPath);
                        }
                        else
                        {
                            attachmentPaths.Add(estimationImage.GetDiskPath(loginID, false));
                        }
                    }
                    else if (estimationImage.FileType == "pdf")
                    {
                        string diskPath = estimationImage.GetDiskPath(loginID, false);

                        if (System.IO.File.Exists(diskPath))
                        {
                            for (int i = 1; i <= estimationImage.PDFPageCount; i++)
                            {
                                string filePath = estimationImage.GetPDFImageEditedDiskPath(loginID, true, i);
                                if (System.IO.File.Exists(filePath))
                                {
                                    attachmentPaths.Add(filePath);
                                }
                                else
                                {
                                    attachmentPaths.Add(estimationImage.GetPDFImageDiskPath(loginID, true, i));
                                }
                            }
                        }
                    }
                }
            }

            return attachmentPaths;
        }

        public static List<ImageAttachment> GetImageAttachments(List<int> reportIDs)
        {
            List<string> attachmentPaths = new List<string>();

            List<ImageAttachment> imageAttachments = new List<ImageAttachment>();
            if (reportIDs != null)
            {
                foreach (int reportID in reportIDs)
                {
                    List<ImageAttachment> imageAttachmentColl = ImageAttachment.GetForReport(reportID);

                    foreach (ImageAttachment imageAttachment in imageAttachmentColl)
                    {
                        ImageAttachment imageAttachmentObj = imageAttachments.Where(eachImageAttachment => eachImageAttachment.EstimationImage.ID
                                                                == imageAttachment.EstimationImage.ID).FirstOrDefault();

                        if (imageAttachmentObj == null)
                        {
                            imageAttachments.Add(imageAttachment);
                        }
                    }
                }
            }

            return imageAttachments;
        }

    }
}