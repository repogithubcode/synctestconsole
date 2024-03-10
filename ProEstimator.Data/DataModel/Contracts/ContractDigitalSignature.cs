using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Contracts
{
    public class ContractDigitalSignature
    {

        public int ID { get; private set; }
        public int LoginID { get; set; }
        public int ContractID { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public DateTime TimeStamp { get; set; }

        public ContractDigitalSignature()
        {

        }

        public ContractDigitalSignature(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            ContractID = InputHelper.GetInteger(row["ContractID"].ToString());
            Name = InputHelper.GetString(row["Name"].ToString());
            Date = InputHelper.GetString(row["Date"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
        }

        public SaveResult Insert()
        {
            if (ID > 0)
            {
                return new SaveResult("Digital Signature has already been saved, cannot insert again.");
            }

            // Do some validation
            if (string.IsNullOrEmpty(Name))
            {
                return new SaveResult("Please enter your name.");
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("ContractID", ContractID));
            parameters.Add(new SqlParameter("Name", Name));
            parameters.Add(new SqlParameter("Date", Date));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("ContractDigitalSignature_Insert", parameters);

            if (intResult.Success)
            {
                ID = intResult.Value;
                return new SaveResult();
            }
            else
            {
                return new SaveResult(intResult.ErrorMessage);
            }
        }

        public static ContractDigitalSignature Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("ContractDigitalSignature_Get", new SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return new ContractDigitalSignature(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public static ContractDigitalSignature GetForContract(int contractID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("ContractDigitalSignature_GetForContractID", new SqlParameter("ContractID", contractID));

            if (tableResult.Success)
            {
                return new ContractDigitalSignature(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public string GetContractContent(bool includePageWrapper)
        {
            Contract mainContract = Contract.Get(this.ContractID);

            StringBuilder builder = new StringBuilder();

            if (includePageWrapper)
            {
                builder.AppendLine(@"
<!DOCTYPE html>
<html>
<head>
<style>
    body {
            border: 0px;
            margin: 0px;
            width: 100%;
            height: 100%;
        }

        .contract-wrapper {
            padding-top: 20px;
            padding-bottom: 20px;
            font-size: 0.85em;
        }

        .signature-wrapper {
            width: 50%;
            padding-top: 40px;
            margin: 0px auto;
        }

        h1 {
            font-size: 1.2em;
            text-align: center;
        }

        .column-container {
            column-count: 2;
            max-width: 900px;
            margin: 0px auto;
            
        }

        .column-container p {
            text-align: justify;
            margin-right: 10px;
        }

        .section-title {
            font-weight: bold;
            text-decoration: underline;
        }

        .section-item {
            margin-left: 10px;
        }

        .contract-table-wrapper {
            max-width: 700px;
            margin: 0px auto;
            border: 5px solid gray;
            margin-bottom: 20px;
        }

        .contract-table-wrapper table {
            width: 100%;
            border-collapse: collapse;
        }

        .contract-table-wrapper thead {
            border-bottom: 4px solid gray;
            font-weight: bold;
        }

        .contract-table-wrapper td {
            padding: 5px;
        }
</style>
</head>
<body>");
            }

            builder.AppendLine(@"<div class=""contract-wrapper"">
    <h1>
        Web-Est, LLC
        <br />END USER LICENSE AGREEMENT
    </h1>
    <div class=""column-container"">
    <p>This End User License Agreement (""Agreement"") is entered into as of the Effective Date (as set forth on the signature page hereto) by and between Web-Est, LLC, a Delaware limited liability company (""WEB-EST""), and the customer identified on the signature page hereto (""CUSTOMER""). This Agreement shall consist of these terms and conditions,</p>
        <p class=""section-title"">1.	  License; Use.</p>
        <p class=""section-item"">1.01.	Subject to the terms and conditions of this Agreement: (i) WEB-EST grants to CUSTOMER a non-transferable, non-sublicensable, non-exclusive and periodically renewable license to use WEB-EST's Web Based Collision Repair Estimating product described in Exhibit A, attached hereto and made a part hereof, including any input and output formats, associated data structures and databases, graphical elements, narrative descriptions and operating instructions (such products, instructions and other items are collectively referred to as ""Software""); and (ii) WEB-EST agrees to sell and CUSTOMER agrees to purchase the services provided herein (""Services"") (the Software and Services are collectively referred to as the ""Products"").</p>
        <p class=""section-item"">1.02.	CUSTOMER acknowledges that the Products are licensed or provided to CUSTOMER solely for CUSTOMER's own internal business use at the Authorized Location(s) specified on Exhibit A. The Products may be used by only one (1) individual user per license, who shall be an employee of CUSTOMER. The Products include a license to WEB-EST's OEM Estimating System Data. CUSTOMER shall not, without obtaining WEB-EST's prior written approval, which may be withheld at WEB-EST's sole and absolute discretion, permit or allow the use of the Products in any location aside from the Authorized Location(s).</p>
        <p class=""section-item"">1.03.	Except for a copy of the finished estimates written by CUSTOMER and limited excerpts of data provided orally or in written documents created in CUSTOMER's ordinary course of business, CUSTOMER shall not sell, distribute, market, prepare saleable derivative works, assign, pledge, sublicense or permit any other distribution or use of the Products (or any information contained in or derived from the Products) without WEB-EST's prior written consent, and CUSTOMER shall not permit any of the foregoing actions to be taken by its end users/employees.</p>
        <p class=""section-item"">1.04.	WEB-EST reserves the right to make changes in rules of operation, security measures, accessibility, procedures, system programming languages and any other matters relating to the Products. CUSTOMER acknowledges that such modifications could require CUSTOMER to modify its associated software programs, at its own expense.</p>
        <p class=""section-item"">1.05.	CUSTOMER shall not transmit, directly or indirectly, the Products, or any component thereof, to: (i) any country outside the United States ; or (ii) any national resident thereof. CUSTOMER further agrees that it will obtain, at its own cost and expense, any and all necessary export licenses for any such approved export or for any approved disclosure of the Products to a foreign national.</p>
        <p class=""section-title"">2.	  Price and Payment Terms.</p>
        <p class=""section-item"">2.01.	CUSTOMER agrees to pay the fees set forth on the attached Exhibit A as consideration for the license of the Products from WEB-EST to CUSTOMER (the ""License Fees""). The License Fees specified on Exhibit A are exclusive of applicable sales, use, excise or similar taxes assessed now or hereafter imposed, and CUSTOMER shall pay to WEB-EST such costs, assessments, and taxes upon receipt of an invoice for the same from WEB-EST. Any pre-payment or deposits specified on Exhibit A are refundable for the FIRST 30-DAYS ONLY, AFTERWHICH ALL SALES SHALL BE FINAL.</p>
        <p class=""section-item"">2.02.	All fees under this Agreement are payable in advance (unless otherwise indicated on Exhibit A) and are due and payable upon receipt of an invoice. Fees are owed for the time period beginning with the date on which CUSTOMER is provided access to the Products.</p>
        <p class=""section-item"">2.03.	Invoices not paid within thirty (30) days of the due date indicated on the invoice shall be subject to interest at the lesser rate of: (i) one and one-half percent (1.5%) per month; or (ii) the maximum interest rate allowed by law in the state in which CUSTOMER's business is located. If CUSTOMER's check is returned to WEB-EST for insufficient funds, CUSTOMER will pay the face value of the check and a service charge of twenty-five dollars ($25.00). Subsequent returned checks will be subject to a service charge of thirty-five dollars ($35.00).</p>
        <p class=""section-item"">2.04.	CUSTOMER agrees that its obligation to make all payments due hereunder shall be absolute and unconditional, and shall not be subject to abatement, reduction, set-off, defense or counterclaim whatsoever. If CUSTOMER is in default of this Agreement, WEB-EST reserves the right to suspend access to the product, product support and/or communication capabilities. If an attorney is retained to collect amounts due hereunder, CUSTOMER shall be liable for any and all reasonable attorneys' fees, paralegal fees and costs incurred by WEB-EST in connection with such action.</p>
        <p class=""single-item"">3.	  Term. This Agreement shall be effective as of the date set forth on the signature page (""Effective Date""). The ""Term"" of this Agreement shall commence upon the Effective Date and continue for a period of Twelve (12) months from the Effective Date, unless earlier terminated as provided in Section 8. Notwithstanding the foregoing, CUSTOMER and WEB-EST may elect to extend the Term by executing an amendment to this Agreement. Any amendment to this Agreement shall incorporate the terms of this Agreement, unless said terms are expressly modified by the amendment.</p>
        <p class=""single-item"">4.	  Training. WEB-EST will provide the training for the Products. ALL TRAINING IS PROVIDED VIA THE TELEPHONE AND/OR THE INTERNET. WEB-EST shall provide this training free of charge and will use its commercially reasonable best efforts to schedule with CUSTOMER a training ""walk-through;"" however, it is the responsibility of CUSTOMER to make its personnel available during regular business hours for such training. The failure of CUSTOMER to provide WEB-EST with a reasonable time for training does not eliminate CUSTOMER's requirement to comply with all terms and conditions (including payments) of this Agreement.</p>
        <p class=""single-item"">5.  Maintenance of Equipment and Software. CUSTOMER shall obtain, maintain and operate, at its own cost and expense, all hardware, Internet access, equipment and non-WEB-EST software required to properly interface with the Products. CUSTOMER acknowledges that some hardware and operating environments may not readily accept the current or future functionality of the Software. CUSTOMER agrees, at its own expense, to make necessary changes or upgrades in hardware, software, memory, memory management and operating system environment to interface properly with the Software. If CUSTOMER utilizes any interface program to interface with the Software, CUSTOMER shall look solely to the supplier of such interface program with respect to any losses or damages caused by such interface program.</p>
        <p class=""section-title"">6.  Updates and Technical Support.</p>
        <p class=""section-item"">6.01	During the Term of this Agreement and provided all fees due hereunder have been paid, WEB-EST shall perform periodic database and/or software updates to subscription based Software (""Updates""). WEB-EST shall provide the Updates at WEB-EST's own expense. CUSTOMER acknowledges that such Update(s) of the online Products may require the Products to be off-line and otherwise unavailable for a short period of time. WEB-EST shall use its commercially reasonable best efforts to conduct such Updates and any other maintenance of the Products during non-business hours or non-peak business hours.</p>
        <p class=""section-item"">6.02	CUSTOMER authorizes WEB-EST to gather information (either electronically or from CUSTOMER'S employees) pertaining to end user hardware and software configuration for the purpose of determining Product requirements and providing support.</p>
        <p class=""single-item"">7.  Proprietary Rights; Confidentiality. CUSTOMER acknowledges that the Products and all portions, reproductions, corrections, modifications, updates, new versions or modules, and improvements thereof (collectively ""Enhancements"") provided to CUSTOMER hereunder are: (i) considered by WEB-EST to be trade secrets and protectable intellectual property; (ii) provided to CUSTOMER in confidence; and (iii) are the exclusive and proprietary property of WEB-EST and/or its third party licensors. Title and full ownership rights in the Products, the Enhancements and all related patent rights, copyrights, trade secrets, trademarks, service marks, related goodwill and confidential and proprietary information, are reserved to and shall remain with WEB-EST and/or the third party licensors. CUSTOMER's rights hereunder are those of a licensed end user only and are conditioned upon CUSTOMER's compliance with the terms and conditions of this Agreement. No transfer of any right, title, or interest, in the Products, other than the limited license set forth herein, is intended or made. CUSTOMER shall not reverse engineer, de-compile or disassemble the software, or attempt to discover any source code or derive the algorithms or know-how underlying the Software. CUSTOMER shall not alter, modify or prepare derivative works of the Software. CUSTOMER agrees that no portion of the information constituting the Products may be disclosed to others, copied, reproduced, compiled, interfaced with any systems or used for any purpose or purposes other than as specifically contemplated by this Agreement. CUSTOMER shall exercise all reasonable precautions to protect the Products and to prevent their dissemination to unauthorized persons.</p>
        <p class=""section-title"">8.  Default and Remedies.</p>
        <p class=""section-item"">8.01	An event of default shall occur if CUSTOMER: (i) fails to pay any License Fees or other payment required hereunder when due; (ii) becomes insolvent or bankrupt or makes any assignment for the benefit of creditors or consents to the appointment of a receiver or trustee for CUSTOMER of CUSTOMER's assets; (iii) takes actions to dissolve or close its business; or (iv) breaches the terms of this Agreement and does not cure such breach within ten (10) days after receipt of written notice of default by WEB-EST.</p>
        <p class=""section-item"">8.02	Upon the occurrence of any event of default and at any time thereafter WEB-EST may, in its sole discretion take any one or more of the following actions: (i) upon notice to CUSTOMER, terminate this Agreement; (ii) declare immediately due and payable, and require CUSTOMER to pay, all amounts hereunder that are past due or currently due and to become due during the balance of the then current term of this Agreement; (iii) proceed with legal action to enforce the terms of this Agreement and/or to recover damages for breach of this Agreement. In the event that legal proceedings are brought pursuant to this Section 8, WEB-EST shall be entitled to reimbursement from CUSTOMER of any and all reasonable attorneys' fees, paralegal fees, and costs associated with said action.</p>
        <p class=""section-item"">8.03	In the event that CUSTOMER and WEB-EST are or become parties to agreements other than this Agreement (an ""Other Agreement"") and CUSTOMER breaches such Other Agreement in a manner that could give rise to termination of such Other Agreement, WEB-EST shall have the right to: (i) terminate this Agreement, without liability to CUSTOMER, upon receipt by CUSTOMER of written notice of WEB-EST's election to do so; (ii) refuse to provide further Product access, support or other services in connection with this Agreement, without liability to CUSTOMER, until such breach is cured; or (iii) setoff any amounts due and owing by WEB-EST to CUSTOMER against amounts due WEB-EST under the terms of the Other Agreement.</p>
        <p class=""section-item"">8.04	Upon the termination of this Agreement for any reason, the license granted hereunder and all rights of CUSTOMER to the Products will immediately cease, CUSTOMER will not attempt to access the online Products, and CUSTOMER shall immediately destroy all WEB-EST Products in electronic and non- electronic form. If requested by WEB-EST, CUSTOMER shall provide written confirmation that it has fulfilled its obligations pursuant to this Section. Termination will not affect CUSTOMER's payment or other obligations to WEB-EST arising prior to termination.</p>
        <p class=""section-item"">8.05	If this Agreement is terminated and WEB-EST subsequently agrees with CUSTOMER to reinstate the effectiveness of the Agreement, the then current term of the Agreement shall be extended by the length of time between such termination and reinstatement dates.</p>
        <p class=""section-title"">9.  Warranties; Limitation of Liability.</p>
        <p class=""section-item"">9.01	WEB-EST PROVIDES ACCESS TO THE PRODUCTS ON AN ""AS-IS"" BASIS AND MAKES NO WARRANTIES OR GUARANTEES, WHETHER STATUTORY, EXPRESS, IMPLIED, ORAL OR WRITTEN, AND ALL WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE (WHETHER OR NOT WEB-EST KNOWS, HAS REASON TO KNOW, HAS BEEN ADVISED, OR IS OTHERWISE AWARE OF SUCH PURPOSE), NON-INFRINGEMENT, AND UNINTERRUPTED OR ERROR-FREE OPERATION ARE EXPRESSLY DISCLAIMED. WEB-EST OBTAINS AND GATHERS ITS INFORMATION FROM SOURCES IT CONSIDERS TO BE RELIABLE, HOWEVER, WEB-EST AND ITS THIRD PARTY DATA SOURCES SHALL HAVE NO LIABILITY WHATSOEVER WITH RESPECT TO THE ACCURACY OR COMPLETENESS OF THE DATA OR THE RESULTS OBTAINED THROUGH USE OF THE PRODUCTS. CERTAIN STATES OR COUNTRIES DO NOT ALLOW THE EXCLUSION OF IMPLIED WARRANTIES; THEREFORE, CERTAIN DISCLOSURES PROVIDED ABOVE MAY NOT APPLY TO CUSTOMER. CUSTOMER SHOULD CONSULT WITH AN ATTORNEY IF CUSTOMER HAS ANY QUESTION REGARDING THE DISCLAIMERS CONTAINED HEREIN. CUSTOMER acknowledges and agrees that WEB-EST is not the manufacturer or distributor of any automotive repair parts referenced in the Products; and does not make any representations or warranties with respect to the quality or availability of such parts.</p>
        <p class=""section-item"">9.02	IN NO EVENT SHALL WEB-EST, ITS DATA PROVIDERS AND/OR ITS LICENSORS, THEIR AGENTS OR EMPLOYEES BE LIABLE TO CUSTOMER OR ANY THIRD PARTY FOR LOSS OF PROFITS, REVENUE, DATA, COST OF SUBSTANTIALLYSIMILAR SOFTWARE, LOSS OF USE, LOSS OF GOODWILL, WORK STOPPAGE, COMPUTER FAILURE OR MALFUNCTION, OR ANY OTHER INCIDENTAL, CONSEQUENTIAL, INDIRECT, CONTINGENT, SECONDARY, OR SPECIAL DAMAGES OR EXPENSES OF ANY NATURE WHATSOEVER AND HOWSOEVER ARISING, EVEN IF WEB-EST HAS BEEN ADVISED OF THE POSSIBILITY OR CERTAINTY OF SUCH DAMAGES. CUSTOMER AGREES THAT THE AGGREGATE LIABILITY OF WEB-EST, ITS DATA PROVIDERS AND/OR ITS LICENSORS HEREUNDER, WHETHER ARISING OUT OF CONTRACT, NEGLIGENCE, STRICT LIABILITY IN TORT OR WARRANTY, SHALL NOT EXCEED THE AMOUNTS PAID BY CUSTOMER TO WEB-EST DURING THE PRECEDING TWELVE (12) MONTHS FOR THE PRODUCT(S) RELATING TO THE EVENT GIVING RISE TO SUCH LIABILITY.</p>
        <p class=""single-item"">10.  Use of Data.  CUSTOMER acknowledges and agrees that WEB-EST creates, develops, maintains, markets, licenses and sells a database of historical aggregate repair information received by WEB-EST&rsquo;s customers, including CUSTOMER, specifically relating to the repair information for vehicles for which WEB-EST&rsquo;s customers render repair services (e.g., vehicle identification numbers [&ldquo;vins&rdquo;], dates of repairs, nature of repairs and actual total cost of repairs relating to such specific vehicles which receive the repair services) (collectively, the &ldquo;User Repair Database&rdquo;).  WEB-EST markets, sells, licenses and distributes the User Repair Database in connection with the sale of WEB-EST reporting products to OEMs, consulting companies and other potential partners and/or customers (the &ldquo;Claims Data Customers&rdquo;), including specific fields of claims data collected or generated by the WEB-EST Software and by any other estimating software product licensed or made available by WEB-EST (collectively, &ldquo;Claims Data&rdquo;).  CUSTOMER consents and agrees to WEB-EST&rsquo;s marketing, sale, licensing and distribution of the User Repair Database and the Claims Data to the Claims Data Customers, during the term of this Agreement and thereafter; provided that such User Repair Database and Claims Data involving CUSTOMER&rsquo;s repair customers shall be anonymized and will not be identifiable, attributable or traceable to CUSTOMER. </p>
        <p class=""single-item"">11.  Notices. Any notices provided pursuant to this Agreement shall be in writing and shall be delivered by hand or sent by express or overnight mail, or by registered or certified mail, postage prepaid, return receipt requested, address to the party to be notified as follows: if to WEB-EST: Web-Est, LLC, 12884 S Frontrunner Blvd, Ste 220 Draper, UT 84020, Attn: Customer Service, with a copy to the Legal Department at the same address; if to CUSTOMER: to the contact name and address set forth on the signature page of this Agreement. All notices shall be deemed given when received by a representative of WEB-EST or CUSTOMER, as appropriate, at the addresses indicated herein. Either party may change its contact and/or address specified in this Section by providing written notice to the other party in accordance with this Section.</p>
        <p class=""single-item"">12.  Indemnity. CUSTOMER will defend, indemnify and hold WEB-EST harmless from and against any claim, demand, suit, action, cause of action, loss, damage, liability, reasonable attorneys' fees and other costs and expenses incurred by WEB-EST as the result of any violation of this Agreement by CUSTOMER or any of its directors, officers, employees, agents, members, representatives or contractors.</p>
        <p class=""section-title"">13.  Miscellaneous Provisions.</p>
        <p class=""section-item"">13.01  Entire Agreement. This Agreement includes any exhibits or schedules attached hereto and sets forth the entire agreement and understanding between the parties as to the subject matter hereof and supersedes all prior discussions or agreements, including any electronic or Internet-based agreements, between them, oral or written, concerning the subject of this Agreement. THIS AGREEMENT MAY NOT BE AMENDED OR MODIFIED EXCEPT BY A WRITTEN AMENDMENT SIGNED BY CUSTOMER AND AN OFFICER OF WEB-EST. </p>
        <p class=""section-item"">13.02  Severability. In the event that one or more provisions of this Agreement is found to be invalid, void or unenforceable, such provision shall be removed from the Agreement and the remaining provisions shall be unaffected thereby. </p>
        <p class=""section-item"">13.03  Waiver. Failure of either party hereto to enforce, at any time, any term of this Agreement shall not be a waiver of that party's right thereafter to enforce each and every term of this Agreement. </p>
        <p class=""section-item"">13.03  Force Majeure. Neither party shall be considered in default in performance of any obligations hereunder if performance of such obligations is prevented or delayed by acts of God or government, acts of terrorism, failure or delay of transportation, or by any other similar cause or causes beyond its reasonable control. </p>
        <p class=""section-item"">13.04  Assignment. CUSTOMER may not assign its rights or delegate its duties hereunder, whether by operation of law or otherwise, without the prior written consent of WEB-EST, which may be withheld at the sole discretion of WEB-EST. Any assignment by CUSTOMER without WEB-EST'S written consent shall be null and void. In addition to outright assignment, the following shall be deemed to be an attempted assignment of this Agreement: (i) the merger of CUSTOMER with another entity; (ii) the sale or transfer of more than fifty percent (50%) of CUSTOMER's capital stock within any six (6) month period; (iii) any transfer of this Agreement occurring by operation of law; or (iv) any similar transfer or transaction. If CUSTOMER sells its business by means of either stock or asset sale, the new owner (if approved by WEB-EST in its sole and absolute discretion) must sign a new End User License Agreement. If WEB-EST and the new owner enter into a new agreement, this Agreement shall be terminated and CUSTOMER shall have no future liability hereunder, but shall remain liable for any amounts due prior to the effective date of such new agreement. In all other circumstances, CUSTOMER shall remain liable under this Agreement. WEB-EST may assign this Agreement to any third party without consent, provided that such party agrees to assume WEB- EST'S obligations hereunder. </p>
        <p class=""section-item"">13.05  Applicable Law. This Agreement shall be deemed made, executed and delivered in the State of Florida and will be governed and construed for all purposes in accordance with the laws of the State of Florida without giving effect to the conflict of laws provisions thereof. CUSTOMER and WEB-EST agree that the state courts located in Pinellas County, Florida and the federal courts of the Middle District of Florida (Tampa Division) shall have the exclusive jurisdiction and venue to hear and resolve all disputes arising hereunder.  Both parties hereto hereby submit to the exclusive jurisdiction of such courts. </p>
        <p class=""section-item"">13.06  UCC Article 2A Disclaimer.  No rights or remedies set forth in Article 2A of the Uniform Commercial Code shall be conferred on CUSTOMER unless expressly granted by WEB-EST in writing.</p>
        <p class=""section-item"">13.07  Author of Agreement. The parties acknowledge that this Agreement was drafted by counsel for WEB-EST. CUSTOMER warrants to WEB-EST that it has had the opportunity to review the content and meaning of this Agreement with its counsel or has voluntarily elected not to do so. In no event shall any ambiguity in this Agreement be construed against the drafter hereof. </p>
        <p class=""section-item"">13.08  Conflict Between Agreement and Exhibits. In the event of any inconsistency between the terms of any Exhibit to this Agreement and the other terms of this Agreement, the terms of the Exhibit shall be deemed to be the controlling provision. </p>
        <p class=""section-item"">13.09  Authorization and Understanding. The person executing this Agreement on behalf of CUSTOMER hereby represents and warrants, under penalty of perjury, that he/she is duly authorized by CUSTOMER to enter into this Agreement on behalf of CUSTOMER. Execution of the Agreement by CUSTOMER confirms that CUSTOMER has received and was provided an opportunity to review the entire Agreement (all 4 pages, plus Exhibit A).</p>
        <p class=""section-item"">13.19  Counterparts; Electronic Signatures. This Agreement may be signed in two (2) or more counterparts, each of which shall be deemed an original, but all of which shall be constitute one and the same instrument. The signatures contained in this Agreement may be either original or facsimile signatures, including an emailed signature page in .pdf format, and the fact that any signature is not an original shall not affect the enforceability or legality of this Agreement.</p>
<br /><br /><br />
<p class=""section-title"">WEB-EST, LLC:</p>
	<p><img src=""data:image/png;base64,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"" style=""margin: 0px; padding: 0px; display: inline;""/> </p>
	<p>Carey Paris, President (Effective Date) " + mainContract.EffectiveDate.ToShortDateString() + @" </p>
	<p>THIS AGREEMENT TAKES EFFECT ONLY UPON BEING SIGNED BY AN AUTHORIZED REPRESENTATIVE OF WEB-EST. WEB-EST'S TERRITORY REPRESENTATIVES AND THEIR AGENTS DO NOT HAVE THE AUTHORITY TO BIND WEB-EST, WHETHER IN WRITING OR ORALLY.</p>
    </div>
    <br /><br />
    <div style=""max-width: 700px; margin: 0px auto;"">
        <p class=""section-title"">Special Terms/Conditions:</p>
        <p>Signee warrants their company/organization is not currently a Mitchell International customer and has not been one for at least 90 days prior to subscribing to Web-Est's estimating product.  Failure to comply with this special condition may result in an immediate termination of this agreement at the sole discretion of Web-Est and will result in a forfeiture of all monies paid by the customer.</p>
    </div>
<br /><br />
    <div style=""max-width: 700px; margin: 0px auto;"">
        <p>" + (string.IsNullOrEmpty(this.Name) ? "" : "Signed: <i>" + this.Name + "</i>") + @"</p>
    </div>
            ");

            Contact contact = Contact.GetContactForLogins(this.LoginID);
            if (contact != null)
            {
                Address address = Address.GetForContact(contact.ContactID);
                LoginInfo loginInfo = LoginInfo.GetByID(LoginID);

                builder.AppendLine(@"
        <div class=""contract-table-wrapper"">           

            <table>
                <thead>
                    <tr>
                        <td>Customer</td>
                        <td>Address</td>
                    </tr>
                </thead>

                <tbody>
                    <tr>
                        <td>" + (!string.IsNullOrEmpty(loginInfo.CompanyName) ? "Business Name: " + loginInfo.CompanyName + "<br />" : "") + @"
                            " + contact.FirstName + " " + contact.LastName + (string.IsNullOrEmpty(contact.Title) ? "" : " - " + contact.Title) + @"<br />
                            " + contact.Phone + @"
                        </td>
                        <td>
                            " + address.Line1 + Environment.NewLine + address.Line2 + @"
                            <br />" + address.City + ", " + address.State + " " + address.Zip + @"
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>");

                if (mainContract != null)
                {
                    List<ContractAddOn> addOns = ContractAddOn.GetForContract(mainContract.ID);
                    if (addOns.Count() >= 1)
                    {
                        builder.AppendLine(@"<br/><br/><h1>EXHIBIT A</h1>");
                    }
                    else
                    {
                        builder.AppendLine(@"<h1>EXHIBIT A</h1>");
                    }
                }
                else
                {
                    builder.AppendLine(@"<h1>EXHIBIT A</h1>");
                }

                builder.AppendLine(@"<div class=""contract-table-wrapper"">
            
            <table>
                <thead>
                    <tr>
                        <td>Product</td>
                        <td>Product Description</td>
                        <td>Payment Terms</td>
                    </tr>
                </thead>
                <tbody>
                ");

                if (mainContract != null)
                {
                    builder.AppendLine(@"
                    <tr>
                        <td>Estimatics</td>
                        <td>" + mainContract.ContractPriceLevel.ContractTerms.TermDescription + @"</td>
                        <td>" + (mainContract.ContractPriceLevel.ContractTerms.DepositAmount > 0 ? "Deposit " + mainContract.ContractPriceLevel.ContractTerms.DepositAmount.ToString("C2") + "<br />" : "") + mainContract.ContractPriceLevel.ContractTerms.NumberOfPayments + " Payments of " + mainContract.ContractPriceLevel.PaymentAmount.ToString("C2") + @"</td>
                    </tr>");

                    List<ContractAddOn> addOns = ContractAddOn.GetForContract(mainContract.ID);
                    foreach (ContractAddOn addOn in addOns)
                    {
                        builder.AppendLine(@"
                        <tr>
                            <td>" + addOn.AddOnType.Type + (addOn.Quantity > 1 ? " (" + addOn.Quantity.ToString() + ")": "") + @"</td>
                            <td>" + addOn.AddOnType.Description + @"</td>
                            <td>" + (addOn.Quantity > 1 && addOn.PriceLevel.ContractTerms.DepositAmount > 0 ? addOn.Quantity.ToString() + " x " : "") + (addOn.PriceLevel.ContractTerms.DepositAmount > 0 ? "Deposit " + addOn.PriceLevel.ContractTerms.DepositAmount.ToString("C2") + "<br />" : "") + (addOn.Quantity > 1 ? addOn.Quantity.ToString() + " x " : "") + addOn.PriceLevel.ContractTerms.NumberOfPayments + " Payments of " + addOn.PriceLevel.PaymentAmount.ToString("C2") + @"</td>
                        </tr>");
                    }
                }

                builder.AppendLine(@"
                </tbody>
            </table>
        </div>");
            }

            builder.AppendLine("</div>");

            if (includePageWrapper)
            {
                builder.AppendLine("</body>");
            }

            return builder.ToString();
        }

    }
}
