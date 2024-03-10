using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel
{
    public class DropDownData
    {
        public string Value { get; set; }
        public string Text { get; set; }

        public DropDownData(string value, string text)
        {
            Value = value;
            Text = text;
        }
    }
}