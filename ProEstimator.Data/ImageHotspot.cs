using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData
{
    public class ImageHotspot
    {
        public int Callout_Number { get; set; }
        public int X_Coordinate { get; set; }
        public int Y_Coordinate { get; set; }
        public int X_Extent { get; set; }
        public int Y_Extent { get; set; }
        public int nimage { get; set; }
        public int npart { get; set; }
        public string Graphic_Filename { get; set; }

        public ImageHotspot(DataRow row)
        {
            Callout_Number = InputHelper.GetInteger(row["Callout_Number"].ToString());    
            X_Coordinate = InputHelper.GetInteger(row["X_Coordinate"].ToString());
            Y_Coordinate = InputHelper.GetInteger(row["Y_Coordinate"].ToString());
            X_Extent = InputHelper.GetInteger(row["X_Extent"].ToString());
            Y_Extent = InputHelper.GetInteger(row["Y_Extent"].ToString());
            nimage = InputHelper.GetInteger(row["nimage"].ToString());
            npart = InputHelper.GetInteger(row["npart"].ToString());
            Graphic_Filename = InputHelper.GetString(row["Graphic_Filename"].ToString());

            if (X_Coordinate < 0)
            {
                X_Coordinate = 0;
            }
            if (Y_Coordinate < 0)
            {
                Y_Coordinate = 0;
            }

            X_Extent = X_Coordinate + X_Extent;
            Y_Extent = Y_Coordinate + Y_Extent;
        }
    }
}
