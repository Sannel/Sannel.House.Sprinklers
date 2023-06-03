﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sannel.House.Sprinklers.Infrastructure;

#nullable disable

namespace Sannel.House.Sprinklers.Infrastructure.Migrations
{
    [DbContext(typeof(SprinklerDbContext))]
    [Migration("20230525014644_AddProgramAndRun")]
    partial class AddProgramAndRun
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("Sannel.House.Sprinklers.Core.Models.StationLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("ActionDate")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan?>("RunLength")
                        .HasColumnType("TEXT");

                    b.Property<byte>("StationId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("RunLog");
                });

            modelBuilder.Entity("Sannel.House.Sprinklers.Core.Schedules.Models.ScheduleProgram", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Enabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ScheduleCron")
                        .HasColumnType("TEXT");

                    b.Property<string>("StationTimes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Programs");
                });

            modelBuilder.Entity("Sannel.House.Sprinklers.Core.Schedules.Models.ZoneRun", b =>
                {
                    b.Property<DateTime>("StartDateTime")
                        .HasColumnType("TEXT");

                    b.Property<byte>("ZoneId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Ran")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("RunLength")
                        .HasColumnType("TEXT");

                    b.HasKey("StartDateTime", "ZoneId");

                    b.ToTable("Runs");
                });
#pragma warning restore 612, 618
        }
    }
}