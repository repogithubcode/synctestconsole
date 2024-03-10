namespace ProEstimator.Business.Model
{
    public class VmPnRequestLineItem
    {
        public int LineNumber { get; set; }
        public string PartType { get; set; }
        public string OemPartNumber { get; set; }
        public string PartDescription { get; set; }
        public string AlternatePartNumber { get; set; }
        public int Quantity { get; set; }
        public decimal ListPrice { get; set; }
        public decimal Price { get; set; }
    }
}
