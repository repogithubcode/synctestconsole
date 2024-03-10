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
    public class DocumentCategoryVM
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public DocumentVM UploadDocumentsVM { get; set; }

        public List<DocumentVM> DocumentsVM { get; set; }
        public bool hasChildren { get; set; }

        public SelectList DocumentCategories { get; set; }

        public DocumentCategoryVM()
        {
            // Sales Reps
            List<SelectListItem> documentCategoryItems = new List<SelectListItem>();

            List<DocumentCategory> documentCategories = DocumentCategory.GetForFilter(null);

            DocumentCategory documentCategory = documentCategories.Where(eachDocumentCategory => eachDocumentCategory.CategoryID == -1).FirstOrDefault();
            
            if(documentCategory != null)
            {
                documentCategories.Remove(documentCategory);
            }

            documentCategoryItems.Insert(0, new SelectListItem() { Text = "Select", Value = "-1" });

            foreach (DocumentCategory dc in documentCategories)
            {
                documentCategoryItems.Add(new SelectListItem() { Text = dc.CategoryName , Value = dc.CategoryID.ToString() });
            }

            DocumentCategories = new SelectList(documentCategoryItems, "Value", "Text");

            // set id as -1 initially
            id = -1;
            UploadDocumentsVM = new DocumentVM();
        }

        public DocumentCategoryVM(DocumentCategory documentCategory)
        {
            id = InputHelper.GetInteger(documentCategory.CategoryID.ToString());
            Name = InputHelper.GetString(documentCategory.CategoryName.ToString());
            Description = InputHelper.GetString(documentCategory.Description.ToString());

            DocumentsVM = new List<DocumentVM>();

            foreach (Document document in documentCategory.Documents)
            {
                DocumentsVM.Add(new DocumentVM(document));
            }
        }
    }
}