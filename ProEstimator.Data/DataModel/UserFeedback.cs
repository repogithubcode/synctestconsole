using System;
using System.Collections.Generic;

namespace ProEstimatorData.DataModel
{
    public class UserFeedback
    {
        public int ID { get; set; }
        public int ActiveLoginID { get; set; }
        public string FeedbackText { get; set; }
        public DateTime CreatedDate { get; set; }
        public string BrowserDetails { get; set; }
        public string DevicePlatform { get; set; }

        public List<string> ImagesPath { get; set; }

        public string ImagePaths { get; set; }
    }
}