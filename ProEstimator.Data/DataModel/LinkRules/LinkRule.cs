namespace ProEstimatorData.DataModel.LinkRules
{
    public class LinkRule : ProEstEntity, IIDSetter
    {
        public int ID { get; private set; }
        int IIDSetter.ID
        {
            set { ID = value; }
        }

        public LinkRuleType RuleType { get; set; }
        public bool Deleted { get; set; }
        public bool Enabled { get; set; }
    }
}
