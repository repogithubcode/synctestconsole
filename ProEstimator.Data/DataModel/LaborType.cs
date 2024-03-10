using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData.DataModel
{
    public enum LaborType
    {
        Empty = 0,
        Body = 1,
        Frame = 2,
        Structure = 3,
        Mechanical = 4,
        Detail = 5,
        Cleanup = 6,
        Paint = 7,
        Other = 8,
        PaintPanelClearcoat = 9,
        FirstPanel = 10,
        AdjPanel = 11,
        NonAdjPanel = 12,
        Nontaxed = 13,
        Taxed = 14,
        Sublet = 15,
        BaseCoat = 16,
        PaintPanelNone = 17,
        PaintPanelThreeStage = 18,
        PaintPanelTwoStage = 19,
        Clearcoat = 20,
        Edging = 21,
        Underside = 22,
        None = 23,
        Electrical = 24,
        Glass = 25,
        Blend = 26,
        ThreeStageAllowance = 27,
        TwoToneAllowance = 28,
        PaintPanelTwoTone = 29,
        Towing = 30,
        Storage = 31
    }
}
