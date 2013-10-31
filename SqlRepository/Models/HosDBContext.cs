using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using SqlRepository.Models.Mapping;

namespace SqlRepository.Models
{
	public partial class HosDBContext : DbContext
	{
		static HosDBContext()
		{
			Database.SetInitializer<HosDBContext>(null);
		}

		public HosDBContext(string connectionString = "")
			: base(connectionString)
		{
		}

		public DbSet<C__RefactorLog> C__RefactorLog { get; set; }
		public DbSet<DriverSummary> DriverSummaries { get; set; }
		public DbSet<DriverWorkstate> DriverWorkstates { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Configurations.Add(new C__RefactorLogMap());
			modelBuilder.Configurations.Add(new DriverSummaryMap());
			modelBuilder.Configurations.Add(new DriverWorkstateMap());
		}
	}
}
