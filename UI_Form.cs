using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UIAzureTracker
{
    public partial class Form1 : Form
    {
        private int secondsUntilNextRun = 0;
        private List<Dictionary<string, object>> lastTweets;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {

            btnProcess.Text = btnProcess.Text.Equals("Cancel") ? "Start" : "Cancel";

            if (bgt.IsBusy)
                bgt.CancelAsync();
            else
                bgt.RunWorkerAsync();

        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            
            var azure = new AzureActivity(txtAccountName.Text, txtAccountKey.Text);
            var lastTweet = azure.GetLastTweetID;

            var tsv = new TwitterSvc();
            var listOfTweets = tsv.GetListOfTweets("BTC", lastTweet, 50);
            lastTweets = listOfTweets;

            foreach (var tweet in listOfTweets.OrderBy(x => x["TweetId"]))
            {
                azure.InsertOrReplaceTweetEntity(tweet, "BTC");
                lastTweet = tweet["TweetId"].ToString();
            }

            azure.UpdateQuickWatcherLastTweetID(lastTweet);

            lastTweets = listOfTweets;
            bgw.ReportProgress(100);
            System.Threading.Thread.Sleep(1000);
        }

        private void bgt_DoWork(object sender, DoWorkEventArgs e)
        {
            for(int i = 0; i >= 0; i--)
            {
                if (bgt.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                if (i == 0)
                {
                    
                    i = 59;
                    if (!bgw.IsBusy)
                    {
                        
                        bgw.RunWorkerAsync();
                    }
                }

                bgt.ReportProgress(i);
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void bgt_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            secondsUntilNextRun = 0;
            lblTimer.Text = "1:00";
        }

        private void bgt_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblTimer.Text = e.ProgressPercentage.ToString("0:00");
            if(!bgw.IsBusy && lastTweets != null && lastTweets.Any())
                UpdateLastTweets();
        }

        private void UpdateLastTweets()
        {
            if (lastTweets == null || !lastTweets.Any()) return;

            if(txtTweets.TextLength > 0) txtTweets.Clear();

            foreach (var tweet in lastTweets)
            {
                txtTweets.Text += tweet["TweetText"].ToString() + Environment.NewLine;
            }

            lastTweets = null;
        }
    }
}
