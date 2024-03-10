namespace ProEstimatorData.DataModel.ProAdvisor
{
    public class ProAdvisorPresetShell : ProEstEntity, IIDSetter
    {
        public int ID { get; private set; }
        int IIDSetter.ID
        {
            set { ID = value; }
        }

        public string OperationType	{ get; set; }
        public bool Sublet { get; set; }
        public string Name { get; set; }
        public string LaborType	{ get; set; }
        public string OtherType { get; set; }
        public string Notes { get; set; }
        public bool OnePerVehicle { get; set; }
        public int AccessLevel { get; set; }
        public bool Deleted { get; set; }
    }
}
