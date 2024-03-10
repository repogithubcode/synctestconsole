using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Streaming;

namespace ProEstimator.Business.Logic
{

    public class CustomReportGenerator
    {

        public ReportFunctionResult RenderCustomReport(int customReportID, int estimateID, int loginID = 0, Report estimateImagesReport = null)
        {
            try
            {
                // Get template and estimate and validate
                CustomReportTemplate template = CustomReportTemplate.Get(customReportID);

                if (template == null)
                {
                    ErrorLogger.LogError("Custom Report Template not found for ID " + customReportID, 0, 0, "CustomReportGenerator RenderCustomReport Template");
                    return new ReportFunctionResult("Custom Report Template not found.");
                }

                Estimate estimate = new Estimate(estimateID);

                if (estimate.CreatedByLoginID != template.LoginID && !template.IsSystemReport)
                {
                    ErrorLogger.LogError("Login ID doesn't match for Estimate " + estimateID + " and Report Template " + customReportID, 0, 0, "CustomReportGenerator RenderCustomReport Unauthorized");
                    return new ReportFunctionResult("Unauthorized request");
                }

                // Create a Report record
                ProEstimatorData.DataModel.Report report = new ProEstimatorData.DataModel.Report();
                if (estimateImagesReport != null)
                {
                    report = estimateImagesReport;
                }
                report.EstimateID = estimateID;
                report.CustomReportTemplateID = template.ID;
                report.FileName = ProEstimatorData.DataModel.Report.GetUniqueReportName(estimateID, template.Name);

                // Get the fully processed report text
                string processedTemplate = RenderPreviewCustomReport(customReportID, estimateID, "", loginID, true,report);

                SaveResult reportRecordSave = report.Save();

                if (!reportRecordSave.Success)
                {
                    return new ReportFunctionResult(reportRecordSave.ErrorMessage);
                }

                // Generate the PDF
                string diskPath = report.GetDiskPath();
                HtmlToPdfSaver.SavePdf(template.Name, diskPath, processedTemplate,report);

                return new ReportFunctionResult(report);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "RenderCustomReport Error");
                return new ReportFunctionResult(ex.Message);
            }
        }

        public string RenderPreviewCustomReport(int customReportID, int estimateID, string editorText, int loginID = 0, Boolean renderCustomReport = false, ProEstimatorData.DataModel.Report report = null)
        {
            string footerHtmlToRender = string.Empty;

            if (customReportID > 0)
            {
                CustomReportTemplate template = CustomReportTemplate.Get(customReportID);

                // Use the passed editorText (right from the editor for previewing), or if it is empty get the template from the db.
                if (string.IsNullOrEmpty(editorText) && template != null)
                {
                    editorText = template.Template;
                }

                editorText = "<div class=\"content\">" + editorText + "</div>";

                // Get the header and footer templates and combine them into the editorText
                if (template != null)
                {
                    // HeaderTemplate
                    if (template.ReportHeaderID > 0)
                    {
                        CustomReportTemplate headerTemplate = CustomReportTemplate.Get(template.ReportHeaderID);
                        if (!string.IsNullOrEmpty(headerTemplate.Template))
                        {
                            editorText = "<div class=\"header\">" + headerTemplate.Template + "</div>" + "\r\n" + editorText;
                        }
                    }

                    // FooterTemplate
                    if (template.ReportFooterID > 0)
                    {
                        CustomReportTemplate footerTemplate = CustomReportTemplate.Get(template.ReportFooterID);
                        if (!string.IsNullOrEmpty(footerTemplate.Template))
                        {
                            footerHtmlToRender = "<div class=\"footer\">" + footerTemplate.Template + "</div>";

                            if(renderCustomReport != true)
                            {
                                editorText += "\r\n" + footerHtmlToRender;
                            }
                            else
                            {
                                string filename = report.GetDiskPath(template.ReportFooterHtmlFileName);
                                report.ReportFooterHtmlFilePath = filename;
                                footerHtmlToRender = ReplaceTags(footerHtmlToRender, estimateID, loginID);

                                string folderPath = Path.GetDirectoryName(filename);
                                if (!Directory.Exists(folderPath))
                                {
                                    Directory.CreateDirectory(folderPath);
                                }

                                System.IO.File.WriteAllText(filename, footerHtmlToRender);
                            }
                        }
                    }
                }
            }

            // Generate the PDF
            string processedTemplate = ReplaceTags(editorText, estimateID, loginID);

            processedTemplate = _htmlTemplate.Replace("<template>", processedTemplate);

            return processedTemplate;
        }

