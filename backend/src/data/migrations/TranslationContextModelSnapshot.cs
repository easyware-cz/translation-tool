﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using src.data.db;

#nullable disable

namespace src.data.migrations
{
    [DbContext(typeof(TranslationContext))]
    partial class TranslationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("src.data.entities.Env", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("Envs");
                });

            modelBuilder.Entity("src.data.entities.Key", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int>("EnvId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EnvId");

                    b.ToTable("Keys");
                });

            modelBuilder.Entity("src.data.entities.Language", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("LanguageCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Languages");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            LanguageCode = "en"
                        },
                        new
                        {
                            Id = 2,
                            LanguageCode = "en-US"
                        },
                        new
                        {
                            Id = 3,
                            LanguageCode = "de"
                        },
                        new
                        {
                            Id = 4,
                            LanguageCode = "fr"
                        },
                        new
                        {
                            Id = 5,
                            LanguageCode = "es"
                        },
                        new
                        {
                            Id = 6,
                            LanguageCode = "it"
                        },
                        new
                        {
                            Id = 7,
                            LanguageCode = "cs"
                        },
                        new
                        {
                            Id = 8,
                            LanguageCode = "sk"
                        });
                });

            modelBuilder.Entity("src.data.entities.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("src.data.entities.Translation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("KeyId")
                        .HasColumnType("integer");

                    b.Property<int>("LanguageId")
                        .HasColumnType("integer");

                    b.Property<string>("TranslationText")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("KeyId");

                    b.HasIndex("LanguageId");

                    b.ToTable("Translations");
                });

            modelBuilder.Entity("src.data.entities.Env", b =>
                {
                    b.HasOne("src.data.entities.Project", "Project")
                        .WithMany("Env")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("src.data.entities.Key", b =>
                {
                    b.HasOne("src.data.entities.Env", "Env")
                        .WithMany("Key")
                        .HasForeignKey("EnvId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Env");
                });

            modelBuilder.Entity("src.data.entities.Translation", b =>
                {
                    b.HasOne("src.data.entities.Key", "Key")
                        .WithMany("Translations")
                        .HasForeignKey("KeyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("src.data.entities.Language", "Language")
                        .WithMany("Translations")
                        .HasForeignKey("LanguageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Key");

                    b.Navigation("Language");
                });

            modelBuilder.Entity("src.data.entities.Env", b =>
                {
                    b.Navigation("Key");
                });

            modelBuilder.Entity("src.data.entities.Key", b =>
                {
                    b.Navigation("Translations");
                });

            modelBuilder.Entity("src.data.entities.Language", b =>
                {
                    b.Navigation("Translations");
                });

            modelBuilder.Entity("src.data.entities.Project", b =>
                {
                    b.Navigation("Env");
                });
#pragma warning restore 612, 618
        }
    }
}
