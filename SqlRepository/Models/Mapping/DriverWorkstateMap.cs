using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SqlRepository.Models.Mapping
{
    public class DriverWorkstateMap : EntityTypeConfiguration<DriverWorkstate>
    {
        public DriverWorkstateMap()
        {
            // Primary Key
            this.HasKey(t => t.DriverId);

            // Properties
            this.Property(t => t.DriverId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("DriverWorkstates");
            this.Property(t => t.DriverId).HasColumnName("DriverId");
            this.Property(t => t.WorkStateId).HasColumnName("WorkStateId");
            this.Property(t => t.Timestamp).HasColumnName("Timestamp");
        }
    }
}