        public string ReplaceTags(string template, int estimateID, int loginID = 0)
        {
            if (estimateID == 0)
            {
                estimateID = 1;
            }

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomReport_GetData", new System.Data.SqlClient.SqlParameter("AdminInfoID", estimateID));

            if (tableResult.Success)
            {
                template = template.Replace("]][[", "]] [[");

                if (template.Contains("[[CompanyLogo]]"))
                {
                    LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                    if (loginInfo != null)
                    {
                        string companyLogoImgTag = "<img id='imgLogo' src='" + loginInfo.GetLogoPath() + "' style='max-height: 100px;'/>";
                        template = template.Replace("[[CompanyLogo]]", companyLogoImgTag);
                    }
                }

                foreach (DataColumn column in tableResult.DataTable.Columns)
                {
                    template = template.Replace("[[" + column.ColumnName + "]]", tableResult.DataTable.Rows[0][column].ToString());
                }
            }
            template = template.Replace("border=\"1\"", "border=\"0\"");

            return template;
        }

        private string GetReportData(string storedProcedureName, List<SqlParameter> parameters, string diskPath)
        {
            DBAccess db = new DBAccess();
            SpreadsheetWriter spreadsheetWriter = null;
            string filename = string.Empty;

            DBAccessTableResult tableResult = db.ExecuteWithTable(storedProcedureName, parameters);

            if (tableResult.Success)
            {
                spreadsheetWriter = new SpreadsheetWriter();
                filename = spreadsheetWriter.WriteSpreadshet(tableResult.DataTable, diskPath);
            }

            return filename;
        }

        public string GetSalesReportData(int loginID, string startDate, string endDate, string estimatorID, string diskPath, string customerIDs)
        {
            string filename = string.Empty;

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginsID", loginID));
            parameters.Add(new SqlParameter("StartDate", startDate));
            parameters.Add(new SqlParameter("EndDate", endDate));
            parameters.Add(new SqlParameter("EstimatorID", estimatorID));
            parameters.Add(new SqlParameter("CustomerIDs", customerIDs));

            filename = GetReportData("SalesReport", parameters, diskPath);
            return filename;
        }

        public string GetCustomerListData(int loginID, string startDate, string endDate, string diskPath, string includeClosedDeleted)
        {
            string filename = string.Empty;

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginsID", loginID));
            parameters.Add(new SqlParameter("StartDate", startDate));
            parameters.Add(new SqlParameter("EndDate", endDate));
            parameters.Add(new SqlParameter("IncludeClosedDeleted", includeClosedDeleted));

            filename = GetReportData("GetCustomerList", parameters, diskPath);
            return filename;
        }

        public string GetSupplierListData(int loginID, string diskPath)
        {
            string filename = string.Empty;

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));

            filename = GetReportData("VendorListReport", parameters, diskPath);
            return filename;
        }

        public string GetSavedCustomerListData(int loginID, string diskPath, string includeClosedDeleted)
        {
            string filename = string.Empty;

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("IncludeClosedDeleted", includeClosedDeleted));

            filename = GetReportData("GetSavedCustomerList", parameters, diskPath);
            return filename;
        }

        public string GetCloseRatioReportListData(int loginID, string startDate, string endDate, string diskPath)
        {
            string filename = string.Empty;

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("StartDate", startDate));
            parameters.Add(new SqlParameter("EndDate", endDate));

            filename = GetReportData("CloseRatioReport", parameters, diskPath);
            return filename;
        }

        private string _htmlTemplate = @"
