using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Globalization;

namespace blockchain_parser.Model
{
    public class LoansHelper : DataBaseHelper
    {

        public List<Loans> FindProjects(List<string> addresses) {
           return IncludeReads(db => db.Loans, include => include.InvestorPaymentReferences, 
            condition => condition.WalletAddress != null && addresses.Any(a => 
                condition.WalletAddress.Equals(a, StringComparison.CurrentCultureIgnoreCase)));
        }
    }
}