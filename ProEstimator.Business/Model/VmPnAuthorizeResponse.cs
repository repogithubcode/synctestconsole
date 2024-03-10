using System;

namespace ProEstimator.Business.Model
{
    public class VmPnAuthorizeResponse
    {
        public Guid UserId { get; set; }
        public string UserUri { get; set; }
        public string RedirectUri { get; set; }
        public Guid RequestId { get; set; }
        public bool Success { get; set; }
    }
}
