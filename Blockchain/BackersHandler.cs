using System;
using System.Linq;
using System.Collections.Generic;
using blockchain_parser.Model;
using System.Numerics;
using System.Globalization;
using System.Threading.Tasks;
using blockchain_parser.Utils;
using System.Threading;
using blockchain_parser.Blockchain.Ethereum;

namespace blockchain_parser.Blockchain
{
    public class BackersHandler
    {
        private struct NotificationData {
            public Loans Project {get; set;}
            public Investors Backer {get; set;} 
            public Users Creator {get; set;} 
            public decimal Amount {get; set;}
        }

        private static readonly object syncLock = new object();

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
            var notifications = new List<NotificationData>();
            int backers_counter = 0;
            if(projects == null || projects.Count == 0){
                Print("no any backer transactions");
                if(all_transactions.Count > 0){
                    SaveLastBlockNumber(all_transactions[0], block_number);
                    Print("Sync block updated: " + block_number);
                }
                return;
            }

            Print("found projects: " + projects.Count);
            processProjects(projects, transactions, all_bids, no_backer_bids, 
                backer_addressess, notifications, block_number, ref backers_counter);

            all_bids.AddRange (searchBackers(no_backer_bids, backer_addressess, notifications, ref backers_counter));
            if(bids_helper.PopulateBackersFundingTransactions(all_bids))
                SendNotifications(notifications);
            Print("identified backer transactions: " + backers_counter + 
                ", " + "unidentified backer transactions: " + (all_bids.Count - backers_counter));
        }

        private void SendNotifications(List<NotificationData> notifications) {
            foreach(var notification in notifications)
                SendEmailNotification(notification.Project, notification.Backer, notification.Creator, notification.Amount);
        }

        private void Print(string message) {
            Logger.LogStatus(ConsoleColor.Cyan, message);
        }

        private void processProjects(List<Loans> projects, Dictionary<string, List<Transaction>> transactions,
            List<LoanBids> all_bids, Dictionary<string, List<LoanBids>> no_backer_bids, 
            HashSet<string> backer_addressess, List<NotificationData> notifications, ulong block_number, ref int backers_counter) {
            var found =  new Dictionary<String, Tuple<Boolean, String, LoanBids>>();

            foreach(var project in projects) {
                var project_transactions = transactions[project.WalletAddress.ToLower()];
                foreach(var project_transaction in project_transactions){
                    long? ref_id = (project_transaction.input ==  "0x0") ? null : 
                        Start.HexToLong(project_transaction.input);
                    if(ref_id.HasValue)
                        foreach(var reference in project.InvestorPaymentReferences)
                        {
                            if(reference.RefId == ref_id){
                                if(found.ContainsKey(project_transaction.hash)) {
                                    var remove = found[project_transaction.hash];
                                    if(remove.Item1)
                                        continue;
                                    backer_addressess.Remove(remove.Item2);
                                    no_backer_bids[remove.Item2].Remove(remove.Item3);
                                }
                                var bid = CreateBid(project.LoanId, ref_id, reference.InvestorId, project_transaction, block_number);
                                all_bids.Add(bid);
                                found[project_transaction.hash] = new Tuple<Boolean, String, LoanBids>(true, null, null);
                                backers_counter++;
                                Print("backer " + reference.InvestorId + " found for project: " + project.LoanId + ", transaction: " + project_transaction.hash);
                                notifications.Add(new NotificationData {
                                    Project = project,
                                    Backer = reference.Investor,
                                    Creator = project.Creator.User,
                                    Amount = bid.BidAmount.Value
                                });
                            }
                        } 
                    if(!ref_id.HasValue && !found.ContainsKey(project_transaction.hash)){
                        var from_address = project_transaction.from.ToLower();
                        if(!no_backer_bids.ContainsKey(from_address))
                            no_backer_bids.Add(from_address, new List<LoanBids>());
                        var item  = CreateBid(project.LoanId, null, null, project_transaction, block_number);
                        item.Project = project;
                        no_backer_bids[from_address].Add(item);
                        if(!backer_addressess.Contains(from_address))
                            backer_addressess.Add(from_address);
                        found[project_transaction.hash] = new Tuple<Boolean, String, LoanBids>(false, from_address, item);
                        Print("potentially unidentified transaction: " + project_transaction.hash);
                    }
                }
            }
        }

