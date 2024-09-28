using MDO2.Core.SM.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MDO2.Core.SM
{
    public class SMClient
    {
        public SMClient()
        {

        }
        public SMClient(string baseUrl, string token)
        {
            var url = baseUrl;
            if (!baseUrl.EndsWith("/"))
                url = baseUrl + "/";

            BaseUrl = new Uri(url);
            Token = token;
        }

        private async Task<GetReportListResponse> GetReportListInternal(string entityId, string projectId, string hotelCode = null, string hotelId = null, string hotelName = null)
        {


            using (var client = GetHttpClient())
            {
                var queryParam = System.Web.HttpUtility.ParseQueryString(string.Empty);
                queryParam.Add("entity_id", entityId);
                queryParam.Add("project_id", projectId.ToString());
                if (!string.IsNullOrWhiteSpace(hotelId)) queryParam.Add("hotel_id", hotelId.ToString());
                else if (!string.IsNullOrWhiteSpace(hotelCode)) queryParam.Add("hotel_code", hotelCode.ToString());
                else if (!string.IsNullOrWhiteSpace(hotelName)) queryParam.Add("hotel_name", hotelName.ToString());
                var query = queryParam.ToString();

                var resp = await client.GetAsync($"reports/get_report_list?{query}");
                if (resp.IsSuccessStatusCode)
                {
                    var respTxt = await resp.Content.ReadAsStringAsync();
                    var respObj = JsonConvert.DeserializeObject<GetReportListResponse>(respTxt);
                    if (!respObj.Error)
                    {
                        return respObj;
                    }
                    else
                    {
                        throw new SMClientException($"GetReportListInternal request failed due to SM reported as error. " +
                            $"SM response message is {respObj.Message}");
                    }
                }
                else
                {
                    throw new SMClientException($"GetReportListInternal request failed due to http status returned {resp.StatusCode}");
                }
            }
        }
        private async Task<GetProjectHotelListResponse> GetProjectHotelListInternal(string projectDbId, string entityId, string projectId)
        {
            using (var client = GetHttpClient())
            {
                var queryParam = System.Web.HttpUtility.ParseQueryString(string.Empty);
                if (!string.IsNullOrWhiteSpace(projectDbId)) queryParam.Add("proj_id", projectDbId);
                if (!string.IsNullOrWhiteSpace(projectId)) queryParam.Add("project_id", projectId);
                if (!string.IsNullOrWhiteSpace(entityId)) queryParam.Add("entity_id", entityId);
                var query = queryParam.ToString();

                var resp = await client.GetAsync($"project/get_project_hotel_list?{query}");
                if (resp.IsSuccessStatusCode)
                {
                    var respTxt = await resp.Content.ReadAsStringAsync();
                    var respObj = JsonConvert.DeserializeObject<GetProjectHotelListResponse>(respTxt);
                    if (!respObj.Error)
                    {
                        return respObj;
                    }
                    else
                    {
                        throw new SMClientException($"GetProjectHotelListInternal request failed due to SM reported as error. " +
                            $"SM response message is {respObj.Message}");
                    }
                }
                else
                {
                    throw new SMClientException($"GetProjectHotelListInternal request failed due to http status returned {resp.StatusCode}");
                }
            }
        }
        private async Task<GetIndexMapDataResponse> GetIndexMapDataInternal(string hmgDbId, string hmgName, string entityId)
        {
            using (var client = GetHttpClient())
            {
                var queryParam = System.Web.HttpUtility.ParseQueryString(string.Empty);
                if (!string.IsNullOrWhiteSpace(hmgDbId)) queryParam.Add("hmg_id", hmgDbId);
                else if (!string.IsNullOrWhiteSpace(hmgName)) queryParam.Add("hmg_name", hmgName);
                else if (!string.IsNullOrWhiteSpace(entityId)) queryParam.Add("entity_id", entityId);
                var query = queryParam.ToString();

                var resp = await client.GetAsync($"indexes/get_index_map_data?{query}");
                if (resp.IsSuccessStatusCode)
                {
                    var respTxt = await resp.Content.ReadAsStringAsync();
                    var respObj = JsonConvert.DeserializeObject<GetIndexMapDataResponse>(respTxt);
                    if (!respObj.Error)
                    {
                        return respObj;
                    }
                    else
                    {
                        throw new SMClientException($"GetIndexMapDataInternal request failed due to SM reported as error. " +
                            $"SM response message is {respObj.Message}");
                    }
                }
                else
                {
                    throw new SMClientException($"GetIndexMapDataInternal request failed due to http status returned {resp.StatusCode}");
                }
            }
        }

        protected virtual HttpClient GetHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = BaseUrl;
            httpClient.DefaultRequestHeaders.Add("Token", $"Token {Token}");

            if (BaseUrl == null) throw new SMClientException("Base url is not set in SMClient.BaseUrl property");
            if (string.IsNullOrWhiteSpace(Token)) throw new SMClientException("Token is not set in SMClient.Token property");

            return httpClient;
        }

        public virtual async Task<GetAllHMGResponse> GetAllHMG()
        {
            using (var client = GetHttpClient())
            {
                var resp = await client.GetAsync("hmg/get_all_hmg_list");
                if (resp.IsSuccessStatusCode)
                {
                    var respTxt = await resp.Content.ReadAsStringAsync();
                    var respObj = JsonConvert.DeserializeObject<GetAllHMGResponse>(respTxt);
                    if (!respObj.Error)
                    {
                        return respObj;
                    }
                    else
                    {
                        throw new SMClientException($"GetAllHMG request failed due to SM response was set to error. " +
                            $"SM response message is {respObj.Message}");
                    }
                }
                else
                {
                    throw new SMClientException($"GetAllHMG request failed due to http status returned {resp.StatusCode}");
                }
            }
        }
        public virtual async Task<GetHMGProjectListResponse> GetHMGProjectList(string hmgId = null, string hmgName = null, int? entityId = null, int? cmpId = null)
        {
            using (var client = GetHttpClient())
            {
                var queryParam = System.Web.HttpUtility.ParseQueryString(string.Empty);
                if (!string.IsNullOrWhiteSpace(hmgId)) queryParam.Add("hmg_id", hmgId);
                if (!string.IsNullOrWhiteSpace(hmgName)) queryParam.Add("hmg_name", hmgName);
                if (cmpId.HasValue) queryParam.Add("cmp_id", cmpId.Value.ToString());
                if (entityId.HasValue) queryParam.Add("entity_id", entityId.Value.ToString());
                var query = queryParam.ToString();

                var resp = await client.GetAsync($"hmg/get_hmg_project_list?{query}");
                if (resp.IsSuccessStatusCode)
                {
                    var respTxt = await resp.Content.ReadAsStringAsync();
                    var respObj = JsonConvert.DeserializeObject<GetHMGProjectListResponse>(respTxt);
                    if (!respObj.Error)
                    {
                        return respObj;
                    }
                    else
                    {
                        throw new SMClientException($"GetHMGProjectList request failed due to SM response was set to error. " +
                            $"SM response message is {respObj.Message}");
                    }
                }
                else
                {
                    throw new SMClientException($"GetHMGProjectList request failed due to http status returned {resp.StatusCode}");
                }
            }
        }
        public virtual async Task<GetIndexResponse> GetIndex(int entityId, long projectId)
        {
            return await GetIndex(entityId.ToString(), projectId);
        }
        public virtual async Task<GetIndexResponse> GetIndex(string entityId, long projectId)
        {
            using (var client = GetHttpClient())
            {
                var queryParam = System.Web.HttpUtility.ParseQueryString(string.Empty);
                queryParam.Add("entity_id", entityId);
                queryParam.Add("project_id", projectId.ToString());
                var query = queryParam.ToString();

                var resp = await client.GetAsync($"indexes/get_index?{query}");
                if (resp.IsSuccessStatusCode)
                {
                    var respTxt = await resp.Content.ReadAsStringAsync();
                    var respObj = JsonConvert.DeserializeObject<GetIndexResponse>(respTxt);
                    if (!respObj.Error)
                    {
                        return respObj;
                    }
                    else
                    {
                        throw new SMClientException($"GetIndex request failed due to SM response was set to error. " +
                            $"SM response message is {respObj.Message}");
                    }
                }
                else
                {
                    throw new SMClientException($"GetIndex request failed due to http status returned {resp.StatusCode}");
                }
            }
        }

        public virtual Task<GetReportListResponse> GetReportList(string entityId, string projectId, string hotelCode = null, string hotelId = null, string hotelName = null)
        {
            if (string.IsNullOrWhiteSpace(entityId))
            {
                throw new ArgumentException($"'{nameof(entityId)}' cannot be null or whitespace", nameof(entityId));
            }
            if (string.IsNullOrWhiteSpace(projectId))
            {
                throw new ArgumentException($"'{nameof(projectId)}' cannot be null or whitespace", nameof(projectId));
            }
            if (string.IsNullOrWhiteSpace(hotelCode) && string.IsNullOrWhiteSpace(hotelId) && string.IsNullOrWhiteSpace(hotelName))
            {
                throw new ArgumentException($"'{nameof(hotelCode)}', '{nameof(hotelId)}' and '{nameof(hotelName)}' " +
                    $"cannot be null or whitespace. Value has to be provided at least for one parameter");
            }

            return GetReportListInternal(entityId, projectId, hotelCode, hotelId, hotelName);
        }
        public virtual Task<GetProjectHotelListResponse> GetProjectHotelList(string projectDbId)
        {
            if (string.IsNullOrWhiteSpace(projectDbId))
            {
                throw new ArgumentException($"'{nameof(projectDbId)}' cannot be null or whitespace.");
            }

            return GetProjectHotelListInternal(projectDbId, null, null);
        }
        public virtual Task<GetProjectHotelListResponse> GetProjectHotelList(string entityId, string projectId)
        {
            if (string.IsNullOrWhiteSpace(entityId) || string.IsNullOrWhiteSpace(projectId))
            {
                throw new ArgumentException($"'{nameof(projectId)}' and '{nameof(entityId)}' both " +
                      $"cannot be null or whitespace. Value has to be provided for all parameters");
            }

            return GetProjectHotelListInternal(null, entityId, projectId);
        }
        public virtual Task<GetIndexMapDataResponse> GetIndexMapDataByHmgId(string hmgId)
        {
            if (string.IsNullOrWhiteSpace(hmgId))
            {
                throw new ArgumentException($"'{nameof(hmgId)}' cannot be null or whitespace.", nameof(hmgId));
            }

            return GetIndexMapDataInternal(hmgId, null, null);
        }
        public virtual Task<GetIndexMapDataResponse> GetIndexMapDataByHmgName(string hmgName)
        {
            if (string.IsNullOrWhiteSpace(hmgName))
            {
                throw new ArgumentException($"'{nameof(hmgName)}' cannot be null or whitespace.", nameof(hmgName));
            }

            return GetIndexMapDataInternal(null, hmgName, null);
        }
        public virtual Task<GetIndexMapDataResponse> GetIndexMapDataByEntityId(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId))
            {
                throw new ArgumentException($"'{nameof(entityId)}' cannot be null or whitespace.", nameof(entityId));
            }

            return GetIndexMapDataInternal(null, null, entityId);
        }

        public Uri BaseUrl { get; set; }
        public string Token { get; set; }
    }
}
