using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UIAzureTracker
{


    public class QuickWatcherTwitterEntity : TableEntity
    {
        public QuickWatcherTwitterEntity()
        {
        }

        public QuickWatcherTwitterEntity(string tweetID)
        {
            LastTweetID = tweetID;
        }


        public string LastTweetID { get; set; }
    }

    public class RegisterTweetsEntity : TableEntity
    {
        public RegisterTweetsEntity()
        {
        }
               
        public DateTime LastUpdateTime { get; set; }
        public string Message { get; set; }
        public int RetweetCount { get; set; }
        public string SearchWord { get; set; }
        public bool SentimentProcessed { get; set; }
        public double SentimentScore { get; set; }
        public string TweetID { get; set; }
        public string TweetLanguage { get; set; }
        public string TweetUser { get; set; }
    }


    public class AzureActivity
    {
        private string accountName;
        private string accountKey;

        public AzureActivity(string accountName, string accountKey)
        {
            this.accountKey = accountKey;
            this.accountName = accountName;
        }

        private string AzureEndpoint => $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey}";
        private CloudStorageAccount StorageAccount => CloudStorageAccount.Parse(AzureEndpoint);

        public string GetLastTweetID => GetQuickWatcherEntity("Tweets", "1")?.LastTweetID ?? "0";

        private QuickWatcherTwitterEntity GetQuickWatcherEntity(string PartitionKey, string RowKey)
        {
            var storageAccount = StorageAccount;
            var tableClient = storageAccount.CreateCloudTableClient();
            var tableObj = tableClient.GetTableReference("QuickWatcher");            
            
            var tResult = tableObj.Execute(TableOperation.Retrieve<QuickWatcherTwitterEntity>(PartitionKey, RowKey, new List<string>{ "LastTweetID" }));
            
            if(tResult?.Result != null && tResult.HttpStatusCode == 200 &&  tResult.Result is QuickWatcherTwitterEntity)
            {
                return tResult.Result as QuickWatcherTwitterEntity;                
            }

            return null;
        }

        public void getOriginalTweet(Dictionary<string, object> tweet, out string TweetText, out string TweetId, out string TweetedBy)
        {
            if (tweet.ContainsKey("OriginalTweet") && tweet["OriginalTweet"] != null)
            {
                var originalTweet = JsonConvert.DeserializeObject<Dictionary<string, object>>(tweet["OriginalTweet"].ToString());
                TweetId = originalTweet["TweetId"].ToString();
                TweetText = originalTweet["TweetText"].ToString();
                TweetedBy = originalTweet["TweetedBy"].ToString();
                return;
            }

            TweetId = tweet["TweetId"].ToString();
            TweetText = tweet["TweetText"].ToString();
            TweetedBy = tweet["TweetedBy"].ToString();
        }
        public void InsertOrReplaceTweetEntity(Dictionary<string, object> tweet, string topic)
        {
            var storageAccount = StorageAccount;
            var tableClient = storageAccount.CreateCloudTableClient();
            var tableObj = tableClient.GetTableReference("Tweets");

            var TweetText = "";
            var TweetId = "";
            var TweetedBy = "";
            getOriginalTweet(tweet, out TweetText, out TweetId, out TweetedBy);

            var tEntity = new RegisterTweetsEntity()
            {
                PartitionKey = "RegisterTweets",
                RowKey = TweetId,
                LastUpdateTime = DateTime.Now,
                Message = TweetText,
                RetweetCount = Convert.ToInt32(tweet["RetweetCount"].ToString()),
                SearchWord = topic,
                SentimentProcessed = false,
                SentimentScore = .5,
                TweetID = TweetId,
                TweetLanguage = tweet["TweetLanguageCode"].ToString(),
                TweetUser = tweet["TweetedBy"].ToString()
            };

            tableObj.Execute(TableOperation.InsertOrReplace(tEntity));
        }

        public void UpdateQuickWatcherLastTweetID(string LastTweetID)
        {

            var storageAccount = StorageAccount;
            var tableClient = storageAccount.CreateCloudTableClient();
            var tableObj = tableClient.GetTableReference("QuickWatcher");
            var wEntity = GetQuickWatcherEntity("Tweets", "1");
            wEntity.LastTweetID = LastTweetID;

            tableObj.Execute(TableOperation.InsertOrReplace(wEntity));
        }

        private IEnumerable<TableBatchOperation> BatchOperation(DataTable dt, int limit = 100)
        {
            var batchOperationContainer = new List<TableBatchOperation>();
            batchOperationContainer.Add(new TableBatchOperation());

            var idxContainer = 0;

            foreach(var row in dt.Rows)
            {
                 
            }
            return null;
        }

    }
}
