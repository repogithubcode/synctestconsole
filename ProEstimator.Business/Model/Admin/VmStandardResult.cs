namespace ProEstimator.Business.Model.Admin
{
    public class VmStandardResult<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public T Payload {get; set;}
    }
}
