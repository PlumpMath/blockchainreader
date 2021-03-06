﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace blockchain_parser.Model
{
    public partial class InvestorPaymentReferences
    {
        public long RefId { get; set; }
        public int InvestorId { get; set; }
        public int BorrowerId { get; set; }
        public int LoanId { get; set; }
        public int RewardId { get; set; }

        public Loans Loan { get; set; }
        public Investors Investor { get; set; }
    }
}
