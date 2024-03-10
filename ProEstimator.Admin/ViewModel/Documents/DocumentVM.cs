using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;
using ProEstimatorData;
using ProEstimator.Business.Model.Admin;
using ProEstimator.Business.Model.Admin.Reports;
using ProEstimator.Business.Model;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel.ManageDocument
{
    public class DocumentVM
    {
        public int DocumentID { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string UploadDate { get; set; }
        public string IsDeleted { get; set; }

        public List<DocumentVM> Documents { get; set; }
        public SelectList DocumentCategories { get; set; }

        public DocumentVM()
        {
            DocumentCategories = GetAllCategoriesListItem();
        }

        public DocumentVM(Document document)
        {
            DocumentID = InputHelper.GetInteger(document.DocumentID.ToString());
            Category = InputHelper.GetString(document.Category.ToString());
            Name = InputHelper.GetString(document.Name.ToString());
            FileName = InputHelper.GetString(document.FileName.ToString());
            UploadDate = InputHelper.GetString(document.UploadDate.ToString());
            IsDeleted = InputHelper.GetString(document.IsDeleted.ToString());

            DocumentCategories = GetAllCategoriesListItem();
        }

        public SelectList GetAllCategoriesListItem()
        {
            // Sales Reps
            List<SelectListItem> documentCategoryItems = new List<SelectListItem>();

            List<string> allDocumentCategories = Document.GetAllCategories();

            documentCategoryItems.Insert(0, new SelectListItem() { Text = "Select", Value = "Select" });

            foreach (string category in allDocumentCategories)
            {
                documentCategoryItems.Add(new SelectListItem() { Text = category, Value = category });
            }

            SelectList documentCategories = new SelectList(documentCategoryItems, "Value", "Text");

            return documentCategories;
        }
    }
}