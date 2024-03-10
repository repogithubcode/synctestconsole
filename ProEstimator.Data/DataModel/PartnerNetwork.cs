namespace ProEstimatorData.DataModel
{
    public class PartnerNetwork : IIDSetter
    {
        public int ID { get; private set; }

        int IIDSetter.ID
        {
            set { ID = value; }
        }

        public string Name { get; set; }
        public string Tagline { get; set; }
        public string Summary { get; set; }
        public string Link { get; set; }
        public int SortOrder { get; set; }
        public bool Active { get; set; }
    }
}
