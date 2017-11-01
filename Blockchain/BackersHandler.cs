using System;
using System.Linq;
using System.Collections.Generic;
using blockchain_parser.Model;
using System.Numerics;
using System.Globalization;

namespace blockchain_parser.Blockchain
{
    public class BackersHandler
    {
        public BackersHandler(){}

        public void processBackersFromTransactions(Dictionary<string, List<Transaction>> transactions, HashSet<string> addresses, ulong block_number) {
            var all_transactions = transactions.Values.ToList().SelectMany(x => x).ToList();
            Print("transactions: " + all_transactions.ToList().Count + ", addresses: " + addresses.Count + ", block number: " + block_number);
            var projects_helper = new LoansHelper();
            var bids_helper = new LoanBidsHelper();
            var all_bids = new List<LoanBids>();
            var no_backer_bids = new Dictionary<string, List<LoanBids>>();
            var backer_addressess = new HashSet<string>();
            var projects = projects_helper.FindProjects(addresses.ToList());
            int backers_counter = 0;
            if(projects == null || projects.Count == 0){
                Print("no any backer transactions");
                if(all_transactions.Count > 0)
                    SaveLastBlockNumber(all_transactions[0], block_number);
                return;
            }

            Print("found projects: " + projects.Count);

            foreach(var project in projects) {
                var project_transactions = transactions[project.WalletAddress.ToLower()];
                foreach(var project_transaction in project_transactions){
                    long? ref_id = (project_transaction.input ==  "0x0") ? null : 
                        Start.HexToLong(project_transaction.input);
                    bool found = false;
                    if(ref_id.HasValue)
                        foreach(var reference in project.InvestorPaymentReferences)
                        {
                            if(reference.RefId == ref_id){
                                all_bids.Add(CreateBid(project.LoanId, ref_id, reference.InvestorId, project_transaction, block_number));
                                found = true;
                                backers_counter++;
                                Print("backer " + reference.InvestorId + " found for project: " + project.LoanId + ", transaction: " + project_transaction.hash);
                            }
                        } 
                    if(!ref_id.HasValue || !found){
                        if(!no_backer_bids.ContainsKey(project_transaction.from.ToLower()))
                            no_backer_bids.Add(project_transaction.from.ToLower(), new List<LoanBids>());
                        no_backer_bids[project_transaction.from.ToLower()].Add(CreateBid(project.LoanId, null, null, project_transaction, block_number));
                        if(!backer_addressess.Contains(project_transaction.from.ToLower()))
                            backer_addressess.Add(project_transaction.from.ToLower());
                        Print("potentially unidentified transaction: " + project_transaction.hash);
                    }
                }
            }

            all_bids.AddRange (searchBackers(no_backer_bids, backer_addressess, ref backers_counter));
            bids_helper.PopulateBackersFundingTransactions(all_bids);
            Print("identified backer transactions: " + backers_counter + 
                ", " + "unidentified backer transactions: " + (all_bids.Count - backers_counter));
        }

        private void Print(string message) {
            Logger.LogStatus(ConsoleColor.Cyan, message);
        }

        private List<LoanBids> searchBackers(Dictionary<string, List<LoanBids>> no_backer_bids, HashSet<string> backer_addresses,
            ref int backers_counter) {
            var backers_helper  = new InvestorsHelper();
            var backers = backers_helper.FindBackers(backer_addresses.ToList());
            if(backers == null || backers.Count == 0)
                return no_backer_bids.Values.ToList().SelectMany(x => x).ToList();
            foreach(var backer in backers) {
                foreach(var bid in no_backer_bids[backer.Eth.ToLower()]){
                    bid.InvestorId = backer.InvestorId;
                    bid.BidStatus = 1;
                    backers_counter++;
                    Print("backer " + bid.InvestorId + " found for project: " + bid.LoanId + ", transaction: " + bid.TransId);
                }
            }
            return no_backer_bids.Values.ToList().SelectMany(x => x).ToList();;
        }

        private void SaveLastBlockNumber(Transaction transaction, ulong block_number){
            var bids_helper = new LoanBidsHelper();
            var bid = CreateBid(0, null, null, transaction, block_number, -1);
            bids_helper.CreateOrUpdateLastBlock(bid);
        }

        private LoanBids CreateBid(int loan_id, long? ref_id, int? investor_id, Transaction transaction, ulong block_number,
            int? status = null) {
            var bid = new LoanBids();
            var hex = Start.PrepareHex(transaction.value);
            var amount_int = BigInteger.Parse("00"+hex, NumberStyles.HexNumber);
            bool contracted = false;
            if(amount_int.ToString().Length > 18){
                amount_int = BigInteger.Divide(amount_int, BigInteger.Parse("10000000000"));
                contracted = true;
            }
            decimal amount = (hex.Length > 32) ? 0 : (decimal)amount_int;
            if(amount > 0)
                amount = amount / ((contracted) ? 100000000m : 1000000000000000000m);
            else 
                amount = 0;

            bid.AcceptedAmount = amount;
            bid.BidAmount = amount;
            bid.BidDatetime = DateTime.UtcNow;
            if(status.HasValue)
                bid.BidStatus = status.Value;
            else
                bid.BidStatus = (investor_id.HasValue) ? 1 : 0;
            bid.LoanId = loan_id;
            bid.InvestorId = investor_id;
            bid.ProcessDate = DateTime.UtcNow;
            bid.RefId = ref_id;
            bid.TransId = transaction.hash;
            bid.BlockNumber = (long)block_number;
            bid.From = transaction.from;
            bid.To = transaction.to;

            return bid;
        }
    }
}