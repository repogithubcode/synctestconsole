using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;

namespace ProEstimator.Business.Model.Admin
{
    public class ImageInfo : IModelMap<ImageInfo>
    {
        public string LoginId { get; set; }

        public string EstimateId { get; set; }

        public string WePath { get; set; }

        public string FileName { get; set; }

        public string ThumbFileName { get; set; }

        public ImageInfo ToModel(System.Data.DataRow row)
        {
            var model = new ImageInfo();
            model.LoginId = ((int)row["LoginId"]).ToString();
            model.EstimateId = ((int)row["EstimateId"]).ToString();
            model.WePath = row["WePath"].SafeString();
            model.FileName = row["FileName"].SafeString();
            model.ThumbFileName = row["ThumbFileName"].SafeString();

            return model;
        }
    }
}
