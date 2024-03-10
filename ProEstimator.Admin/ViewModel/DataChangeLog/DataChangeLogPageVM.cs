using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData;

namespace ProEstimator.Admin.ViewModel.DataChangeLog
{

    public class DataChangeLogPageVM
    {
        public string SelectedItemType { get; set; }
        public SelectList ItemTypeList { get; set; }

        public DataChangeLogPageVM()
        {
            List<string> itemTypes = ChangeLogManager.GetAllChangeLogItemTypes();
            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem() { Text = "--All Types--", Value = "" });
            foreach (string item in itemTypes)
            {
                items.Add(new SelectListItem() { Text = item, Value = item });
            }

            ItemTypeList = new SelectList(items, "Value", "Text");
        }

    }
}