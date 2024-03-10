using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class ImagesVM
    {
        public int LoginID { get; set; }
        public int EstimateID { get; set; }
        public bool HasImagesContract { get; set; }
        public List<ImageVM> Images { get; set; }

        public List<SelectListItem> ImageTags { get; set; }
        public List<SelectListItem> SupplementVersions { get; set; }

        public string ErrorMessage { get; set; }

        public int UserID { get; set; }

        public bool IsModelPopup { get; set; }

        public bool EditorIsTrial { get; set; }

        public bool EstimateIsLocked { get; set; }
        public ImagesVM(int supplementVersion)
        {
            Images = new List<ImageVM>();

            List<ImageTag> imageTags = ImageTag.GetAll();

            ImageTags = new List<SelectListItem>();
            ImageTags.Add(new SelectListItem() { Text = "-----Select Tag-----", Value = "0" });
            if (imageTags != null && imageTags.Count > 0)
            {
                foreach (ImageTag imageTag in imageTags)
                {
                    ImageTags.Add(new SelectListItem() { Text = imageTag.Tag, Value = imageTag.ID.ToString() });
                }
            }

            SupplementVersions = new List<SelectListItem>();
            for (int i = 0; i <= supplementVersion; i++)
            {
                SupplementVersions.Add(new SelectListItem() { Text = i.ToString(), Value = i.ToString() });
            }
        }
    }

    public class ImageVM
    {
        public int ImageID { get; set; }
        public string ImagePath { get; set; }
        public int ImageTag { get; set; }
        public string ImageTagCustom { get; set; }
        public string FileExtension { get; set; }
        public int SupplementVersion { get; set; }
        public bool Include { get; set; }
        public bool Deleted { get; set; }
        public int PDFPageCount { get; set; }
        public int UserID { get; set; }
        public int LoginID { get; set; }
        public ImageExtraInfo ImageExtra { get; set; }
    }
}