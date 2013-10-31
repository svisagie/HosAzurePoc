using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SqlRepository.Models.Mapping
{
    public class DriverSummaryMap : EntityTypeConfiguration<DriverSummary>
    {
        public DriverSummaryMap()
        {
            // Primary Key
            this.HasKey(t => t.DriverId);

            // Properties
            this.Property(t => t.DriverId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("DriverSummaries");
            this.Property(t => t.DriverId).HasColumnName("DriverId");
            this.Property(t => t.WorkStateId).HasColumnName("WorkStateId");
            this.Property(t => t.TotalSeconds).HasColumnName("TotalSeconds");
        }
    }
}
