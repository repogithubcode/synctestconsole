using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData.Models.EditorTemplateModel
{
    public class PaintGainValues
    {
        public double PaintGain_Blend { get; set; }
        public double PaintGain_ThreeTwoBlend { get; set; }
        public double PaintGain_Underside { get; set; }
        public double PaintGain_EdgingMin { get; set; }
        public double PaintGain_2ToneMajor { get; set; }
        public double PaintGain_2ToneNonAdjacent { get; set; }
        public double PaintGain_3StageMajor { get; set; }
        public double PaintGain_3StageNonAdjacent { get; set; }
        public double PaintGain_ClearCoatMajor { get; set; }
        public double PaintGain_ClearCoatNonAdj { get; set; }
        public double PaintDeduction_Adjacent { get; set; }
        public double PaintDeduction_NonAdjacent { get; set; }
        public bool PaintDeduction_AllowAdjacentDeductions { get; set; }

    }
}
