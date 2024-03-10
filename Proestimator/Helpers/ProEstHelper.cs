using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.Models;
using ProEstimatorData.Models.SubModel;
using System.Web.Mvc;
using ProEstimatorData.Models.EditorTemplateModel;

namespace Proestimator.Helpers
{
    public static class ProEstHelper
    {

        public static int CintWithTry(object TheValue, int DefaultValue = 0)
        {
            int functionReturnValue = 0;
            try
            {
                functionReturnValue = Convert.ToInt32(TheValue);
            }
            catch (System.Exception ex)
            {
                try
                {
                    functionReturnValue = DefaultValue;
                }
                catch
                {
                    functionReturnValue = 0;
                }
            }
            return functionReturnValue;
        }

        public static float CSngWithTry(object TheValue, float DefaultValue = 0)
        {
            float functionReturnValue = 0;
            try
            {
                functionReturnValue = Convert.ToSingle(TheValue);
            }
            catch (System.Exception ex)
            {
                try
                {
                    functionReturnValue = DefaultValue;
                }
                catch
                {
                    functionReturnValue = 0;
                }
            }
            if (functionReturnValue == Double.NaN)
            {
                functionReturnValue = 0;
            }
            return functionReturnValue;
        }

        public static double CDblWithTry(object TheValue, double DefaultValue = 0)
        {
            double functionReturnValue = 0;
            try
            {
                functionReturnValue = Convert.ToDouble(TheValue);
            }
            catch (System.Exception ex)
            {
                try
                {
                    functionReturnValue = DefaultValue;
                }
                catch
                {
                    functionReturnValue = 0;
                }
            }
            return functionReturnValue;
        }

        public static bool VerifyInteger(string TheValue)
        {
            bool functionReturnValue = false;
            int TestValue = 0;
            if (!string.IsNullOrEmpty(TheValue))
            {
                try
                {
                    TestValue = Convert.ToInt32(TheValue.Replace(",", "").Replace("$", ""));
                    functionReturnValue = true;
                }
                catch
                {
                    functionReturnValue = false;
                }
            }
            else
            {
                functionReturnValue = true;
            }
            return functionReturnValue;
        }

        public static List<SimpleListItem> GetLaborTypeList(int estimateID = -1)
        {
            // Fill the Labor Type List
            List<SimpleListItem> laborTypeList = new List<SimpleListItem>();
            laborTypeList.Add(new SimpleListItem("All", "0"));
            laborTypeList.Add(new SimpleListItem("Body", "1"));
            laborTypeList.Add(new SimpleListItem("Frame", "2"));
            laborTypeList.Add(new SimpleListItem("Structure", "3"));
            laborTypeList.Add(new SimpleListItem("Mechanical", "4"));
            laborTypeList.Add(new SimpleListItem("Electrical", "24"));
            laborTypeList.Add(new SimpleListItem("Glass", "25"));

            if (ManualEntryDetail.UseAluminum(estimateID) || estimateID == -1)
            {
                laborTypeList.Add(new SimpleListItem("Aluminum", "5"));
            }
            else
            {
                laborTypeList.Add(new SimpleListItem("Detail", "5"));
            }


            laborTypeList.Add(new SimpleListItem("Cleanup", "6"));
            laborTypeList.Add(new SimpleListItem("Other", "8"));
            laborTypeList.Add(new SimpleListItem("Refinish", "-1"));
            laborTypeList.Add(new SimpleListItem("PDR", "-2"));

            return laborTypeList;
        }

    }
}