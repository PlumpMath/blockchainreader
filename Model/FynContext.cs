using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace blockchain_parser.Model
{
    public partial class FynContext : DbContext
    {
        public virtual DbSet<InvestorPaymentReferences> InvestorPaymentReferences { get; set; }
        public virtual DbSet<Investors> Investors { get; set; }
        public virtual DbSet<LoanBids> LoanBids { get; set; }
        public virtual DbSet<Loans> Loans { get; set; }
        public virtual DbSet<EmailNotifications> EmailNotifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(AppConfig.getConnectionString("FynMySqlDatabase"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<EmailNotifications>(entity =>
            {
                 entity.HasKey(e => e.SlugName);
                 entity.HasAlternateKey(e => e.Language);
                 entity.ToTable("email_notifications");
                 
                entity.Property(e => e.SlugName)
                    .HasColumnName("slug_name");
                
                entity.Property(e => e.Subject)
                    .HasColumnName("email_subject").IsRequired();
                
                entity.Property(e => e.Message)
                    .HasColumnName("email_content");

                entity.Property(e => e.Language)
                    .HasColumnName("language");
                    
            });

            modelBuilder.Entity<Borrowers>(entity =>
            {
                 entity.HasKey(e => e.BorrowerId);
                 entity.ToTable("borrowers");
                 entity.HasIndex(e => e.UserId).HasName("fk_borrowers_users_idx");
                 
                 entity.Property(e => e.BorrowerId)
                    .HasColumnName("borrower_id")
                    .HasColumnType("int(11)");
                
                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)").IsRequired();

                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            });

            modelBuilder.Entity<Users>(entity =>
            {
                 entity.HasKey(e => e.UserId);
                 entity.ToTable("users");

                 entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.UserType)
                    .HasColumnName("usertype")
                    .HasColumnType("int(11)").IsRequired();
                
                entity.Property(e => e.Username)
                    .HasColumnName("username").IsRequired();

                entity.Property(e => e.Email)
                    .HasColumnName("email").IsRequired();

                entity.Property(e => e.FirstName)
                    .HasColumnName("firstname"); 

                entity.Property(e => e.LastName)
                    .HasColumnName("lastname");         

                entity.Property(e => e.Language)
                    .HasColumnName("language")
                    .HasColumnType("int(11)").IsRequired();      
            });

            modelBuilder.Entity<InvestorPaymentReferences>(entity =>
            {
                entity.HasKey(e => e.RefId);

                entity.ToTable("investor_payment_references");

                entity.HasIndex(e => e.InvestorId)
                    .HasName("fk_investor_payment_references_1_idx");

                entity.HasIndex(e => e.LoanId)
                    .HasName("fk_investor_payment_references_3_idx");

                entity.HasIndex(e => e.RewardId)
                    .HasName("fk_investor_payment_references_4_idx");

                entity.HasIndex(e => new { e.BorrowerId, e.InvestorId, e.LoanId, e.RewardId })
                    .HasName("index_refernmces")
                    .IsUnique();

                entity.Property(e => e.RefId)
                    .HasColumnName("ref_id")
                    .HasColumnType("bigint(20)")
                    .ValueGeneratedNever();

                entity.Property(e => e.BorrowerId)
                    .HasColumnName("borrower_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.InvestorId)
                    .HasColumnName("investor_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LoanId)
                    .HasColumnName("loan_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RewardId)
                    .HasColumnName("reward_id")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("0");

                entity.HasOne(d => d.Loan)
                    .WithMany(p => p.InvestorPaymentReferences)
                    .HasForeignKey(d => d.LoanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_investor_payment_references_3");
                
                entity.HasOne(e => e.Investor).WithMany().HasForeignKey(e => e.InvestorId);
            });

            modelBuilder.Entity<Investors>(entity =>
            {
                entity.HasKey(e => new { e.InvestorId });

                entity.ToTable("investors");

                entity.HasIndex(e => e.Eth)
                    .HasName("unique_eth")
                    .IsUnique();

                entity.HasIndex(e => e.UserId)
                    .HasName("unique_user")
                    .IsUnique();

                entity.Property(e => e.InvestorId)
                    .HasColumnName("investor_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AddressProofImage)
                    .HasColumnName("address_proof_image")
                    .HasMaxLength(255);

                entity.Property(e => e.ApprovalDatetime)
                    .HasColumnName("approval_datetime")
                    .HasColumnType("datetime");

                entity.Property(e => e.AvailableBalance)
                    .HasColumnName("available_balance")
                    .HasColumnType("decimal(20,8)")
                    .HasDefaultValueSql("0.00000000");

                entity.Property(e => e.EstimatedYearlyIncome)
                    .HasColumnName("estimated_yearly_income")
                    .HasColumnType("decimal(20,8)")
                    .HasDefaultValueSql("0.00000000");

                entity.Property(e => e.Eth)
                    .HasColumnName("eth")
                    .HasMaxLength(255);

                entity.Property(e => e.Gender)
                    .HasColumnName("gender")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IdentityCardImageBack)
                    .HasColumnName("identity_card_image_back")
                    .HasMaxLength(255);

                entity.Property(e => e.IdentityCardImageFront)
                    .HasColumnName("identity_card_image_front")
                    .HasMaxLength(255);

                entity.Property(e => e.Nationality)
                    .HasColumnName("nationality")
                    .HasMaxLength(60);

                entity.Property(e => e.NricNumber)
                    .HasColumnName("nric_number")
                    .HasMaxLength(60);

                entity.Property(e => e.OAuthLoginId)
                    .HasColumnName("oAuth_login_id")
                    .HasMaxLength(255);

                entity.Property(e => e.OAuthType)
                    .HasColumnName("oAuthType")
                    .HasMaxLength(45);

                entity.Property(e => e.RegisterDatetime)
                    .HasColumnName("register_datetime")
                    .HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("int(11)");

                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            });

            modelBuilder.Entity<LoanBids>(entity =>
            {
                entity.HasKey(e => e.BidId);

                entity.ToTable("loan_bids");

                entity.HasIndex(e => e.LoanId)
                    .HasName("fk_loan_bids_loans1_idx");

                entity.HasIndex(e => e.TransId)
                    .HasName("trans_id")
                    .IsUnique();

                entity.Property(e => e.BidId)
                    .HasColumnName("bid_id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.AcceptedAmount)
                    .HasColumnName("accepted_amount")
                    .HasColumnType("decimal(24,8)");

                entity.Property(e => e.BidAmount)
                    .HasColumnName("bid_amount")
                    .HasColumnType("decimal(24,8)");

                entity.Property(e => e.BidDatetime)
                    .HasColumnName("bid_datetime")
                    .HasColumnType("datetime");

                entity.Property(e => e.BidInterestRate)
                    .HasColumnName("bid_interest_rate")
                    .HasColumnType("decimal(24,8)");

                entity.Property(e => e.BidStatus)
                    .HasColumnName("bid_status")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Emi)
                    .HasColumnName("emi")
                    .HasColumnType("decimal(24,8)");

                entity.Property(e => e.InvestorId)
                    .HasColumnName("investor_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LoanId)
                    .HasColumnName("loan_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ProcessDate)
                    .HasColumnName("process_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Remarks).HasColumnName("remarks");

                entity.Property(e => e.TransId)
                    .IsRequired()
                    .HasColumnName("trans_id")
                    .HasMaxLength(128);

                entity.Property(e => e.BlockNumber)
                    .IsRequired()
                    .HasColumnName("block_number")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.From)
                    .IsRequired()
                    .HasColumnName("from");

                entity.Property(e => e.To)
                    .IsRequired()
                    .HasColumnName("to");

                entity.Property(e => e.TransactionType)
                    .IsRequired()
                    .HasColumnType("smallint(6)")
                    .HasColumnName("tx_type");
                
                 entity.Property(e => e.Currency)
                    .IsRequired()
                    .HasColumnType("smallint(6)")
                    .HasColumnName("currency");

                 entity.Property(e => e.RefCode)
                    .IsRequired()
                    .HasColumnName("ref_code");
                
                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnType("smallint(6)")
                    .HasColumnName("status");
                
                entity.Ignore(e => e.Project);
            });

            modelBuilder.Entity<Loans>(entity =>
            {
                entity.HasKey(e => e.LoanId);

                entity.ToTable("loans");

                entity.HasIndex(e => e.BorrowerId)
                    .HasName("fk_loans_borrowers1_idx");

                entity.HasIndex(e => e.ContractAddress)
                    .HasName("unique_contract_address")
                    .IsUnique();

                entity.HasIndex(e => e.LoanReferenceNumber)
                    .HasName("unique_ref_id")
                    .IsUnique();

                entity.HasIndex(e => e.WalletAddress)
                    .HasName("unique_wallet_address")
                    .IsUnique();

                entity.Property(e => e.LoanId)
                    .HasColumnName("loan_id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedNever();

                entity.Property(e => e.ApplyAmount)
                    .HasColumnName("apply_amount")
                    .HasColumnType("decimal(20,8)");

                entity.Property(e => e.ApplyDate)
                    .HasColumnName("apply_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.BidType)
                    .HasColumnName("bid_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BorrowerId)
                    .HasColumnName("borrower_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Comments).HasColumnName("comments");

                entity.Property(e => e.ContractAddress)
                    .HasColumnName("contract_address")
                    .HasMaxLength(255);

                entity.Property(e => e.Costpertoken)
                    .HasColumnName("costpertoken")
                    .HasColumnType("decimal(20,8)");

                entity.Property(e => e.CrowdDuration)
                    .HasColumnName("crowd_duration")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CrowdEndDate)
                    .IsRequired()
                    .HasColumnName("crowd_end_date")
                    .HasMaxLength(255);

                entity.Property(e => e.CrowdStartDate)
                    .IsRequired()
                    .HasColumnName("crowd_start_date")
                    .HasMaxLength(255);

                entity.Property(e => e.EmiAmount)
                    .HasColumnName("emi_amount")
                    .HasColumnType("decimal(20,8)");

                entity.Property(e => e.EthBaalance)
                    .HasColumnName("eth_baalance")
                    .HasMaxLength(25);

                entity.Property(e => e.FeaturedLoan)
                    .HasColumnName("featured_loan")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FinalInterestRate)
                    .HasColumnName("final_interest_rate")
                    .HasColumnType("decimal(6,2)");

                entity.Property(e => e.FundingDuration)
                    .HasColumnName("funding_duration")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Latitude)
                    .HasColumnName("latitude")
                    .HasMaxLength(25);

                entity.Property(e => e.LoanApprovalDate)
                    .HasColumnName("loan_approval_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.LoanCurrencyCode)
                    .HasColumnName("loan_currency_code")
                    .HasMaxLength(4);

                entity.Property(e => e.LoanDescription).HasColumnName("loan_description");

                entity.Property(e => e.LoanDisplayOrder)
                    .HasColumnName("loan_display_order")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LoanImageUrl)
                    .HasColumnName("loan_image_url")
                    .HasMaxLength(255);

                entity.Property(e => e.LoanProcessDate)
                    .HasColumnName("loan_process_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.LoanProductImage)
                    .HasColumnName("loan_product_image")
                    .HasMaxLength(255);

                entity.Property(e => e.LoanReferenceNumber)
                    .IsRequired()
                    .HasColumnName("loan_reference_number")
                    .HasMaxLength(45);

                entity.Property(e => e.LoanRiskGrade)
                    .HasColumnName("loan_risk_grade")
                    .HasMaxLength(4);

                entity.Property(e => e.LoanSanctionedAmount)
                    .HasColumnName("loan_sanctioned_amount")
                    .HasColumnType("decimal(20,8)");

                entity.Property(e => e.LoanTenure)
                    .HasColumnName("loan_tenure")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LoanTitle)
                    .IsRequired()
                    .HasColumnName("loan_title")
                    .HasMaxLength(255);

                entity.Property(e => e.LoanVideoUrl)
                    .HasColumnName("loan_video_url")
                    .HasMaxLength(255);

                entity.Property(e => e.LocationDescription)
                    .HasColumnName("location_description")
                    .HasMaxLength(255);

                entity.Property(e => e.Longitude)
                    .HasColumnName("longitude")
                    .HasMaxLength(25);

                entity.Property(e => e.MinForPartialSub)
                    .HasColumnName("min_for_partial_sub")
                    .HasColumnType("decimal(20,8)");

                entity.Property(e => e.Numberoftokens)
                    .HasColumnName("numberoftokens")
                    .HasColumnType("decimal(20,8)");

                entity.Property(e => e.PartialSubAllowed)
                    .HasColumnName("partial_sub_allowed")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PenaltyCompoundingPeriod)
                    .HasColumnName("penalty_compounding_period")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PenaltyFeeMinimum)
                    .HasColumnName("penalty_fee_minimum")
                    .HasColumnType("decimal(20,8)");

                entity.Property(e => e.PenaltyFeePercent)
                    .HasColumnName("penalty_fee_percent")
                    .HasColumnType("decimal(6,2)");

                entity.Property(e => e.PenaltyFixedAmount)
                    .HasColumnName("penalty_fixed_amount")
                    .HasColumnType("decimal(20,8)");

                entity.Property(e => e.PenaltyFixedPercent)
                    .HasColumnName("penalty_fixed_percent")
                    .HasColumnType("decimal(6,2)");

                entity.Property(e => e.PenaltyInterest)
                    .HasColumnName("penalty_interest")
                    .HasColumnType("decimal(6,2)");

                entity.Property(e => e.PenaltyTypeApplicable)
                    .HasColumnName("penalty_type_applicable")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PreDuration)
                    .HasColumnName("pre_duration")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PreEndDate)
                    .IsRequired()
                    .HasColumnName("pre_end_date")
                    .HasMaxLength(255);

                entity.Property(e => e.PreStartDate)
                    .IsRequired()
                    .HasColumnName("pre_start_date")
                    .HasMaxLength(255);

                entity.Property(e => e.PresaleCustomText)
                    .IsRequired()
                    .HasColumnName("presale_custom_text")
                    .HasColumnType("text");

                entity.Property(e => e.PresaleTermsAndConditions)
                    .IsRequired()
                    .HasColumnName("presale_terms_and_conditions")
                    .HasColumnType("text");

                entity.Property(e => e.Purpose).HasColumnName("purpose");

                entity.Property(e => e.PurposeSingleline)
                    .HasColumnName("purpose_singleline")
                    .HasMaxLength(50);

                entity.Property(e => e.RepaymentType)
                    .HasColumnName("repayment_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RiskIndustry).HasColumnName("risk_industry");

                entity.Property(e => e.RiskStrength).HasColumnName("risk_strength");

                entity.Property(e => e.RiskWeakness).HasColumnName("risk_weakness");

                entity.Property(e => e.RistrictedCountries)
                    .IsRequired()
                    .HasColumnName("ristricted_countries")
                    .HasMaxLength(255);

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TargetInterest)
                    .HasColumnName("target_interest")
                    .HasColumnType("decimal(6,2)");

                entity.Property(e => e.TermsAndConditions)
                    .IsRequired()
                    .HasColumnName("terms_and_conditions")
                    .HasColumnType("text");

                entity.Property(e => e.TokenType)
                    .HasColumnName("token_type")
                    .HasColumnType("int(1)");

                entity.Property(e => e.TotalDisbursed)
                    .HasColumnName("total_disbursed")
                    .HasColumnType("decimal(20,8)");

                entity.Property(e => e.TotalInterestPaid)
                    .HasColumnName("total_interest_paid")
                    .HasColumnType("decimal(20,8)");

                entity.Property(e => e.TotalPenaltiesPaid)
                    .HasColumnName("total_penalties_paid")
                    .HasColumnType("decimal(20,8)");

                entity.Property(e => e.TotalPrincipalRepaid)
                    .HasColumnName("total_principal_repaid")
                    .HasColumnType("decimal(20,8)");

                entity.Property(e => e.TransFees)
                    .HasColumnName("trans_fees")
                    .HasColumnType("decimal(20,8)");

                entity.Property(e => e.TransactionDataText)
                    .IsRequired()
                    .HasColumnName("transaction_data_text")
                    .HasColumnType("text");

                entity.Property(e => e.UrlTitle)
                    .IsRequired()
                    .HasColumnName("url_title")
                    .HasMaxLength(255);

                entity.Property(e => e.WalletAddress)
                    .HasColumnName("wallet_address")
                    .HasMaxLength(255);
                
                entity.Property(e => e.Language)
                    .HasColumnName("language");

                entity.HasOne(e => e.Creator).WithMany().HasForeignKey(e => e.BorrowerId);
            });
        }

        public override int SaveChanges()
        {
            foreach (var entity in ChangeTracker.Entries().Where(e => e.State == EntityState.Modified))
            {
                if(entity.Entity is VersionedRow){
                    var saveEntity = entity.Entity as VersionedRow;
                    saveEntity.OnSavingChanges();
                }
            }

            return base.SaveChanges();
        }
    }
}
