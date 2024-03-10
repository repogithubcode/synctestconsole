using ProEstimator.Business.Logic;
using ProEstimator.Business.Model.Admin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Console
{
    class Program
    {
        static AdminService _service = new AdminService();

        static void Main(string[] args)
        {
            var loginId = 66015;
            System.Console.WriteLine(string.Format("Getting images for login ID: {0}", loginId));
            try
            {
                _service.MigrateImages(loginId);
            }
            catch(Exception ex)
            {
                var inner = ex.InnerException;
                System.Console.WriteLine(inner.Message);
            }
            //var info = GetImageInfoForLogin(loginId);
            //CopyImages(info);
            
            System.Console.ReadLine();
        }

        private static void CopyImages(List<ImageInfo> info)
        {
            foreach (var item in info)
            {
                var PePath = string.Format(@"D:\ProEstimatorStorage\{0}\{1}\{2}", item.LoginId, item.EstimateId, "Images");
                System.Console.WriteLine(string.Format("Creating Directory: {0}", PePath));
                Directory.CreateDirectory(PePath);
                System.Console.WriteLine(string.Format("Copying from: {0} , Copying to: {1}", item.WePath, string.Format(@"{0}\{1}", PePath, item.FileName)));
                if (!File.Exists(string.Format(@"{0}\{1}", PePath, item.FileName)))
                {
                    File.Copy(string.Format(@"{0}", item.WePath), string.Format(@"{0}\{1}", PePath, item.FileName));
                }
                if (!File.Exists(string.Format(@"{0}\{1}", PePath, item.ThumbFileName)))
                {
                    File.Copy(string.Format(@"{0}", item.WePath), string.Format(@"{0}\{1}", PePath, item.ThumbFileName));
                }
            }
        }

        private static List<ImageInfo> GetImageInfoForLogin(int login)
        {
            var result = new List<ImageInfo>();
            var fromBus = _service.GetImagesForLogin(login);
            result.AddRange(fromBus);

            return result;
        }
    }
}
