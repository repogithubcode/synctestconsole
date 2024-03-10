using System.ComponentModel.DataAnnotations;

namespace Proestimator.DataAttributes
{
    public class CustomEmailAddressAttribute : DataTypeAttribute
    {

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }
            var input = value as string;
            var emailAddressAttribute = new EmailAddressAttribute();

            return (input != null) && (string.IsNullOrEmpty(input) || emailAddressAttribute.IsValid(input));
        }

        public CustomEmailAddressAttribute(DataType dataType) : base(dataType)
        {
        }

        public CustomEmailAddressAttribute(string customDataType) : base(customDataType)
        {
        }
    }
}