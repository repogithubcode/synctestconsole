using System.ComponentModel.DataAnnotations;
using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmAccount : IModelMap<VmAccount>
    {
        [Required(ErrorMessage = "Username is required")]
        public int SalesRepID { get; set; }
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public bool IsValidLogin { get; set; }
        public bool IsAdminUser { get; set; }
        public bool IsManager { get; set; }

        public string RedirectAfterLogin { get; set; }

        public VmAccount ToModel(DataRow row)
        {
            var model = new VmAccount();
            model.IsValidLogin = (bool)row["isValidLogin"];
            if (model.IsValidLogin)
            {
                model.SalesRepID = (int)row["SalesRepID"];
                model.UserName = row["UserName"].SafeString();
                model.IsAdminUser = (bool)row["isAdminUser"];
            }

            return model;
        }
    }
}
