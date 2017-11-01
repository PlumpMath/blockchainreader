using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace blockchain_parser.Model
{
    public class LoansHelper : DataBaseHelper
    {

        public List<Loans> FindProjects(List<string> addresses) {
           return IncludeReads(db => db.Loans, include => include.InvestorPaymentReferences, 
            condition => addresses.Any(a => condition.WalletAddress == a));
        }
    }
}