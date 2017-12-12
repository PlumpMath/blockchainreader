using System;
using System.Collections.Generic;

namespace blockchain_parser.Model
{
    public partial class Loans
    {
        public Loans()
        {
            InvestorPaymentReferences = new HashSet<InvestorPaymentReferences>();
        }

        public int LoanId { get; set; }
        public string LoanTitle { get; set; }
        public string LoanDescription { get; set; }
        public string LoanReferenceNumber { get; set; }
        public int? BorrowerId { get; set; }
        public string LoanRiskGrade { get; set; }
        public string PurposeSingleline { get; set; }
        public string Purpose { get; set; }
        public int? TokenType { get; set; }
        public DateTime? ApplyDate { get; set; }
        public int? FundingDuration { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string LocationDescription { get; set; }
        public decimal? ApplyAmount { get; set; }
        public decimal? Costpertoken { get; set; }
        public decimal? Numberoftokens { get; set; }
        public string ContractAddress { get; set; }
        public string WalletAddress { get; set; }
        public string LoanCurrencyCode { get; set; }
        public int? LoanTenure { get; set; }
        public decimal? TargetInterest { get; set; }
        public int? BidType { get; set; }
        public int? PartialSubAllowed { get; set; }
        public decimal? MinForPartialSub { get; set; }
        public int? RepaymentType { get; set; }
        public int? Status { get; set; }
        public string Comments { get; set; }
        public DateTime? LoanApprovalDate { get; set; }
        public DateTime? LoanProcessDate { get; set; }
        public decimal? FinalInterestRate { get; set; }
        public decimal? LoanSanctionedAmount { get; set; }
        public decimal? TransFees { get; set; }
        public decimal? TotalDisbursed { get; set; }
        public decimal? TotalPrincipalRepaid { get; set; }
        public decimal? TotalInterestPaid { get; set; }
        public decimal? TotalPenaltiesPaid { get; set; }
        public string LoanProductImage { get; set; }
        public string LoanVideoUrl { get; set; }
        public string LoanImageUrl { get; set; }
        public int? LoanDisplayOrder { get; set; }
        public int? FeaturedLoan { get; set; }
        public decimal? PenaltyFixedAmount { get; set; }
        public decimal? PenaltyFixedPercent { get; set; }
        public int? PenaltyTypeApplicable { get; set; }
        public int? PenaltyCompoundingPeriod { get; set; }
        public decimal? EmiAmount { get; set; }
        public string EthBaalance { get; set; }
        public decimal? PenaltyInterest { get; set; }
        public decimal? PenaltyFeeMinimum { get; set; }
        public decimal? PenaltyFeePercent { get; set; }
        public string RiskIndustry { get; set; }
        public string RiskStrength { get; set; }
        public string RiskWeakness { get; set; }
        public int PreDuration { get; set; }
        public string PreStartDate { get; set; }
        public int CrowdDuration { get; set; }
        public string CrowdStartDate { get; set; }
        public string PreEndDate { get; set; }
        public string CrowdEndDate { get; set; }
        public string RistrictedCountries { get; set; }
        public string TermsAndConditions { get; set; }
        public string TransactionDataText { get; set; }
        public string PresaleTermsAndConditions { get; set; }
        public string PresaleCustomText { get; set; }
        public string UrlTitle { get; set; }
        public Borrowers Creator { get; set; }

        public ICollection<InvestorPaymentReferences> InvestorPaymentReferences { get; set; }
    }
}
