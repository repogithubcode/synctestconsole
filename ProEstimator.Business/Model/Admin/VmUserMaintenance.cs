using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmUserMaintenance : IModelMap<VmUserMaintenance>
    {
        public int Id { get; set; }
        public string LoginName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Organization { get; set; }
        public int? OrganizationId { get; set; }
        public string CreationDate { get; set; }
        public bool PasswordExpired { get; set; }
        public string LastActivity { get; set; }
        public int Rank { get; set; }
        public string Password { get; set; }
        public string SalesRep { get; set; }

        public VmUserMaintenance ToModel(DataRow row)
        {
            var model = new VmUserMaintenance();
            model.Id = (int)row["id"];
            model.LoginName = row["loginname"].SafeString();
            model.FirstName = row["FirstName"].SafeString();
            model.LastName = row["LastName"].SafeString();
            model.CompanyName = row["CompanyName"].SafeString();
            model.Organization = row["organization"].SafeString();
            model.OrganizationId = row["organizationid"].SafeInt();
            model.CreationDate = row["creationdate"].SafeDate();
            model.PasswordExpired = (bool)row["passwordexpired"];
            model.LastActivity = row["LastActivity"].SafeDate();
            model.Rank = (int)row["Rank"];
            model.Password = row["Password"].SafeString();
            model.SalesRep = row["SalesRep"].SafeString();

            return model;
        }
    }
}