        private List<LoanBids> searchBackers(Dictionary<string, List<LoanBids>> no_backer_bids, HashSet<string> backer_addresses,
            List<NotificationData> notifications,
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
                    notifications.Add(new NotificationData {
                                    Project = bid.Project,
                                    Backer = backer,
                                    Creator = bid.Project.Creator.User,
                                    Amount = bid.BidAmount.Value
                    });
                    bid.Project = null;
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
            bid.TransId = transaction.hash;
            bid.BlockNumber = (long)block_number;
            bid.From = transaction.from;
            bid.To = transaction.to;
            bid.TransactionType = 1;
            bid.Currency = 1;
            bid.RefCode = null;
            bid.Status = 1;

            return bid;
        }

        private void SendEmailNotification(Loans project, Investors backer, Users creator, decimal amount) {

            if(project == null || backer == null || creator == null){
                Print("PARAMETERS FOR SENDING EMAIL ARE MISSSING!");
                return;
            }

            Task.Run(() => {
                try {
                var emails = new EmailNotificationsHelper();
                var backer_email = emails.GetEmailNotification(AppConfig.NotifyBackerEmailTemplate, backer.User.Language);
                var caretor_email = emails.GetEmailNotification(AppConfig.NotifyCreatorEmailTemplate, creator.Language);

                if(backer_email == null || caretor_email == null){
                    Print("EMAILS TEMPLATES ARE MISSSING!");
                    return;
                }

                backer_email.Message = backer_email.Message.Replace("[amount]", amount.ToString());
                backer_email.Message = backer_email.Message.Replace("[project_url]", project.UrlTitle);
                backer_email.Message = backer_email.Message.Replace("[project]", project.LoanTitle);
                backer_email.Message = backer_email.Message.Replace("[eth_address]", backer.Eth);
                backer_email.Message = backer_email.Message.Replace("[currency]", "ETH");

                caretor_email.Message = caretor_email.Message.Replace("[amount]", amount.ToString());
                caretor_email.Message = caretor_email.Message.Replace("[project_url]", project.UrlTitle);
                caretor_email.Message = caretor_email.Message.Replace("[project]", project.LoanTitle);
                caretor_email.Message = caretor_email.Message.Replace("[backer]", backer.User.Username);
                caretor_email.Message = caretor_email.Message.Replace("[currency]", "ETH");

                var email_to_backer = new Email(backer_email.Subject, backer_email.Message, backer.User.Email);
                var email_to_creator = new Email(caretor_email.Subject, caretor_email.Message, creator.Email);
                
                int sending_tries = 10;
                while(!email_to_backer.Send() && sending_tries > 0){
                    sending_tries--;
                    Print("Failed to send email, resending... " + sending_tries);
                }
                if(sending_tries == 0)
                    Print("UNABLE TO SENT EMAIL  " + backer_email.Subject + " TO " + backer.User.Email);
                else
                    Print("Notification email " + backer_email.Subject + " sent to " + backer.User.Email);

                sending_tries = 10;
                 while(!email_to_creator.Send() && sending_tries > 0) {
                     sending_tries--;
                      Print("Failed to send email, resending... " + sending_tries);
                 }
                 if(sending_tries == 0)
                    Print("UNABLE TO SENT EMAIL  " + caretor_email.Subject + " TO " + creator.Email);
                 else
                    Print("Notification email " + caretor_email.Subject + " sent to " + creator.Email);
                } catch(Exception ex) {
                    Logger.LogStatus(ConsoleColor.Red, ex.ToString());
                }
            });
        }
    }
    
}