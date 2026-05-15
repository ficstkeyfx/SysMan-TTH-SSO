`# Get Database

Scaffold-DbContext "Data Source=6pg.org,1433; Initial Catalogs=dbLLKH; User ID=nghiand;Password=@NangHaNoi2020@" Microsoft.EntityFrameworkCore.SqlServer -OutputDir tmp -DataAnnotations -f -UseDatabaseNames

dotnet ef dbcontext scaffold "data source=172.17.10.157\\SQLExpress;initial catalog=dbLLKH;user id=admin;password=@NangHaNoi2020@;trustservercertificate=True" Microsoft.EntityFrameworkCore.SqlServer -o Models --use-database-names --data-annotations --force

protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
if (!optionsBuilder.IsConfigured)
{
IConfigurationRoot configuration = new ConfigurationBuilder()
.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
.AddJsonFile("appsettings.json")
.Build();
optionsBuilder.UseSqlServer(configuration.GetConnectionString("DBConnection"));
}
}
`