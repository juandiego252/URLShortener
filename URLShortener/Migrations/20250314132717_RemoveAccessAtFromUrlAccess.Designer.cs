﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using URLShortener.Infrastructure.Database;

#nullable disable

namespace URLShortener.Migrations
{
    [DbContext(typeof(StoreContext))]
    [Migration("20250314132717_RemoveAccessAtFromUrlAccess")]
    partial class RemoveAccessAtFromUrlAccess
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("URLShortener.Models.ShortenedUrl", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccessCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastAccessedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("OriginalUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShortCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ShortenedUrls");
                });

            modelBuilder.Entity("URLShortener.Models.UrlAccess", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("AccessedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("ShortenedUrlId")
                        .HasColumnType("int");

                    b.Property<string>("UserAgent")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ShortenedUrlId");

                    b.ToTable("UrlAccesses");
                });

            modelBuilder.Entity("URLShortener.Models.UrlAccess", b =>
                {
                    b.HasOne("URLShortener.Models.ShortenedUrl", "ShortenedUrl")
                        .WithMany("AccessLogs")
                        .HasForeignKey("ShortenedUrlId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ShortenedUrl");
                });

            modelBuilder.Entity("URLShortener.Models.ShortenedUrl", b =>
                {
                    b.Navigation("AccessLogs");
                });
#pragma warning restore 612, 618
        }
    }
}
