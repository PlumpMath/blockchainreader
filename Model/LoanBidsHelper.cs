using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace blockchain_parser.Model
{
    public class LoanBidsHelper : DataBaseHelper
    {

        public long? getLatestBlock() {
            var bid = Read(db => db.LoanBids, sort => sort.BlockNumber);
            if(bid == null)
                return null;
            return bid.BlockNumber;
        }
        public bool PopulateBackersFundingTransactions(List<LoanBids> funding_transactions) {
            var result = Create(db => db.LoanBids, funding_transactions);
            if(result.HasValue && result.Value)
                return true;
            return false;
        }

        public void CreateOrUpdateLastBlock(LoanBids block_transactions) {
            var b = block_transactions;
          
            Update(db => db.LoanBids, condition => condition.BidStatus == b.BidStatus, update => {
                    if(update == null)
                         Create(db => db.LoanBids, b);
                    else
                        update.BlockNumber = b.BlockNumber;
            });
        }
    }
}