<html dir='ltr' lang='en-gb'>
	<head>
		<title></title>
		<style data-cke-temp='1'>
			html{cursor:text;*cursor:auto}
			img,input,textarea{cursor:default}
      		</style>
      		<link type='text/css' rel='stylesheet' href='" + ConfigurationManager.AppSettings["BaseURL"] + @"Content/ckeditor/contents.css?t=L0QD'>
      		<link type='text/css' rel='stylesheet' href='" + ConfigurationManager.AppSettings["BaseURL"] + @"Content/ckeditor/plugins/tableselection/styles/tableselection.css'>
      
		<style data-cke-temp='1'>

         		.cke_contents_ltr a.cke_anchor,.cke_contents_ltr a.cke_anchor_empty,.cke_editable.cke_contents_ltr a[name],.cke_editable.cke_contents_ltr a[data-cke-saved-name]
			{
				background:url(" + ConfigurationManager.AppSettings["BaseURL"] + @"Content/ckeditor/plugins/link/images/anchor.png?t=L0QD) no-repeat left center;
				border:1px dotted #00f;
				background-size:16px;
				padding-left:18px;
				cursor:auto;
			}
			.cke_contents_ltr img.cke_anchor
			{
				background:url(" + ConfigurationManager.AppSettings["BaseURL"] + @"Content/ckeditor/plugins/link/images/anchor.png?t=L0QD) no-repeat left center;
				border:1px dotted #00f;
				background-size:16px;
				width:16px;
				min-height:15px;
				height:1.15em;
				vertical-align:text-bottom;
			}
			.cke_contents_rtl a.cke_anchor,.cke_contents_rtl a.cke_anchor_empty,.cke_editable.cke_contents_rtl a[name],.cke_editable.cke_contents_rtl a[data-cke-saved-name]
			{
				background:url(" + ConfigurationManager.AppSettings["BaseURL"] + @"Content/ckeditor/plugins/link/images/anchor.png?t=L0QD) no-repeat right center;
				border:1px dotted #00f;
				background-size:16px;
				padding-right:18px;
				cursor:auto;
			}
			.cke_contents_rtl img.cke_anchor
			{
				background:url(" + ConfigurationManager.AppSettings["BaseURL"] + @"Content/ckeditor/plugins/link/images/anchor.png?t=L0QD) no-repeat right center;
				border:1px dotted #00f;
				background-size:16px;
				width:16px;
				min-height:15px;
				height:1.15em;
				vertical-align:text-bottom;
			}
         		.cke_show_borders  table.cke_show_border,.cke_show_borders  table.cke_show_border > tr > td, .cke_show_borders  table.cke_show_border > tr > th,.cke_show_borders  table.cke_show_border > tbody > tr > td, .cke_show_borders  table.cke_show_border > tbody > tr > th,.cke_show_borders  table.cke_show_border > thead > tr > td, .cke_show_borders  table.cke_show_border > thead > tr > th,.cke_show_borders  table.cke_show_border > tfoot > tr > td, .cke_show_borders  table.cke_show_border > tfoot > tr > th
			{
				border : #d3d3d3 1px dotted
			}
         		.cke_widget_wrapper
			{
				position:relative;
				outline:none
			}
			.cke_widget_inline
			{
				display:inline-block
			}
			.cke_widget_wrapper:hover>.cke_widget_element
			{	
				outline:2px solid #ffd25c;
				cursor:default
			}
			.cke_widget_wrapper:hover .cke_widget_editable
			{
				outline:2px solid #ffd25c;
			}
			.cke_widget_wrapper.cke_widget_focused>.cke_widget_element,.cke_widget_wrapper .cke_widget_editable.cke_widget_editable_focused
			{
				outline:2px solid #47a4f5;
			}
			.cke_widget_editable
			{
				cursor:text
			}
			.cke_widget_drag_handler_container
			{
				position:absolute;
				width:15px;
				height:0;
				display:block;
				opacity:0.75;
				transition:height 0s 0.2s;
				line-height:0;
			}
			.cke_widget_wrapper:hover>.cke_widget_drag_handler_container
			{
				height:15px;	
				transition:none;
			}
			.cke_widget_drag_handler_container:hover{opacity:1}
			.cke_editable[contenteditable='false'] .cke_widget_drag_handler_container{display:none;}
			img.cke_widget_drag_handler
			{
				cursor:move;
				width:15px;
				height:15px;
				display:inline-block
			}
			.cke_widget_mask
			{
				position:absolute;
				top:0;
				left:0;
				width:100%;	
				height:100%;
				display:block
			}
			.cke_widget_partial_mask{position:absolute;display:block}
			.cke_editable.cke_widget_dragging, .cke_editable.cke_widget_dragging *{cursor:move !important}
         		.cke_upload_uploading img{opacity: 0.3}
         		.cke_placeholder{background-color:#ff0}

            .flex-container {
                display: flex;
                flex-flow: column;
                height:100%;
            }

            .flex-container .header {
            }

            .flex-container .content {
                flex: 1 1 auto;
            }

            .flex-container .footer {
                flex: 0 1 50px;
            }
		</style>
	</head>
	<body contenteditable='true' class='cke_editable cke_editable_themed cke_contents_ltr' spellcheck='false' style='cursor: auto;'>
    <div class='flex-container'>
    <template>
    </div>
	</body>
</html>
";
    
        public void MergePDFfiles(string[] pdfDocumentsToMerge, string mergedFileNameWithDiscPath)
        {
            // Create a PdfStreamWriter instance, responsible to write the document into the specified file 
            using (PdfStreamWriter fileWriter = new PdfStreamWriter(System.IO.File.OpenWrite(mergedFileNameWithDiscPath)))
            {
                // Iterate through the files you would like to merge 
                foreach (string documentName in pdfDocumentsToMerge)
                {
                    // Open each of the files 
                    using (PdfFileSource fileToMerge = new PdfFileSource(System.IO.File.OpenRead(documentName)))
                    {
                        // Iterate through the pages of the current document 
                        foreach (PdfPageSource pageToMerge in fileToMerge.Pages)
                        {
                            // Append the current page to the fileWriter, which holds the stream of the result file 
                            fileWriter.WritePage(pageToMerge);
                        }
                    }
                }
            }
        }
    }
}
