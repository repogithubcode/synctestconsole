using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class EmailTemplate : ProEstEntity 
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Subject { get; set; }
        public string Template { get; set; }

        public EmailTemplate()
        {

        }

        public EmailTemplate(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Name = InputHelper.GetString(row["Name"].ToString());
            Description = InputHelper.GetString(row["Description"].ToString());
            Subject = InputHelper.GetString(row["Subject"].ToString());
            Template = InputHelper.GetString(row["Template"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("Name", Name));
            parameters.Add(new SqlParameter("Description", Description));
            parameters.Add(new SqlParameter("Subject", Subject));
            parameters.Add(new SqlParameter("Template", Template));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("EmailTemplate_Save", parameters);
            
            if (intResult.Success)
            {
                ID = intResult.Value;

                ChangeLogManager.LogChange(activeLoginID, "EmailTemplate", ID, 0, parameters, RowAsLoaded);
            }

            return new SaveResult(intResult);
        }

        public static List<EmailTemplate> GetAll()
        {
            List<EmailTemplate> results = new List<EmailTemplate>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("EmailTemplate_GetAll");

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                results.Add(new EmailTemplate(row));
            }

            return results;
        }

        public static EmailTemplate Get(int id)
        {
            return GetAll().FirstOrDefault(o => o.ID == id);
        }

        public static EmailTemplate GetByName(string name)
        {
            return GetAll().FirstOrDefault(o => o.Name == name);
        }

        /// <summary>
        /// Searches the email template for tags like #Tag# and returns a list of strings.
        /// </summary>
        public List<string> GetAllTags(string temp)
        {
            List<string> tags = new List<string>();

            string buffer = "";
            bool inTag = false;

            string tempWithTags = Template;
            if (temp != "") tempWithTags = temp;

            foreach (char character in tempWithTags)
            {
                if (!inTag)
                {
                    // Tag start found
                    if (character =='#')
                    {
                        inTag = true;
                        buffer = "";
                    }
                }
                else
                {
                    if (character == '#')
                    {
                        // Tag end found
                        if (!tags.Contains(buffer))
                        {
                            tags.Add(buffer);
                            inTag = false;
                            buffer = "";
                        }
                    }
                    else if (char.IsLetterOrDigit(character))
                    {
                        // Tags can only have numbers and letters
                        buffer += character;
                    }
                    else
                    {
                        // Tags end and are ignored if there's anything other than a number or letter inside.
                        inTag = false;
                        buffer = "";
                    }
                }
            }

            return tags;
        }

        public string ProcessTemplate(List<TagAndValue> tags, string temp = "")
        {
            StringBuilder errors = new StringBuilder();

            List<string> allTags = GetAllTags(temp);

            string processed = Template;
            if (temp != "") processed = temp;

            foreach (TagAndValue tag in tags)
            {
                if (processed.Contains("#" + tag.Tag + "#"))
                {
                    processed = processed.Replace("#" + tag.Tag + "#", tag.Value);

                    if (allTags.Contains(tag.Tag))
                    {
                        allTags.Remove(tag.Tag);
                    }
                }
                else
                {
                    errors.AppendLine("Tag '" + tag.Tag + "' not found in email template " + ID);
                }
            }

            foreach(string tag in allTags)
            {
                errors.AppendLine("Tag '" + tag + "' not replaced in template " + ID);
            }

            return processed;
        }
    }

    public class TagAndValue
    {
        public string Tag { get; set; }
        public string Value { get; set; }

        public TagAndValue()
        {

        }

        public TagAndValue(string tag, string value)
        {
            Tag = tag;
            Value = value;
        }
    }
}
