namespace Bootstrapper
{
    public class DatabaseConfigurator
    {
        public string ConnectionString { get; } = default!;
        public string EntityFrameworkUsingOption { get; } = default!;
        public string EntityFrameworkDefaultSchema { get; } = default!;
        public string EntityFrameworkIdentityNamingConventions { get; } = default!;
        public DatabaseConfigurator(string dbprovider,
            string dbserver,
            string dbname,
            string dbuser,
            string dbpass)
            {
                switch (dbprovider)
                {
                    case "sqlserver":
                        ConnectionString = $"Server={dbserver};Database={dbname};User Id={dbuser};Password={dbpass};TrustServerCertificate=True";
                        EntityFrameworkUsingOption = @"UseSqlServer(""name=ConnectionStrings:DefaultConnection""));";
                        EntityFrameworkDefaultSchema = @"builder.HasDefaultSchema(""dbo"");";
                        EntityFrameworkIdentityNamingConventions = "";
                        break;
                    case "postgresql":
                        ConnectionString = $"Host={dbserver};Database={dbname};Username={dbuser};Password={dbpass}";
                        EntityFrameworkUsingOption = @"UseNpgsql(""name=ConnectionStrings:DefaultConnection"")
            .UseSnakeCaseNamingConvention());";
                        EntityFrameworkDefaultSchema = @"builder.HasDefaultSchema(""public"");";
                        EntityFrameworkIdentityNamingConventions = @"builder.Entity<IdentityUser>().ToTable(""asp_net_users"");
            builder.Entity<IdentityUserToken<string>>().ToTable(""asp_net_user_tokens"");
            builder.Entity<IdentityUserLogin<string>>().ToTable(""asp_net_user_logins"");
            builder.Entity<IdentityUserClaim<string>>().ToTable(""asp_net_user_claims"");
            builder.Entity<IdentityRole>().ToTable(""asp_net_roles"");
            builder.Entity<IdentityUserRole<string>>().ToTable(""asp_net_user_roles"");
            builder.Entity<IdentityRoleClaim<string>>().ToTable(""asp_net_role_claims"");
";
                        break;
                    case "sqlite":
                        throw new NotImplementedException($"SQL provider '{dbprovider}' not implemented");
                        // ConnectionString = $"...";
                        // EntityFrameworkUsingOption = $"...";
                        // break;
                    default:
                        throw new Exception($"Unsupported SQL provider '{dbprovider}'");
                }


            }
    }
}