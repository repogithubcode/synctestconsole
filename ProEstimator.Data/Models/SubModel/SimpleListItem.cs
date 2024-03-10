using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProEstimatorData.Models.SubModel
{
    public class SimpleListItem
    {
        public string Text { get; set; }
        public string Value { get; set; }

        public SimpleListItem()
        {

        }

        public SimpleListItem(string text, string value)
        {
            this.Text = text;
            this.Value = value;
        }
    }
}