
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProEstimatorData
{
    public class ImageAndHotspots
    {
        public ImageAndGraphic image { get; set; }
        public List<ImageHotspot> hotspot { get; set; }
        public string Base64Img { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        public ImageAndHotspots()
        {
            hotspot = new List<ImageHotspot>();
        }
    }
}