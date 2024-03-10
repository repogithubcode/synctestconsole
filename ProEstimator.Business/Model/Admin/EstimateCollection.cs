using System.Collections.Generic;
using System.Linq;

namespace ProEstimator.Business.Model.Admin
{
    public class EstimateCollection
    {
        private static EstimateCollection _instance;
        private EstimateCollection() { IDs = new List<LoginEstimate>(); }

        public static EstimateCollection GetInstance
        {
            get { return _instance ?? (_instance = new EstimateCollection()); }
        }
        private List<LoginEstimate> IDs { get; set; }

        public void AddEstimate(LoginEstimate item)
        {
            IDs.Add(item);
        }

        public LoginEstimate GetEstimates(int key)
        {
            return IDs.FirstOrDefault(item => item.IDs.Contains(key));
        }
        public LoginEstimate GetEstimatesForLogin(int key)
        {
            return IDs.FirstOrDefault(item => item.Key == key);
        }
        public void AddEstimateRange(LoginEstimate item)
        {
            var target = IDs.FirstOrDefault(x => x.Key == item.Key);
            if (target != null)
            {
                target.IDs.AddRange(item.IDs.Where(x => !target.IDs.Contains(x)));
            }
            else
            {
                IDs.Add(item);
            }
        }

        public void RemoveEstimate(int key, int value)
        {
            var account = IDs.FirstOrDefault(x => x.Key == key);
            if (account != null)
            {
                var estimate = account.IDs.FirstOrDefault(x => x == value);
                if (estimate != 0)
                {
                    account.IDs.Remove(value);
                }
            }
        }

        public void RemoveForLogin(int loginId)
        {
            var account = IDs.FirstOrDefault(x => x.Key == loginId);
            if (account != null && account.IDs.Any())
            {
                account.IDs.RemoveAll(x => true);
            }
        }
    }

    public class LoginEstimate
    {
        public int Key { get; set; }
        public List<int> IDs { get; set; } 
    }
}
