using System;
using blockchain_parser.Blockchain.Ethereum;
using blockchain_parser.Blockchain;
using System.Collections.Generic;
using blockchain_parser.Model;
using System.Threading;
using System.Linq;
using blockchain_parser.Utils;

namespace blockchain_parser
{
    sealed class Start
    {

        /* public static void Release(object release) {
           Print(String.Format("Memory used before collection:       {0:N0} bytes", 
                GC.GetTotalMemory(false)));
            GC.Collect();
            GC.SuppressFinalize(release);
            Print(String.Format("Memory used after full collection:   {0:N0} bytes", 
                GC.GetTotalMemory(true)));
        } */

        public static string PrepareHex(string hex) {
            if(hex.StartsWith("0x"))
                hex = hex.Remove(0,2);
            return FormatHex(hex.ToUpper());
        }

        public static long? HexToLong(string hex) {
            try {
                return Convert.ToInt64(PrepareHex(hex), 16);
            } catch(Exception) {
                return null;
            }
        }

        public static ulong? HexToULong(string hex) {
            try {
                return Convert.ToUInt64(PrepareHex(hex), 16);
            } catch(Exception) {
                return null;
            }
        }

        private static string FormatHex(string hex) {
            if(hex.Length % 2 != 0)
                hex = ("0" + hex);
            if(hex.Length > 4 && hex.Length % 4 != 0)
                hex = ("00" + hex);
            return hex;
        }

        public static void Print(string message) {
            Logger.LogStatus(ConsoleColor.Gray, message);
        }

        private static void PrePrint(string message) {
            Logger.LogStatus(ConsoleColor.Yellow, message);
        }

        private static void processTransactionsTo(Dictionary<string, List<Transaction>> transactions, HashSet<string> addresses, ulong block_number) {
             var backers_handler = new BackersHandler();
             backers_handler.processBackersFromTransactions(transactions, addresses, block_number);
            // Release(backers_handler);
        }

        private static void processPastBlocks(string[] args) {
           var bids_helper = new LoanBidsHelper();
           var latest_block = bids_helper.getLatestBlock();
           
           PrePrint("latest block: " + ((latest_block.HasValue) ? latest_block.Value.ToString() : "none"));
           
           if(args.Length > 0) {
               foreach(var block_hash in args){
                 PrePrint("recover block " + block_hash);
                 var block_details = Ethereum.GetBlockDetails(block_hash);
                 var block_processor = new BlockProcessor();
                 block_processor.onTransactionsTo = processTransactionsTo;
                 block_processor.processBlockDetails(block_details, block_callback: false);
               }
           }
           else if(latest_block.HasValue) {
                var block_details = Ethereum.GetBlockDetails((ulong)latest_block.Value);
                while(block_details != null) {
                    PrePrint("recover block " + block_details.hash);
                    var block_processor = new BlockProcessor();
                    block_processor.onTransactionsTo = processTransactionsTo;
                    block_processor.processBlockDetails(block_details, block_callback: false);
                    latest_block++;
                    block_details = Ethereum.GetBlockDetails((ulong)latest_block.Value);
                }
		        PrePrint("recovering finished on block " + (latest_block-1));
            }
        }

        static void Main(string[] args)
        {
           Print("*Blockchain Parser* version 0.1.1.6");

           processPastBlocks(args);

           Ethereum.StartListenNewBlocks((new_block) => {
               Print("processing block " + new_block.hash);
               var block_processor = new BlockProcessor();
               block_processor.onTransactionsTo = processTransactionsTo;
               block_processor.processBlock(new_block);
           });

            Print("Started service for " + AppConfig.WebsocketNodeUrl + " node listening...");
            while (true)
            {
                Thread.Sleep(1000);
                if (args.Length > 0)
                    break;
            }
        
            Thread.Sleep(15000);
            Print("Service ended");
        }
    }
}
