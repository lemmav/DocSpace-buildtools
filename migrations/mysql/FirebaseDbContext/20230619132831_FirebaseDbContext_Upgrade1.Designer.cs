// <auto-generated />
using System;
using ASC.Core.Common.EF.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ASC.Migrations.MySql.Migrations.FirebaseDb
{
    [DbContext(typeof(FirebaseDbContext))]
    [Migration("20230619132831_FirebaseDbContext_Upgrade1")]
    partial class FirebaseDbContextUpgrade1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("ASC.Core.Common.EF.FireBaseUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Application")
                        .HasColumnType("varchar(20)")
                        .HasColumnName("application")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<string>("FirebaseDeviceToken")
                        .HasColumnType("varchar(255)")
                        .HasColumnName("firebase_device_token")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<bool?>("IsSubscribed")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("is_subscribed");

                    b.Property<int>("TenantId")
                        .HasColumnType("int")
                        .HasColumnName("tenant_id");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(36)")
                        .HasColumnName("user_id")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.HasIndex("TenantId", "UserId")
                        .HasDatabaseName("user_id");

                    b.ToTable("firebase_users", (string)null);
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbTenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasColumnName("alias")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<bool>("Calls")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("calls")
                        .HasDefaultValueSql("'1'");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("datetime")
                        .HasColumnName("creationdatetime");

                    b.Property<int>("Industry")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("industry")
                        .HasDefaultValueSql("'0'");

                    b.Property<string>("Language")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(10)")
                        .HasColumnName("language")
                        .HasDefaultValueSql("'en-US'")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("timestamp")
                        .HasColumnName("last_modified");

                    b.Property<string>("MappedDomain")
                        .HasColumnType("varchar(100)")
                        .HasColumnName("mappeddomain")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)")
                        .HasColumnName("name")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<string>("OwnerId")
                        .HasColumnType("varchar(38)")
                        .HasColumnName("owner_id")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<string>("PaymentId")
                        .HasColumnType("varchar(38)")
                        .HasColumnName("payment_id")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<bool>("Spam")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("spam")
                        .HasDefaultValueSql("'1'");

                    b.Property<int>("Status")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("status")
                        .HasDefaultValueSql("'0'");

                    b.Property<DateTime?>("StatusChanged")
                        .HasColumnType("datetime")
                        .HasColumnName("statuschanged");

                    b.Property<string>("TimeZone")
                        .HasColumnType("varchar(50)")
                        .HasColumnName("timezone")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<int>("TrustedDomainsEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("trusteddomainsenabled")
                        .HasDefaultValueSql("'1'");

                    b.Property<string>("TrustedDomainsRaw")
                        .HasColumnType("varchar(1024)")
                        .HasColumnName("trusteddomains")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<int>("Version")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("version")
                        .HasDefaultValueSql("'2'");

                    b.Property<DateTime?>("Version_Changed")
                        .HasColumnType("datetime")
                        .HasColumnName("version_changed");

                    b.HasKey("Id");

                    b.HasIndex("Alias")
                        .IsUnique()
                        .HasDatabaseName("alias");

                    b.HasIndex("LastModified")
                        .HasDatabaseName("last_modified");

                    b.HasIndex("MappedDomain")
                        .HasDatabaseName("mappeddomain");

                    b.HasIndex("Version")
                        .HasDatabaseName("version");

                    b.ToTable("tenants_tenants", null, t =>
                        {
                            t.ExcludeFromMigrations();
                        });

                    b.HasAnnotation("MySql:CharSet", "utf8");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Alias = "localhost",
                            Calls = false,
                            CreationDateTime = new DateTime(2021, 3, 9, 17, 46, 59, 97, DateTimeKind.Utc).AddTicks(4317),
                            Industry = 0,
                            LastModified = new DateTime(2022, 7, 8, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Name = "Web Office",
                            OwnerId = "66faa6e4-f133-11ea-b126-00ffeec8b4ef",
                            Spam = false,
                            Status = 0,
                            TrustedDomainsEnabled = 0,
                            Version = 0
                        },
                        new
                        {
                            Id = -1,
                            Alias = "settings",
                            Calls = false,
                            CreationDateTime = new DateTime(2021, 3, 9, 17, 46, 59, 97, DateTimeKind.Utc).AddTicks(4317),
                            Industry = 0,
                            LastModified = new DateTime(2022, 7, 8, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Name = "Web Office",
                            OwnerId = "00000000-0000-0000-0000-000000000000",
                            Spam = false,
                            Status = 1,
                            TrustedDomainsEnabled = 0,
                            Version = 0
                        });
                });

            modelBuilder.Entity("ASC.Core.Common.EF.FireBaseUser", b =>
                {
                    b.HasOne("ASC.Core.Common.EF.Model.DbTenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");
                });
#pragma warning restore 612, 618
        }
    }
}
