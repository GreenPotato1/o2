﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using O2.Certificate.Data;

namespace O2.Certificate.Data.Migrations
{
    [DbContext(typeof(O2BusinessDataContext))]
    partial class O2BusinessDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("O2.Certificate.Data.Models.O2C.O2CCertificate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("AddedDate")
                        .HasColumnName("added_date")
                        .HasColumnType("bigint");

                    b.Property<long?>("DateOfCert")
                        .HasColumnName("date_of_cert")
                        .HasColumnType("bigint");

                    b.Property<string>("Education")
                        .HasColumnName("education")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Firstname")
                        .HasColumnName("firstname")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<string>("Lastname")
                        .HasColumnName("lastname")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<bool?>("Lock")
                        .HasColumnName("lock")
                        .HasColumnType("bit");

                    b.Property<string>("Middlename")
                        .HasColumnName("middlename")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<long>("ModifiedDate")
                        .HasColumnName("modified_date")
                        .HasColumnType("bigint");

                    b.Property<string>("Number")
                        .HasColumnName("number")
                        .HasColumnType("nvarchar(10)")
                        .HasMaxLength(10);

                    b.Property<string>("Serial")
                        .HasColumnName("serial")
                        .HasColumnType("nvarchar(1)")
                        .HasMaxLength(1);

                    b.Property<int>("ShortNumber")
                        .HasColumnName("short_number")
                        .HasColumnType("int");

                    b.Property<bool?>("Visible")
                        .HasColumnName("visible")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("O2CCertificate");
                });

            modelBuilder.Entity("O2.Certificate.Data.Models.O2C.O2CCertificateLocation", b =>
                {
                    b.Property<Guid>("O2CLocationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("O2CCertificateId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("O2CLocationId", "O2CCertificateId");

                    b.HasIndex("O2CCertificateId");

                    b.ToTable("O2CCertificateLocation");
                });

            modelBuilder.Entity("O2.Certificate.Data.Models.O2C.O2CContact", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("AddedDate")
                        .HasColumnName("added_date")
                        .HasColumnType("bigint");

                    b.Property<string>("Key")
                        .HasColumnName("contact_key")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("ModifiedDate")
                        .HasColumnName("modified_date")
                        .HasColumnType("bigint");

                    b.Property<Guid?>("O2CCertificateId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Value")
                        .HasColumnName("contact_value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("O2CCertificateId");

                    b.ToTable("O2CContact");
                });

            modelBuilder.Entity("O2.Certificate.Data.Models.O2C.O2CLocation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("AddedDate")
                        .HasColumnName("added_date")
                        .HasColumnType("bigint");

                    b.Property<string>("Country")
                        .HasColumnName("country")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<long>("ModifiedDate")
                        .HasColumnName("modified_date")
                        .HasColumnType("bigint");

                    b.Property<string>("Region")
                        .HasColumnName("region")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.HasKey("Id");

                    b.ToTable("O2CLocation");
                });

            modelBuilder.Entity("O2.Certificate.Data.Models.O2C.O2CPhoto", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnName("id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("O2CCertificateId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("AddedDate")
                        .HasColumnName("added_date")
                        .HasColumnType("bigint");

                    b.Property<string>("FileName")
                        .HasColumnName("fileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsMain")
                        .HasColumnName("isMain")
                        .HasColumnType("bit");

                    b.Property<long>("ModifiedDate")
                        .HasColumnName("modified_date")
                        .HasColumnType("bigint");

                    b.Property<string>("Url")
                        .HasColumnName("url")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id", "O2CCertificateId");

                    b.HasIndex("O2CCertificateId");

                    b.ToTable("O2CPhoto");
                });

            modelBuilder.Entity("O2.Certificate.Data.Models.O2Ev.O2EvEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("AddedDate")
                        .HasColumnName("added_date")
                        .HasColumnType("bigint");

                    b.Property<bool>("AllDay")
                        .HasColumnName("all_day")
                        .HasColumnType("bit");

                    b.Property<long>("EndDate")
                        .HasColumnName("end_date")
                        .HasColumnType("bigint");

                    b.Property<long>("ModifiedDate")
                        .HasColumnName("modified_date")
                        .HasColumnType("bigint");

                    b.Property<string>("ShortDescription")
                        .HasColumnName("short_description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("StartDate")
                        .HasColumnName("start_date")
                        .HasColumnType("bigint");

                    b.Property<string>("Title")
                        .HasColumnName("title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("O2EvEvent");
                });

            modelBuilder.Entity("O2.Certificate.Data.Models.O2Ev.O2EvMeta", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("AddedDate")
                        .HasColumnName("added_date")
                        .HasColumnType("bigint");

                    b.Property<Guid>("EventId")
                        .HasColumnName("event_id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LocationCountry")
                        .HasColumnName("country")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LocationRegion")
                        .HasColumnName("region")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("ModifiedDate")
                        .HasColumnName("modified_date")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("EventId")
                        .IsUnique();

                    b.ToTable("O2EvMeta");
                });

            modelBuilder.Entity("O2.Certificate.Data.Models.O2Ev.O2EvPhoto", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("AddedDate")
                        .HasColumnName("added_date")
                        .HasColumnType("bigint");

                    b.Property<string>("FileName")
                        .HasColumnName("fileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsMain")
                        .HasColumnName("isMain")
                        .HasColumnType("bit");

                    b.Property<long>("ModifiedDate")
                        .HasColumnName("modified_date")
                        .HasColumnType("bigint");

                    b.Property<Guid>("O2EvEventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Url")
                        .HasColumnName("url")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("O2EvEventId");

                    b.ToTable("O2EvPhoto");
                });

            modelBuilder.Entity("O2.Certificate.Data.Models.O2C.O2CCertificateLocation", b =>
                {
                    b.HasOne("O2.Certificate.Data.Models.O2C.O2CCertificate", "O2CCertificate")
                        .WithMany("Locations")
                        .HasForeignKey("O2CCertificateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("O2.Certificate.Data.Models.O2C.O2CLocation", "O2CLocation")
                        .WithMany("O2CCertificateLocation")
                        .HasForeignKey("O2CLocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("O2.Certificate.Data.Models.O2C.O2CContact", b =>
                {
                    b.HasOne("O2.Certificate.Data.Models.O2C.O2CCertificate", "O2CCertificate")
                        .WithMany("Contacts")
                        .HasForeignKey("O2CCertificateId");
                });

            modelBuilder.Entity("O2.Certificate.Data.Models.O2C.O2CPhoto", b =>
                {
                    b.HasOne("O2.Certificate.Data.Models.O2C.O2CCertificate", "O2CCertificate")
                        .WithMany("Photos")
                        .HasForeignKey("O2CCertificateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("O2.Certificate.Data.Models.O2Ev.O2EvMeta", b =>
                {
                    b.HasOne("O2.Certificate.Data.Models.O2Ev.O2EvEvent", "O2EvEvent")
                        .WithOne("Meta")
                        .HasForeignKey("O2.Certificate.Data.Models.O2Ev.O2EvMeta", "EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("O2.Certificate.Data.Models.O2Ev.O2EvPhoto", b =>
                {
                    b.HasOne("O2.Certificate.Data.Models.O2Ev.O2EvEvent", "O2EvEvent")
                        .WithMany("Photos")
                        .HasForeignKey("O2EvEventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
