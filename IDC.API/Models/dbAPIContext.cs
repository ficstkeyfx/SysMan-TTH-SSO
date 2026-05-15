using Microsoft.EntityFrameworkCore;
using IDC.Shared.Models.SysMan;


#nullable disable

namespace api.Models
{
    public partial class dbAPIContext : DbContext
    {
        public dbAPIContext()
        {
        }

        public dbAPIContext(DbContextOptions<dbAPIContext> options)
            : base(options)
        {
        }
        public virtual DbSet<tbChucNang> tbChucNangs { get; set; }
        public virtual DbSet<tbChucNang_Nhom> tbChucNang_Nhoms { get; set; }
        public virtual DbSet<tbSoftwareUser> tbSoftwareUsers { get; set; }
        public virtual DbSet<tbSystemUser> tbSystemUsers { get; set; }
        public virtual DbSet<vSystemUser> vSystemUsers { get; set; }
        public virtual DbSet<tbNhom> tbNhoms { get; set; }
        public virtual DbSet<tbPhanMem> tbPhanMems { get; set; }
        public virtual DbSet<tbNguoiDung_Nhom> tbNguoiDung_Nhoms { get; set; }
        public virtual DbSet<tbSessionLogin> tbSessionLogins { get; set; }
        public virtual DbSet<vNguoiDungHeThong> vNguoiDungHeThongs { get; set; }
        public virtual DbSet<vNguoiDungPhanMem> vNguoiDungPhanMems { get; set; }
        public virtual DbSet<tbVaiTro> tbVaiTros { get; set; }
        public virtual DbSet<tbVaiTro_NguoiDung> tbVaiTro_NguoiDungs { get; set; }
        public virtual DbSet<tbPhanMem_NguoiDung> tbPhanMem_NguoiDungs { get; set; }
        public virtual DbSet<tbChucNang_VaiTro> tbChucNang_VaiTros { get; set; }

        // Unable to generate entity type for table 'dbo.Docs' since its primary key could not be scaffolded. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("dbConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<tbChucNang>(entity =>
            {
                entity.ToTable("tbChucNang");
                entity.HasKey(e => e.IdChucNang);
            });
            modelBuilder.Entity<tbChucNang_Nhom>(entity =>
            {
                entity.ToTable("tbChucNang_Nhom");
                entity.HasKey(e => e.IdChucNang_Nhom);
            });
            modelBuilder.Entity<tbSoftwareUser>(entity =>
            {
                entity.ToTable("tbSoftwareUser");
                entity.HasKey(e => e.IdSoftwareUser);
            });
            modelBuilder.Entity<tbNhom>(entity =>
            {
                entity.ToTable("tbNhom");
                entity.HasKey(e => e.IdNhom);
            });
            modelBuilder.Entity<tbNguoiDung_Nhom>(entity =>
            {
                entity.ToTable("tbNguoiDung_Nhom");
                entity.HasKey(e => e.IdNguoiDung_Nhom);
            });
            modelBuilder.Entity<tbPhanMem>(entity =>
            {
                entity.ToTable("tbPhanMem");
                entity.HasKey(e => e.IdPhanMem);
            });

            modelBuilder.Entity<tbSystemUser>(entity =>
            {
                entity.ToTable("tbSystemUser");
                entity.HasKey(e => e.IdUser);
            });
            modelBuilder.Entity<vSystemUser>(entity =>
            {
                entity.ToTable("vSystemUser");
                entity.HasKey(e => e.IdUser);
            });
            modelBuilder.Entity<tbSessionLogin>(entity =>
            {
                entity.ToTable("tbSessionLogin");
                entity.HasKey(e => e.Token);
            });
            modelBuilder.Entity<vNguoiDungHeThong>(entity =>
            {
                entity.ToTable("vNguoiDungHeThong");
                entity.HasKey(e => e.IdUser);
            });
            modelBuilder.Entity<vNguoiDungPhanMem>(entity =>
            {
                entity.ToTable("vNguoiDungPhanMem");
                entity.HasKey(e => e.IdUser);
            });
            modelBuilder.Entity<tbVaiTro>(entity =>
            {
                entity.ToTable("tbVaiTro");
                entity.HasKey(e => e.IdVaiTro);
            });
            modelBuilder.Entity<tbVaiTro_NguoiDung>(entity =>
            {
                entity.ToTable("tbVaiTro_NguoiDung");
                entity.HasKey(e => e.IdVaiTro_NguoiDung);
            });
            modelBuilder.Entity<tbChucNang_VaiTro>(entity =>
            {
                entity.ToTable("tbChucNang_VaiTro");
                entity.HasKey(e => e.IdChucNang_VaiTro);
            });
            modelBuilder.Entity<tbPhanMem_NguoiDung>(entity =>
            {
                entity.ToTable("tbPhanMem_NguoiDung");
                entity.HasKey(e => e.IdPhanMem_NguoiDung);
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
