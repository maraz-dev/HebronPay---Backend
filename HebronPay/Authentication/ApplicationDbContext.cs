using HebronPay.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HebronPay.Authentication
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {


        }
        public DbSet<OTP> OTPs { get; set; }
        public DbSet<HebronPayWallet> HebronPayWallets { get; set; }
        public DbSet<HebronPayTransaction> HebronPayTransactions { get; set; }
        public DbSet<SubAccount> SubAccounts { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }



    }
}
