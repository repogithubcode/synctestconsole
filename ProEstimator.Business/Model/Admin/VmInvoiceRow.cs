using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmInvoiceRow : IModelMap<VmInvoiceRow>, IDataTableMap<VmInvoiceRow>
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public decimal SalesTax { get; set; }
        public decimal Total { get; set; }
        public string DueDate { get; set; }
        public string Notes { get; set; }
        public bool? Status { get; set; }
        public bool? Late { get; set; }

        public int DT_RowId;
        public VmInvoiceRow ToModel(DataRow row)
        {
            var model = new VmInvoiceRow();
            model.DT_RowId = (int)row["InvoiceId"];
            model.Id = (int) row["InvoiceId"];
            model.Type = row["InvoiceType"].SafeString();
            model.Amount = (decimal) row["InvoiceAmount"];
            model.SalesTax = (decimal)row["SalesTax"];
            model.Total = (decimal)row["InvoiceTotal"];
            model.DueDate = row["DueDate"].SafeString();
            model.Notes = row["Notes"].SafeString();
            model.Status = row["Paid"].SafeBool();
            model.Late = row["Late"].SafeBool();

            return model;
        }

        public VmInvoiceRow MapFromDataTableRow(Dictionary<string, Dictionary<string, string>> row)
        {
            var model = new VmInvoiceRow();
            int myKey;
            if (int.TryParse(row.FirstOrDefault().Key, out myKey))
            {
                model.Id = myKey;
            }
            model.Amount = Decimal.Parse(row.FirstOrDefault().Value["Amount"]);
            model.SalesTax = Decimal.Parse(row.FirstOrDefault().Value["SalesTax"]);
            model.DueDate = row.FirstOrDefault().Value["DueDate"];

            return model;
        }

        public Dictionary<string, Dictionary<string, string>> MapToDataTableRow()
        {
            var model = this;
            var row = new Dictionary<string, Dictionary<string, string>>();
            row[model.Id.ToString()] = new Dictionary<string, string>()
            {
                {"Type", model.Type},
                {"Amount", model.Amount.ToString(CultureInfo.InvariantCulture)},
                {"SalesTax", model.SalesTax.ToString(CultureInfo.InvariantCulture)},
                {"Total", model.Total.ToString(CultureInfo.InvariantCulture)},
                {"DueDate", model.DueDate},
                {"Notes", model.Notes},
                {"Status", model.Status.ToString()},
                {"Late", model.Late.ToString()},
                {"DT_RowId", model.DT_RowId.ToString()},
                {"Id", model.Id.ToString()}
            };

            return row;
        }
    }
}
