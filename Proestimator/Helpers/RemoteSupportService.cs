using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Web.Services;

using Proestimator;
using Proestimator.Helpers;
using ProEstimatorData;
using ProEstimatorData.Models.EditorTemplateModel;
using ProEstimatorData.Models;
using ProEstimatorData.Models.SubModel;
using Proestimator.ViewModel;

namespace Proestimator.Helpers
{
    public class RemoteSupportService
    {
        private RemoteSupportService()
        {

        }

        private static RemoteSupportService _remoteSupportService;
        private List<RemoteSupport> _remoteSupportCollection = new List<RemoteSupport>();

        private static readonly object _lock = new object();

        public static RemoteSupportService GetInstance()
        {
            if (_remoteSupportService == null)
            {
                lock (_lock)
                {
                    if (_remoteSupportService == null)
                    {
                        _remoteSupportService = new RemoteSupportService();
                    }
                }
            }
            return _remoteSupportService;
        }

        public void TurnOnRemoteSupportLink(int loginID)
        {
            RemoteSupport remoteSupport = _remoteSupportCollection.Where(o => o.LoginID == loginID).FirstOrDefault();
            if (remoteSupport == null)
            {
                remoteSupport = new RemoteSupport(loginID);
                _remoteSupportCollection.Add(remoteSupport);
            }
        }

        public void TurnOffRemoteSupportLink(int loginID)
        {
            RemoteSupport remoteSupport = _remoteSupportCollection.Where(o => o.LoginID == loginID).FirstOrDefault();
            if (remoteSupport != null)
            {
                _remoteSupportCollection.Remove(remoteSupport);
            }
        }

        public Boolean IsTurnOnRemoteSupportLink(int loginID)
        {
            RemoteSupport remoteSupport = _remoteSupportCollection.Where(o => o.LoginID == loginID).FirstOrDefault();

            if (remoteSupport == null)
            {
                return false;
            }
            else
            {
                if (remoteSupport.IsRemoteSupportEnable)
                {
                    return true;
                }
                else
                {
                    TurnOffRemoteSupportLink(loginID);
                    return false;
                }
            }
        }
    }

    public class RemoteSupport
    {
        public int LoginID { get; set; }
        public DateTime RemoteSupportEnabledAt { get; set; }

        public RemoteSupport(int loginID)
        {
            LoginID = loginID;
            RemoteSupportEnabledAt = DateTime.Now;
        }

        public Boolean IsRemoteSupportEnable
        {
            get
            {
                return (int)DateTime.Now.Subtract(RemoteSupportEnabledAt).TotalMinutes < 5 ? true : false;
            }
        }
    }
}