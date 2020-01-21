using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity.AzureStorageAccount
{
    static class CloudStorageAccountExtensions
    {
        public static async Task<TableResult> DeleteAsync<T>(this CloudTableClient tableClient, string tableName, T item, string partitionKey, string rowKey)
        {
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            var tableEntity = new TableEntityAdapter<T>(item, partitionKey, rowKey);
            var tableOperation = TableOperation.Delete(tableEntity);
            return await table.ExecuteAsync(tableOperation);
        }

        public static async Task<IList<TableResult>> DeleteAsync<T>(this CloudTableClient tableClient, string tableName, IEnumerable<T> items, string partitionKey, Func<T, string> rowKey)
        {
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            var batch = new TableBatchOperation();
            var tableOperations = items.Select(item => new TableEntityAdapter<T>(item, partitionKey, rowKey(item)))
                .Select(TableOperation.Delete);

            foreach (var tableOperation in tableOperations)
            {
                batch.Add(tableOperation);
            }

            return await table.ExecuteBatchAsync(batch);
        }

        public static async Task<T> FindAsync<T>(this CloudTableClient tableClient, string tableName, string partitionKey, string rowKey)
        {
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            var tableOperation = TableOperation.Retrieve<TableEntityAdapter<T>>(partitionKey, rowKey);
            var result = await table.ExecuteAsync(tableOperation);
            var adapterResult = ((TableEntityAdapter<T>)result?.Result);
            if (adapterResult == null) { return default(T); }
            else { return adapterResult.OriginalEntity; }
        }

        public static async Task<IList<T>> FindAllAsync<T>(this CloudTableClient tableClient, string tableName, string partitionKey)
        {
            var list = new List<T>();

            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            var query = new TableQuery<TableEntityAdapter<T>>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            ;

            var token = default(TableContinuationToken);
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, token);
                list.AddRange(segment.Select(x => x.OriginalEntity));
                token = segment.ContinuationToken;
            } while (token != null);

            return list;
        }

        public static async Task<TableResult> InsertAsync<T>(this CloudTableClient tableClient, string tableName, T item, string partitionKey, string rowKey)
        {
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            var tableEntity = new TableEntityAdapter<T>(item, partitionKey, rowKey);
            var tableOperation = TableOperation.Insert(tableEntity);
            return await table.ExecuteAsync(tableOperation);
        }

        public static Task<IList<TableResult>> InsertAsync<T>(this CloudTableClient tableClient, string tableName, IEnumerable<T> items, string partitionKey, Func<T, string> rowKey) =>
            tableClient.InsertAsync(tableName, items, _ => partitionKey, rowKey);

        public static async Task<IList<TableResult>> InsertAsync<T>(this CloudTableClient tableClient, string tableName, IEnumerable<T> items, Func<T, string> partitionKey, Func<T, string> rowKey)
        {
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            var batch = new TableBatchOperation();
            var tableOperations = items.Select(item => new TableEntityAdapter<T>(item, partitionKey(item), rowKey(item)))
                .Select(TableOperation.Insert);

            foreach (var tableOperation in tableOperations)
            {
                batch.Add(tableOperation);
            }

            return await table.ExecuteBatchAsync(batch);
        }

        public static async Task<TableResult> InsertOrReplaceAsync<T>(this CloudTableClient tableClient, string tableName, T item, string partitionKey, string rowKey)
        {
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            var tableEntity = new TableEntityAdapter<T>(item, partitionKey, rowKey);
            var tableOperation = TableOperation.InsertOrReplace(tableEntity);
            return await table.ExecuteAsync(tableOperation);
        }
    }
}
