using System.Globalization;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Listener.Models;
using System.Text.Json;
using System.Fabric;

namespace Listener.Services
{
    internal class ApiService
    {
        private long GenerateIntGUID()
        {
            var day = DateTime.Now.Day + (100 * DateTime.Now.Month) + (10000 * DateTime.Now.Year);
            var hour = DateTime.Now.Millisecond +
                (1000 * DateTime.Now.Second) +
                (100000 * DateTime.Now.Minute) +
                (10000000 * DateTime.Now.Hour);
            return hour + (1000000000L * day);
        }

        public async Task<TrackerEventLog> Post(GPSTrackerMessageData data)
        {
            var context = FabricRuntime.GetActivationContext();
            var configSettings = context.GetConfigurationPackageObject("Config").Settings;
            var configData = configSettings.Sections["AppConfigSection"];
            var url = configData.Parameters["APIServiceUrl"].Value;

            try
            {
                var guid = GenerateIntGUID();

                var item = new TrackerEvent()
                {
                    id = guid,
                    name = data.Name
                };
                item.createdat = DateTime.Now;
                item.user = data.EquipmentID;
                item.title = data.EquipmentID;

                item.gpsdate = data.Date;
                item.gpstime = data.Time;
                item.batterylevel = data.BatteryLevel;
                item.direction = data.Direction;
                item.elevation = data.Elevation;
                item.equipmentid = data.EquipmentID;
                item.gpsdataeffective = data.GpsDataEffective;
                item.gsmlevel = data.GsmLevel;
                item.gsmmcc = data.GsmMCC;
                item.gsmmnc = data.GsmMNC;
                item.latlogo = data.LatLogo;
                item.lnglogo = data.LngLogo;
                item.planthestep = data.PlanTheStep;
                item.rolllsonfoot = data.RolllsOnFoot;
                item.satlevel = data.SatLevel;
                item.speeddec = data.SpeedDec;
                item.trackerstate = data.TrackerState;
                item.gps_data = data.GpsData;

                var lat = 0d;
                if (data.Lat != null)
                    lat = double.Parse(data.Lat, CultureInfo.InvariantCulture);
                var lng = 0d;
                if (data.Lng != null)
                    lng = double.Parse(data.Lng, CultureInfo.InvariantCulture);

                item.lat = lat;
                if (!string.IsNullOrEmpty(item.lnglogo) && item.lnglogo.Equals("W"))
                    item.lng = -lng;
                else
                    item.lng = lng;

                var json = JsonSerializer.Serialize(item);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var contentBytes1 = Encoding.UTF8.GetBytes(json);
                    var inputData1 = new ByteArrayContent(contentBytes1);
                    inputData1.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = client.PostAsync(url, inputData1).Result;

                    var reply = string.Empty;
                    if (response != null)
                        reply = await response.Content.ReadAsStringAsync();
                    else
                        throw new InvalidOperationException("No response");

                    if (response.StatusCode != HttpStatusCode.Created)
                    {
                        return new TrackerEventLog() { destinationEvent = null, sourceEvent = json, error = reply };
                    }
                    else
                    {
                        var retJson = JsonSerializer.Deserialize<TrackerEvent>(reply);
                        return new TrackerEventLog() { destinationEvent = retJson, sourceEvent = json, error = string.Empty };
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }

        }
    }
}

