using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ProEstimator.Business.Model;
using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.Business.Logic
{
    public class CarfaxService
    {
        #region fields

        private static HttpClient _client;
        private const string Path = "https://quickvin.carfax.com/1";

        #endregion

        #region constructors

        public CarfaxService()
        {
            _client = new HttpClient();
        }

        #endregion

        /// <summary>
        /// This method accepts a vehicle plate number and state to perform a carfax lookup to return specific data about the vehicle
        /// </summary>
        /// <param name="plateNumber"></param>
        /// <param name="state"></param>
        /// <returns>A string value for the vin</returns>
        public async Task<string> GetVinByPlateAndState(string plateNumber, string state)
        {
            var result = string.Empty;
            var request = new CarfaxRequest
            {
                Licenseplate = plateNumber,
                State = state
            };
            Carfaxresponse response = await GetCarfaxInfo(request);
            if (response != null && response.Quickvinplus != null)
            {
                result = response.Quickvinplus.Vininfo.Vin;
            }
            return result;
        }

        public async Task<Carfaxresponse> GetInfoByPlateAndState(string plateNumber, string state)
        {
            var request = new CarfaxRequest
            {
                Licenseplate = plateNumber,
                State = state
            };
            return await GetCarfaxInfo(request);
        }

        public async Task<Carfaxresponse> GetInfoByVin(string vin)
        {
            var request = new CarfaxRequest
            {
                Vin = vin
            };
            return await GetCarfaxInfo(request);
        }

        private async Task<Carfaxresponse> GetCarfaxInfo(CarfaxRequest request)
        {
            Carfaxresponse response;

            var settings = new XmlWriterSettings { OmitXmlDeclaration = true };
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            var requestSerializer = new XmlSerializer(typeof(CarfaxRequest));

            HttpResponseMessage httpResponse;
            using (var sw = new StringWriter())
            {
                using (var writer = XmlWriter.Create(sw, settings))
                {
                    requestSerializer.Serialize(writer, request, ns);
                    httpResponse = await _client.PostAsync(Path, new StringContent(sw.ToString()));
                    httpResponse.EnsureSuccessStatusCode();
                }
            }

            var contents = await httpResponse.Content.ReadAsStringAsync();

            using (var str = new StringReader(contents))
            {
                var xmlSerializer = new XmlSerializer(typeof(Carfaxresponse));
                response = (Carfaxresponse)xmlSerializer.Deserialize(str);
            }

            if(response != null && response.Errormessages == "")
            {
                CarfaxInfo vehInfo = new CarfaxInfo();
                string vin = response.Quickvinplus.Vininfo.Vin;
                vehInfo.Vin = vin;
                vehInfo.VinInfo = contents;
                CarfaxInfo vinInfo = CarfaxInfo.GetByVin(vin);
                if(vinInfo != null)
                {
                    vehInfo.ID = vinInfo.ID;
                }
                vehInfo.Save();
            }

            return response;
        }

        public List<VehicleInfoTypeData> GetExistingInfo(string vinInfo)
        {
            List<VehicleInfoTypeData> vinInfoList = new List<VehicleInfoTypeData>();
            XmlReader vinReader = XmlReader.Create(new System.IO.StringReader(vinInfo));
            int i = 1;
            while (vinReader.Read())
            {
                if (vinReader.NodeType == XmlNodeType.Element)
                {
                    if (vinReader.HasAttributes)
                    {
                        vinReader.MoveToFirstAttribute();
                        if(i > 1)
                        {
                            vinInfoList.Add(new VehicleInfoTypeData("", ""));
                        }
                        vinInfoList.Add(new VehicleInfoTypeData("<b>" + i.ToString() + ".</b>", ""));
                        vinInfoList.Add(new VehicleInfoTypeData(vinReader.LocalName, vinReader.Value));
                        i++;
                    }
                    else
                    {   
                        string type = vinReader.LocalName;
                        vinReader.Read();
                        if (vinReader.NodeType != XmlNodeType.Whitespace && vinReader.Value != "")
                        {
                            vinInfoList.Add(new VehicleInfoTypeData(type, vinReader.Value));
                        }
                        if (type == "request-info")
                        {
                            while(vinReader.LocalName != "request-info")
                            {
                                vinReader.Read();
                            }
                        }
                    }
                }
            }

            return vinInfoList;
        }

        public string GetVinByPlateAndStateASD(string plateNumber, string state)
        {
            var result = string.Empty;
            var request = new CarfaxRequest
            {
                Licenseplate = plateNumber,
                State = state
            };

            var settings = new XmlWriterSettings { OmitXmlDeclaration = true };
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            var requestSerializer = new XmlSerializer(typeof(CarfaxRequest));

            using (var sw = new StringWriter())
            {
                using (var writer = XmlWriter.Create(sw, settings))
                {
                    requestSerializer.Serialize(writer, request, ns);
                    using (var wb = new WebClient())
                    {
                        var res = wb.UploadString(Path, "POST", sw.ToString());
                        Carfaxresponse response;
                        using (var str = new StringReader(res))
                        {
                            var xmlSerializer = new XmlSerializer(typeof(Carfaxresponse));
                            response = (Carfaxresponse)xmlSerializer.Deserialize(str);
                        }

                        if (response != null && response.Quickvinplus != null)
                        {
                            result = response.Quickvinplus.Vininfo.Vin;
                        }
                    }
                }
            }

            return result;
        }
    }

    public class VehicleInfoTypeData
    {
        public string Type { get; set; }
        public string Data { get; set; }
        public VehicleInfoTypeData(string type, string data)
        {
            Type = type;
            Data = data;
        }
    }

    public class VehicleFunctionResult : FunctionResult
    {
        public List<VehicleInfoTypeData> VehicleInfo { get; private set; }

        public VehicleFunctionResult(List<VehicleInfoTypeData> vehicleInfo)
            : base()
        {
            VehicleInfo = vehicleInfo;
        }

        public VehicleFunctionResult(string errorMessage)
            : base(errorMessage)
        {

        }
    }
}
