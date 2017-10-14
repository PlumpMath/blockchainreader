using System;
using System.Collections.Generic;

namespace blockchain_parser.Model
{
    public partial class LoanBids
    {
        public int BidId { get; set; }
        public int LoanId { get; set; }
        public int? InvestorId { get; set; }
        public DateTime BidDatetime { get; set; }
        public decimal? BidAmount { get; set; }
        public decimal? BidInterestRate { get; set; }
        public int? BidStatus { get; set; }
        public decimal? AcceptedAmount { get; set; }
        public DateTime? ProcessDate { get; set; }
        public decimal? Emi { get; set; }
        public string TransId { get; set; }
        public long? RefId { get; set; }
        public string Remarks { get; set; }
        public long BlockNumber { get; set; }
    }
}
