using System;
using System.Collections.Generic;

namespace blockchain_parser.Model
{
    public partial class Investors
    {
        public int InvestorId { get; set; }
        public int UserId { get; set; }
        public decimal? AvailableBalance { get; set; }
        public string OAuthType { get; set; }
        public string OAuthLoginId { get; set; }
        public string NricNumber { get; set; }
        public int? Status { get; set; }
        public DateTime? RegisterDatetime { get; set; }
        public DateTime? ApprovalDatetime { get; set; }
        public string Nationality { get; set; }
        public int? Gender { get; set; }
        public decimal? EstimatedYearlyIncome { get; set; }
        public string IdentityCardImageFront { get; set; }
        public string IdentityCardImageBack { get; set; }
        public string AddressProofImage { get; set; }
        public string Eth { get; set; }
    }
}
