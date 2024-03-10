using System.Configuration;
using System.Xml.Serialization;

namespace ProEstimator.Business.Model
{
   [XmlRoot(ElementName = "carfax-request", Namespace = null)]
    public class CarfaxRequest
    {
        [XmlElement(ElementName = "license-plate")]
        public string Licenseplate { get; set; }
        [XmlElement(ElementName = "state")]
        public string State { get; set; }
        [XmlElement(ElementName = "product-data-id")]
        public string Productdataid { get; set; }
        [XmlElement(ElementName = "location-id")]
        public string Locationid { get; set; }
        [XmlElement(ElementName = "vin")]
        public string Vin { get; set; }

        public CarfaxRequest()
        {
            Productdataid = ConfigurationManager.AppSettings["productDataId"];
            Locationid = ConfigurationManager.AppSettings["locationId"];
        }
    }
}
