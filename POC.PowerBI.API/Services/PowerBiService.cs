using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace POC.PowerBI.API.Services
{
    public class PowerBiService
    {
        private ITokenAcquisition tokenAcquisition { get; }
        private string urlPowerBiServiceApiRoot { get; }

        public PowerBiService(IConfiguration configuration, ITokenAcquisition tokenAcquisition)
        {
            this.urlPowerBiServiceApiRoot = configuration["PowerBi:ServiceRootUrl"];
            this.tokenAcquisition = tokenAcquisition;
        }

        public static readonly string[] RequiredScopes = new string[] {
          "https://analysis.windows.net/powerbi/api/.default"
        };

        public async Task<string> GetAccessToken()
        {
            //return await this.tokenAcquisition.GetAccessTokenForAppAsync(RequiredScopes[0]);

            var httpClient = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("scope", "openid"),
                new KeyValuePair<string, string>("resource", "https://analysis.windows.net/powerbi/api"),
                new KeyValuePair<string, string>("client_id", "97bf5718-25f0-4790-915d-d115a3ec0710"),
                new KeyValuePair<string, string>("client_secret", "YWViZjQ2MTUtZTdjNC00ZTNlLTllNGUtZWMwODE3ODY2NTQ2="),
                new KeyValuePair<string, string>("username", "powerbi.account@prabhujimudragalladevggktec.onmicrosoft.com"),
                new KeyValuePair<string, string>("password", "TMYfLJJ4DT59ZNr"),
            });
            var result = await httpClient.PostAsync("https://login.microsoftonline.com/common/oauth2/token", content);
            string resultContent = await result.Content.ReadAsStringAsync();

            var obj = JsonConvert.DeserializeObject<TokenModel>(resultContent);
            return obj.access_token;
        }

        public async Task<PowerBIClient> GetPowerBiClient()
        {
            var token = await this.GetAccessToken();
            var tokenCredentials = new TokenCredentials(token, "Bearer");
            return new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);
        }

        public async Task<IList<Microsoft.PowerBI.Api.Models.Report>> GetAllReports()
        {
            PowerBIClient pbiClient = await this.GetPowerBiClient();
            var reports = (await pbiClient.Reports.GetReportsAsync()).Value;
            return reports;
        }


        public async Task<Microsoft.PowerBI.Api.Models.Report> GetReport(Guid reportId)
        {
            PowerBIClient pbiClient = await this.GetPowerBiClient();
            var report = await pbiClient.Reports.GetReportAsync(reportId);
            return report;

        }

        public async Task<List<Microsoft.PowerBI.Api.Models.Report>> GetAllReportsUsingAPI()
        {
            var httpClient = new HttpClient();
            var token = await this.GetAccessToken();

            var url = $"https://api.powerbi.com/v1.0/myorg/reports";
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            var result = await httpClient.GetAsync(url);
            string resultContent = await result.Content.ReadAsStringAsync();

            var apiResult = JsonConvert.DeserializeObject<APIResult>(resultContent);
            return apiResult.Value;
        }

        public async Task<Microsoft.PowerBI.Api.Models.Dataset> UploadDataset(CreateDatasetRequest datasetSchema)
        {
            //  //Using the package
            //PowerBIClient pbiClient = await this.GetPowerBiClient();
            //var y = await pbiClient.Datasets.PostDatasetAsync(datasetSchema);
            //return y;

            var httpClient = new HttpClient();
            var token = await this.GetAccessToken();

            var url = $"https://api.powerbi.com/v1.0/myorg/datasets";
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var datasetSchema1 = JsonConvert.DeserializeObject<MyDataSet>(JsonConvert.SerializeObject(datasetSchema));
            var content = new StringContent(JsonConvert.SerializeObject(datasetSchema1), Encoding.UTF8, "application/json");

            var result = await httpClient.PostAsync(url, content);

            string resultContent = await result.Content.ReadAsStringAsync();

            var apiResult = JsonConvert.DeserializeObject<Microsoft.PowerBI.Api.Models.Dataset>(resultContent);
            return apiResult;
        }

        public async Task<string> AddRows(Guid datasetId, string tableName, PostRowsRequest rows)
        {
            //// Using the package
            //PowerBIClient pbiClient = await this.GetPowerBiClient();
            //await pbiClient.Datasets.PostRowsAsync(datasetId.ToString(), tableName, rows);
            //return "Success";

            var httpClient = new HttpClient();
            var token = await this.GetAccessToken();

            var url = $"https://api.powerbi.com/v1.0/myorg/datasets/{datasetId}/tables/{tableName}/rows";
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var content = new StringContent(JsonConvert.SerializeObject(rows), Encoding.UTF8, "application/json");

            var result = await httpClient.PostAsync(url, content);
            string resultContent = await result.Content.ReadAsStringAsync();
            return resultContent;
        }


        public async Task<string> ImportPBIX(IFormFile file, string datasetDisplayName)
        {
            try
            {
                byte[] byteArray = null;
                var filePath = Path.GetTempFileName();
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    byteArray = stream.ToArray();

                    // Throwing error
                    //PowerBIClient pbiClient = await this.GetPowerBiClient();
                    //var import = await pbiClient.Imports.PostImportWithFileAsync(stream, datasetDisplayName);
                    //return import;

                    var httpClient = new HttpClient();
                    var token = await this.GetAccessToken();

                    var url = $"https://api.powerbi.com/v1.0/myorg/imports?datasetDisplayName={datasetDisplayName}";
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                    using

                    var multipartFormDataContent = new MultipartFormDataContent();
                    var fileContent = new ByteArrayContent(byteArray);
                    multipartFormDataContent.Add(fileContent, "file", file.Name);

                    var result = await httpClient.PostAsync(url, multipartFormDataContent);
                    string resultContent = await result.Content.ReadAsStringAsync();
                    return resultContent;
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }


        private MyDataSet GetDataTable()
        {
            return new MyDataSet()
            {
                name = "SalesMarketing",
                tables = new List<MyTable>()
                {
                    new MyTable()
                    {
                        name = "Product",
                        columns = new List<MySchema>()
                        {
                            new MySchema()
                            {
                                name = "ProductID",
                                dataType = "Int64"
                            },
                            new MySchema()
                            {
                                name = "Name",
                                dataType = "string"
                            },
                            new MySchema()
                            {
                                name = "Category",
                                dataType = "string"
                            },
                            new MySchema()
                            {
                                name = "ManufacturedOn",
                                dataType = "DateTime"
                            },
                            new MySchema()
                            {
                                name = "Sales",
                                dataType = "Int64",
                                formatString = "Currency"
                            },
                        }
                    }
                }
            };
        }



    }

    public class MyDataSet
    {
        public string name { get; set; }
        public List<MyTable> tables { get; set; }
    }

    public class MyTable
    {
        public string name { get; set; }
        public List<MySchema> columns { get; set; }

    }

    public class MySchema
    {
        public string name { get; set; }
        public string dataType { get; set; }

        public string formatString { get; set; }
    }

    internal class APIResult
    {
        public List<Microsoft.PowerBI.Api.Models.Report> Value { get; set; }
    }
}
