using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using blockchain_parser.Model;

namespace blockchain_parser.Blockchain.Ethereum
{
    public class BlockProcessor
    {
        public BlockProcessor() {
            onTransactionsTo = (transactions, addresses, block_number) => {

            };
        }
        
        public Action<Dictionary<string, List<Transaction>>, HashSet<string>, ulong> onTransactionsTo {get; set;}

        public void processBlock(dynamic block, bool with_value = true) {
            if(block == null || block.hash == null || !isTypeValues(JTokenType.String, block.hash))
                return;
                
            dynamic block_response = Ethereum.GetBlockDetails((string)block.hash);
            processBlockDetails(block_response);
        }

        public void processBlockDetails(dynamic block_response, bool with_value = true) {
            if(block_response == null || block_response.transactions == null ||
                !(block_response.transactions is JArray)){
                    Print("unknown block");
                    return;
                }
            var transactions = (JArray)block_response.transactions;
            if(transactions.Count > 0)
                processTransactions(transactions, with_value, (string)block_response.hash);
            else
                Print("no transactions for block " + block_response.hash);
        }

        protected bool isTypeValues(JTokenType type, params JToken[] values) {
            int count = 0;
            foreach(JToken value in values) {
                if(value.Type != type){
                    return false;
                }
                count++;
            }
            return true;
        }

        private void Print(string message) {
            Logger.LogStatus(ConsoleColor.Gray, message);
        }

        protected void processTransactions(JArray transactions, bool with_value, string block_hash){

            Dictionary<string, List<Transaction>> processing_transactions_to = new Dictionary<string, List<Transaction>>();
            HashSet<string> addresses = new HashSet<string>();
            ulong block_number = 0;
            int count = 0;
            int total_count = 0;

            foreach(JToken transaction in transactions){
                if(transaction.Type != JTokenType.Object ||
                 !isTypeValues(JTokenType.String, 
                 transaction["blockNumber"], 
                 transaction["hash"],
                 transaction["from"],
                 transaction["input"],
                 transaction["to"],
                 transaction["value"]) ||
                 (with_value && (string)transaction["value"] == "0x0") ){
                    Print("transaction " + transaction["hash"] + " of block " + block_hash + " skipped");
                    continue;
                 }

                var bn = Start.HexToULong((string)transaction["blockNumber"]);
                block_number = (bn.HasValue) ? bn.Value : 0;
                if(count > AppConfig.MaxTransactionPerSqlQuery)
                {
                   onTransactionsTo(processing_transactions_to, addresses, block_number);
                   processing_transactions_to.Clear();
                   addresses.Clear();
                   count = 0;
                }

                var new_transaction = new Transaction {
                    hash = (string)transaction["hash"],
                    from = (string)transaction["from"],
                    to = (string)transaction["to"],
                    input = (string)transaction["input"],
                    value = (string)transaction["value"]
                };

                if(!addresses.Contains(new_transaction.to.ToLower()))
                    addresses.Add(new_transaction.to.ToLower());

                if(!processing_transactions_to.ContainsKey(new_transaction.to.ToLower()))
                    processing_transactions_to.Add(new_transaction.to.ToLower(), new List<Transaction>());

                processing_transactions_to[new_transaction.to.ToLower()].Add(new_transaction);

                count++;
                total_count++;
            }
            if(count > 0)
                onTransactionsTo(processing_transactions_to, addresses, block_number);
            Print("total transactions " + transactions.Count + " in block " + block_number);
            Print("total processed transactions " + total_count + " in block " + block_number);
        } 

    }
}