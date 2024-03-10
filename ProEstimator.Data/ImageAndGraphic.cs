using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData
{
    public class ImageAndGraphic
    {

        public int NImage { get; private set; }
        public string GraphicFileName { get; private set; }

        public ImageAndGraphic()
        {
            NImage = -1;
            GraphicFileName = "No Image";
        }

        public ImageAndGraphic(DataRow row)
        {
            NImage = InputHelper.GetInteger(row["nimage"].ToString());
            GraphicFileName = InputHelper.GetString(row["Graphic_Filename"].ToString());
        }

    }
}
