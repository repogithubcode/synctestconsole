using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProEstimator.Business.Model
{
	[XmlRoot(ElementName="aces-detail")]
	public class Acesdetail {
		[XmlElement(ElementName="aces-aspiration-id")]
		public string Acesaspirationid { get; set; }
		[XmlElement(ElementName="aces-base-vehicle")]
		public string Acesbasevehicle { get; set; }
		[XmlElement(ElementName="aces-body-number-doors")]
		public string Acesbodynumberdoors { get; set; }
		[XmlElement(ElementName="aces-body-type")]
		public string Acesbodytype { get; set; }
		[XmlElement(ElementName="aces-cc-displacement")]
		public string Acesccdisplacement { get; set; }
		[XmlElement(ElementName="aces-ci-displacement")]
		public string Acescidisplacement { get; set; }
		[XmlElement(ElementName="aces-cylinders")]
		public string Acescylinders { get; set; }
		[XmlElement(ElementName="aces-drive-id")]
		public string Acesdriveid { get; set; }
		[XmlElement(ElementName="aces-engine-vin-id")]
		public string Acesenginevinid { get; set; }
		[XmlElement(ElementName="aces-enginebase-id")]
		public string Acesenginebaseid { get; set; }
		[XmlElement(ElementName="aces-engineconfig-id")]
		public string Acesengineconfigid { get; set; }
		[XmlElement(ElementName="aces-fuel")]
		public string Acesfuel { get; set; }
		[XmlElement(ElementName="aces-fuel-delivery")]
		public string Acesfueldelivery { get; set; }
		[XmlElement(ElementName="aces-liters")]
		public string Acesliters { get; set; }
		[XmlElement(ElementName="aces-make-id")]
		public string Acesmakeid { get; set; }
		[XmlElement(ElementName="aces-model-id")]
		public string Acesmodelid { get; set; }
		[XmlElement(ElementName="aces-region-id")]
		public string Acesregionid { get; set; }
		[XmlElement(ElementName="aces-sub-model-id")]
		public string Acessubmodelid { get; set; }
		[XmlElement(ElementName="aces-vehicle-id")]
		public string Acesvehicleid { get; set; }
		[XmlElement(ElementName="aces-vehicle-type-id")]
		public string Acesvehicletypeid { get; set; }
		[XmlElement(ElementName="aces-year-id")]
		public string Acesyearid { get; set; }
	}

	[XmlRoot(ElementName="aces-details")]
	public class Acesdetails {
		[XmlElement(ElementName="aces-detail")]
		public List<Acesdetail> Acesdetail { get; set; }
	}

	[XmlRoot(ElementName="trim")]
	public class Trim {
		[XmlElement(ElementName="aaia-legacy-asp")]
		public string Aaialegacyasp { get; set; }
		[XmlElement(ElementName="aaia-legacy-cc")]
		public string Aaialegacycc { get; set; }
		[XmlElement(ElementName="aaia-legacy-code-id")]
		public string Aaialegacycodeid { get; set; }
		[XmlElement(ElementName="aaia-legacy-country")]
		public string Aaialegacycountry { get; set; }
		[XmlElement(ElementName="aaia-legacy-engdesg")]
		public string Aaialegacyengdesg { get; set; }
		[XmlElement(ElementName="aaia-legacy-engtype")]
		public string Aaialegacyengtype { get; set; }
		[XmlElement(ElementName="aaia-legacy-fuel")]
		public string Aaialegacyfuel { get; set; }
		[XmlElement(ElementName="aaia-legacy-fueldel")]
		public string Aaialegacyfueldel { get; set; }
		[XmlElement(ElementName="aaia-legacy-liter")]
		public string Aaialegacyliter { get; set; }
		[XmlElement(ElementName="aaia-legacy-make")]
		public string Aaialegacymake { get; set; }
		[XmlElement(ElementName="aaia-legacy-model")]
		public string Aaialegacymodel { get; set; }
		[XmlElement(ElementName="aaia-legacy-ref-date")]
		public string Aaialegacyrefdate { get; set; }
		[XmlElement(ElementName="aaia-legacy-submodel")]
		public string Aaialegacysubmodel { get; set; }
		[XmlElement(ElementName="aaia-legacy-veh-year")]
		public string Aaialegacyvehyear { get; set; }
		[XmlElement(ElementName="aces-aaia-canada-region")]
		public string Acesaaiacanadaregion { get; set; }
		[XmlElement(ElementName="aces-aaia-usa-region")]
		public string Acesaaiausaregion { get; set; }
		[XmlElement(ElementName="aces-details")]
		public Acesdetails Acesdetails { get; set; }
		[XmlElement(ElementName="base-make-name")]
		public string Basemakename { get; set; }
		[XmlElement(ElementName="base-series-name")]
		public string Baseseriesname { get; set; }
		[XmlElement(ElementName="base-year-model")]
		public string Baseyearmodel { get; set; }
		[XmlElement(ElementName="nonoem-aspiration")]
		public string Nonoemaspiration { get; set; }
		[XmlElement(ElementName="nonoem-base-model")]
		public string Nonoembasemodel { get; set; }
		[XmlElement(ElementName="nonoem-body")]
		public string Nonoembody { get; set; }
		[XmlElement(ElementName="nonoem-cc")]
		public string Nonoemcc { get; set; }
		[XmlElement(ElementName="nonoem-cid")]
		public string Nonoemcid { get; set; }
		[XmlElement(ElementName="nonoem-country-of-origin")]
		public string Nonoemcountryoforigin { get; set; }
		[XmlElement(ElementName="nonoem-ct-body-style")]
		public string Nonoemctbodystyle { get; set; }
		[XmlElement(ElementName="nonoem-ct-number-axles")]
		public string Nonoemctnumberaxles { get; set; }
		[XmlElement(ElementName="nonoem-ct-trailer-length")]
		public string Nonoemcttrailerlength { get; set; }
		[XmlElement(ElementName="nonoem-doors")]
		public string Nonoemdoors { get; set; }
		[XmlElement(ElementName="nonoem-drive")]
		public string Nonoemdrive { get; set; }
		[XmlElement(ElementName="nonoem-eng-name")]
		public string Nonoemengname { get; set; }
		[XmlElement(ElementName="nonoem-engine-mfg")]
		public string Nonoemenginemfg { get; set; }
		[XmlElement(ElementName="nonoem-engine-vin")]
		public string Nonoemenginevin { get; set; }
		[XmlElement(ElementName="nonoem-fuel")]
		public string Nonoemfuel { get; set; }
		[XmlElement(ElementName="nonoem-head")]
		public string Nonoemhead { get; set; }
		[XmlElement(ElementName="nonoem-induction")]
		public string Nonoeminduction { get; set; }
		[XmlElement(ElementName="nonoem-induction-type")]
		public string Nonoeminductiontype { get; set; }
		[XmlElement(ElementName="nonoem-liter")]
		public string Nonoemliter { get; set; }
		[XmlElement(ElementName="nonoem-make")]
		public string Nonoemmake { get; set; }
		[XmlElement(ElementName="nonoem-market")]
		public string Nonoemmarket { get; set; }
		[XmlElement(ElementName="nonoem-selectivity")]
		public string Nonoemselectivity { get; set; }
		[XmlElement(ElementName="nonoem-speedometer-digits")]
		public string Nonoemspeedometerdigits { get; set; }
		[XmlElement(ElementName="nonoem-submodel1")]
		public string Nonoemsubmodel1 { get; set; }
		[XmlElement(ElementName="nonoem-submodel2")]
		public string Nonoemsubmodel2 { get; set; }
		[XmlElement(ElementName="nonoem-submodel3")]
		public string Nonoemsubmodel3 { get; set; }
		[XmlElement(ElementName="nonoem-submodel4")]
		public string Nonoemsubmodel4 { get; set; }
		[XmlElement(ElementName="nonoem-submodel5")]
		public string Nonoemsubmodel5 { get; set; }
		[XmlElement(ElementName="nonoem-submodel6")]
		public string Nonoemsubmodel6 { get; set; }
		[XmlElement(ElementName="nonoem-submodels")]
		public string Nonoemsubmodels { get; set; }
		[XmlElement(ElementName="nonoem-type")]
		public string Nonoemtype { get; set; }
		[XmlElement(ElementName="nonoem-valves")]
		public string Nonoemvalves { get; set; }
		[XmlElement(ElementName="nonoem-vin-header")]
		public string Nonoemvinheader { get; set; }
		[XmlElement(ElementName="nonoem-wheel-base")]
		public string Nonoemwheelbase { get; set; }
		[XmlElement(ElementName="nonoem-year")]
		public string Nonoemyear { get; set; }
		[XmlElement(ElementName="oem-aaia-code-source")]
		public string Oemaaiacodesource { get; set; }
		[XmlElement(ElementName="oem-air-conditioning")]
		public string Oemairconditioning { get; set; }
		[XmlElement(ElementName="oem-antilock-brakes")]
		public string Oemantilockbrakes { get; set; }
		[XmlElement(ElementName="oem-aspiration")]
		public string Oemaspiration { get; set; }
		[XmlElement(ElementName="oem-base-list-price")]
		public string Oembaselistprice { get; set; }
		[XmlElement(ElementName="oem-base-model")]
		public string Oembasemodel { get; set; }
		[XmlElement(ElementName="oem-base-shipping-weight")]
		public string Oembaseshippingweight { get; set; }
		[XmlElement(ElementName="oem-body-type")]
		public string Oembodytype { get; set; }
		[XmlElement(ElementName="oem-brakes-code")]
		public string Oembrakescode { get; set; }
		[XmlElement(ElementName="oem-cab-configuration")]
		public string Oemcabconfiguration { get; set; }
		[XmlElement(ElementName="oem-carburetion")]
		public string Oemcarburetion { get; set; }
		[XmlElement(ElementName="oem-carfax-filler")]
		public string Oemcarfaxfiller { get; set; }
		[XmlElement(ElementName="oem-country-of-origin")]
		public string Oemcountryoforigin { get; set; }
		[XmlElement(ElementName="oem-cylinders")]
		public string Oemcylinders { get; set; }
		[XmlElement(ElementName="oem-daytime-running-lights")]
		public string Oemdaytimerunninglights { get; set; }
		[XmlElement(ElementName="oem-displacement")]
		public string Oemdisplacement { get; set; }
		[XmlElement(ElementName="oem-driving-wheels")]
		public string Oemdrivingwheels { get; set; }
		[XmlElement(ElementName="oem-engine-information")]
		public string Oemengineinformation { get; set; }
		[XmlElement(ElementName="oem-exception-flag")]
		public string Oemexceptionflag { get; set; }
		[XmlElement(ElementName="oem-front-axle-code")]
		public string Oemfrontaxlecode { get; set; }
		[XmlElement(ElementName="oem-fuel")]
		public string Oemfuel { get; set; }
		[XmlElement(ElementName="oem-full-body-style")]
		public string Oemfullbodystyle { get; set; }
		[XmlElement(ElementName="oem-gvw")]
		public string Oemgvw { get; set; }
		[XmlElement(ElementName="oem-high-performance-code")]
		public string Oemhighperformancecode { get; set; }
		[XmlElement(ElementName="oem-induction")]
		public string Oeminduction { get; set; }
		[XmlElement(ElementName="oem-ma-state-exceptions")]
		public string Oemmastateexceptions { get; set; }
		[XmlElement(ElementName="oem-make-abbrev")]
		public string Oemmakeabbrev { get; set; }
		[XmlElement(ElementName="oem-ncic-make-abbrev")]
		public string Oemncicmakeabbrev { get; set; }
		[XmlElement(ElementName="oem-ncic-series-abbrev")]
		public string Oemncicseriesabbrev { get; set; }
		[XmlElement(ElementName="oem-nvpp-make-abbrev")]
		public string Oemnvppmakeabbrev { get; set; }
		[XmlElement(ElementName="oem-nvpp-make-code")]
		public string Oemnvppmakecode { get; set; }
		[XmlElement(ElementName="oem-nvpp-series-model")]
		public string Oemnvppseriesmodel { get; set; }
		[XmlElement(ElementName="oem-nvpp-series-name")]
		public string Oemnvppseriesname { get; set; }
		[XmlElement(ElementName="oem-power-brakes")]
		public string Oempowerbrakes { get; set; }
		[XmlElement(ElementName="oem-power-steering")]
		public string Oempowersteering { get; set; }
		[XmlElement(ElementName="oem-power-windows")]
		public string Oempowerwindows { get; set; }
		[XmlElement(ElementName="oem-price-variance")]
		public string Oempricevariance { get; set; }
		[XmlElement(ElementName="oem-proactive-vin-indicator")]
		public string Oemproactivevinindicator { get; set; }
		[XmlElement(ElementName="oem-radio")]
		public string Oemradio { get; set; }
		[XmlElement(ElementName="oem-rear-axle-code")]
		public string Oemrearaxlecode { get; set; }
		[XmlElement(ElementName="oem-restraint-type")]
		public string Oemrestrainttype { get; set; }
		[XmlElement(ElementName="oem-roof")]
		public string Oemroof { get; set; }
		[XmlElement(ElementName="oem-security-system")]
		public string Oemsecuritysystem { get; set; }
		[XmlElement(ElementName="oem-segmentation-code")]
		public string Oemsegmentationcode { get; set; }
		[XmlElement(ElementName="oem-series-abbrev")]
		public string Oemseriesabbrev { get; set; }
		[XmlElement(ElementName="oem-tilt-wheel")]
		public string Oemtiltwheel { get; set; }
		[XmlElement(ElementName="oem-tire-size")]
		public string Oemtiresize { get; set; }
		[XmlElement(ElementName="oem-ton-rating")]
		public string Oemtonrating { get; set; }
		[XmlElement(ElementName="oem-transmission")]
		public string Oemtransmission { get; set; }
		[XmlElement(ElementName="oem-transmission-type")]
		public string Oemtransmissiontype { get; set; }
		[XmlElement(ElementName="oem-tx-state-exceptions")]
		public string Oemtxstateexceptions { get; set; }
		[XmlElement(ElementName="oem-valves")]
		public string Oemvalves { get; set; }
		[XmlElement(ElementName="oem-vehicle-type")]
		public string Oemvehicletype { get; set; }
		[XmlElement(ElementName="oem-vin-pattern")]
		public string Oemvinpattern { get; set; }
		[XmlElement(ElementName="oem-vin-select-pattern")]
		public string Oemvinselectpattern { get; set; }
		[XmlElement(ElementName="oem-weight-variance")]
		public string Oemweightvariance { get; set; }
		[XmlElement(ElementName="oem-wheel-base")]
		public string Oemwheelbase { get; set; }
		[XmlElement(ElementName="oemopt-optional-radio1")]
		public string Oemoptoptionalradio1 { get; set; }
		[XmlElement(ElementName="oemopt-optional-radio2")]
		public string Oemoptoptionalradio2 { get; set; }
		[XmlElement(ElementName="oemopt-optional-roof1")]
		public string Oemoptoptionalroof1 { get; set; }
		[XmlElement(ElementName="oemopt-optional-roof2")]
		public string Oemoptoptionalroof2 { get; set; }
		[XmlElement(ElementName="oemopt-optional-transmission1")]
		public string Oemoptoptionaltransmission1 { get; set; }
		[XmlElement(ElementName="oemopt-optional-transmission2")]
		public string Oemoptoptionaltransmission2 { get; set; }
		[XmlElement(ElementName="oemtruck-truck-driving-wheels")]
		public string Oemtrucktruckdrivingwheels { get; set; }
		[XmlElement(ElementName="oemtruck-truck-engine-mfg")]
		public string Oemtrucktruckenginemfg { get; set; }
		[XmlElement(ElementName="oemtruck-truck-engine-model")]
		public string Oemtrucktruckenginemodel { get; set; }
		[XmlElement(ElementName="oemtruck-truck-engine-type-code")]
		public string Oemtrucktruckenginetypecode { get; set; }
		[XmlElement(ElementName="oemtruck-truck-wheels")]
		public string Oemtrucktruckwheels { get; set; }
		[XmlAttribute(AttributeName="submodel")]
		public string Submodel { get; set; }
	}

	[XmlRoot(ElementName="carfax-vin-decode")]
	public class Carfaxvindecode {
		[XmlElement(ElementName="trim")]
		public List<Trim> Trim { get; set; }
	}

	[XmlRoot(ElementName="vin-info")]
	public class Vininfo {
		[XmlElement(ElementName="carfax-vin-decode")]
		public Carfaxvindecode Carfaxvindecode { get; set; }
		[XmlElement(ElementName="vin")]
		public string Vin { get; set; }
	}

	[XmlRoot(ElementName="quickvinplus")]
	public class Quickvinplus {
		[XmlElement(ElementName="vin-info")]
		public Vininfo Vininfo { get; set; }
	}

	[XmlRoot(ElementName="request-info")]
	public class Requestinfo {
		[XmlElement(ElementName="location-id")]
		public string Locationid { get; set; }
		[XmlElement(ElementName="vin")]
		public string Vin { get; set; }
		[XmlElement(ElementName="license-plate")]
		public string Licenseplate { get; set; }
		[XmlElement(ElementName="state")]
		public string State { get; set; }
	}

	[XmlRoot(ElementName="carfax-response")]
	public class Carfaxresponse {
		[XmlElement(ElementName="quickvinplus")]
		public Quickvinplus Quickvinplus { get; set; }
		[XmlElement(ElementName="error-messages")]
		public string Errormessages { get; set; }
		[XmlElement(ElementName="request-info")]
		public Requestinfo Requestinfo { get; set; }
	}
}
