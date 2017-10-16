﻿using System;
using blockchain_parser.Blockchain.Ethereum;
using blockchain_parser.Blockchain;
using System.Collections.Generic;
using blockchain_parser.Model;

namespace blockchain_parser
{
    sealed class Start
    {

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
                return "0"+hex;
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
        }


        static void Main(string[] args)
        {
           Print("*Blockchain Parser* version 0.1.0.1");
           var bids_helper = new LoanBidsHelper();
           var latest_block = bids_helper.getLatestBlock();
           
           PrePrint("latest block: " + ((latest_block.HasValue) ? latest_block.Value.ToString() : "none"));
           
           if(latest_block.HasValue) {
                var block_details = Ethereum.GetBlockDetails((ulong)latest_block.Value);
                while(block_details != null) {
                    PrePrint("recover block " + block_details.hash);
                    var block_processor = new BlockProcessor();
                    block_processor.onTransactionsTo = processTransactionsTo;
                    block_processor.processBlockDetails(block_details);
                    latest_block++;
                    block_details = Ethereum.GetBlockDetails((ulong)latest_block.Value);
                }
		        PrePrint("recovering finished on block " + (latest_block-1));
            }

           Ethereum.StartListenNewBlocks((new_block) => {
               Print("processing block " + new_block.hash);
               var block_processor = new BlockProcessor();
               block_processor.onTransactionsTo = processTransactionsTo;
               block_processor.processBlock(new_block);
           });

            Print("Started service for " + AppConfig.WebsocketNodeUrl + " node listening...");
            Console.ReadKey();
            Ethereum.StopListenNewBlocks();
        }
    }
}
