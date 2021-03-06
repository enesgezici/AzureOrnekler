﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Samples
{
    public partial class Demo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected async void Button1_Click(object sender, EventArgs e)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("Urunler");
            await table.CreateIfNotExistsAsync();
        }

        protected async void Button2_Click(object sender, EventArgs e)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("Urunler");
            Urun urun = new Urun()
            {
                PartitionKey = "Musteri1",
                RowKey = (new Random().Next(1, 100)).ToString(),
                Adi = "Deneme",
                Aciklama = "Açıklama"
            };
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(urun);
            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
        }

        protected async void Button3_Click(object sender, EventArgs e)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("Urunler");
            TableOperation retrieveOperation = TableOperation.Retrieve<Urun>("Musteri1", "1");
            TableResult result = await table.ExecuteAsync(retrieveOperation);
            Urun urun = result.Result as Urun;
            if (urun != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(urun);
                await table.ExecuteAsync(deleteOperation);
            }
        }

        protected async void Button4_Click(object sender, EventArgs e)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("Urunler");
            TableBatchOperation batchOperation = new TableBatchOperation();

            for (int i = 0; i < 100; i++)
            {
                batchOperation.InsertOrMerge(new Urun()
                {
                    PartitionKey = "Musteri" + i.ToString(),
                    RowKey = (new Random().Next(1, 100)).ToString(),
                    Adi = "Deneme",
                    Aciklama = "Açıklama"
                });
            }
            IList<TableResult> results = await table.ExecuteBatchAsync(batchOperation);

            foreach (var res in results)
            {
                var eklenenUrun = res.Result as Urun;
            }
        }

        protected async void Button5_Click(object sender, EventArgs e)
        {
            //Creating test data.
            CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("Urunler");
            await table.CreateIfNotExistsAsync();

            for (int i = 0; i < 10000; i++)
            {
                Urun urun = new Urun()
                {
                    PartitionKey = "Musteri1",
                    RowKey = i.ToString(),
                    Adi = "Deneme",
                    Aciklama = "Açıklama"
                };
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(urun);
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
            }

            //Query data
            TableQuery<Urun> partitionScanQuery = new TableQuery<Urun>().Where
            (TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Silver"));

            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<Urun> segment =
                    await table.ExecuteQuerySegmentedAsync(partitionScanQuery, token);
                token = segment.ContinuationToken;
                foreach (Urun entity in segment)
                {
                    Response.Write(string.Format("Kim gelmiş?: {0},{1}\t{2}",
                            entity.PartitionKey,
                            entity.RowKey,
                            entity.Adi));
                }
            }
            while (token != null);
        }
    }
}