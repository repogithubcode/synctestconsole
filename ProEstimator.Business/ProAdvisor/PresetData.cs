namespace ProEstimator.Business.ProAdvisor
{
    public class PresetData
    {
        public int PresetID { get; set; }
        public int ShellID { get; set; }
        public string Labor { get; set; }
        public string Refinish { get; set; }
        public string Charge { get; set; }
        public string OtherCharge { get; set; }
        public bool Active { get; set; }
        public bool AutoSelect { get; set; }
        public string OtherTypeOverride { get; set; }
    }
}
