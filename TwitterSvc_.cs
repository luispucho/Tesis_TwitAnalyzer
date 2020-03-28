using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAzureTracker
{
    class TwitterSvc
    {
        private const string ConnectionURL = "https://prod-06.southcentralus.logic.azure.com:443/workflows/[AzureCodeConnection]";
        private const string privatePhrase = "ThisIsThePhraseThatNeedsToBeIncluded";

        

        public List<Dictionary<string, object>> GetListOfTweets(string topic, string lastTweet, int quantity)
        {

            var client = new RestClient(ConnectionURL)
            {
                Timeout = -1
            };

            var callParameters = new Dictionary<string, object>()
            {
                {"topic",topic},
                {"lastTweet" , lastTweet },
                {"quantity", quantity },
                {"phrase", privatePhrase }
            };
            var callParametersStr = JsonConvert.SerializeObject(callParameters);

            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            _ = request.AddParameter("application/json",
                callParametersStr, 
                ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);            
            return tweetDecerealize(response.Content);
            
        }

        private List<Dictionary<string , object>> tweetDecerealize (string json)
        {
            
            var values = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);

            return values;
        }

    }
}
