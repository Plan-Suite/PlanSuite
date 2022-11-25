﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlanSuite.Data;

#nullable disable

namespace PlanSuite.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20221120230105_GanttStartDate")]
    partial class GanttStartDate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("latin1_swedish_ci")
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("RoleId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Value")
                        .HasColumnType("longtext");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("FinishedFirstTimeLogin")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("PaymentExpiry")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("PaymentTier")
                        .HasColumnType("int");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext");

                    b.Property<string>("StripeCustomerId")
                        .HasColumnType("longtext");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.AuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<int>("LogCategory")
                        .HasColumnType("int")
                        .HasColumnName("log_category");

                    b.Property<int>("LogType")
                        .HasColumnType("int")
                        .HasColumnName("log_type");

                    b.Property<string>("TargetID")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("target_id");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("timestamp");

                    b.Property<Guid>("UserID")
                        .HasColumnType("char(36)")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.ToTable("audit_logs");
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.Card", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<Guid>("CardAssignee")
                        .HasColumnType("char(36)")
                        .HasColumnName("card_assignee");

                    b.Property<string>("CardDescription")
                        .HasColumnType("longtext")
                        .HasColumnName("card_description");

                    b.Property<DateTime?>("CardDueDate")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("card_due_date");

                    b.Property<int>("CardMilestone")
                        .HasColumnType("int")
                        .HasColumnName("card_milestone");

                    b.Property<string>("CardName")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("card_name");

                    b.Property<int>("CardPriority")
                        .HasColumnType("int")
                        .HasColumnName("card_priority");

                    b.Property<DateTime?>("CardStartDate")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("card_start_date");

                    b.Property<int>("ColumnId")
                        .HasColumnType("int")
                        .HasColumnName("column_id");

                    b.HasKey("Id");

                    b.ToTable("cards");
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.CardChecklist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<int>("ChecklistCard")
                        .HasColumnType("int")
                        .HasColumnName("checklist_card");

                    b.Property<string>("ChecklistName")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("checklist_name");

                    b.HasKey("Id");

                    b.ToTable("card_checklists");
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.ChangeLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Additions")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("additions");

                    b.Property<string>("Changes")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("changes");

                    b.Property<string>("Fixes")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("fixes");

                    b.Property<string>("VersionName")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("version_name");

                    b.HasKey("Id");

                    b.ToTable("change_log");
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.ChecklistItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<int>("ChecklistId")
                        .HasColumnType("int")
                        .HasColumnName("checklist_id");

                    b.Property<int>("ItemIndex")
                        .HasColumnType("int")
                        .HasColumnName("item_index");

                    b.Property<string>("ItemName")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("item_name");

                    b.Property<bool>("ItemTicked")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("item_ticked");

                    b.HasKey("Id");

                    b.ToTable("checklist_items");
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.Column", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int")
                        .HasColumnName("project_id");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("title");

                    b.HasKey("Id");

                    b.ToTable("columns");
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.JournalNote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("content");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("created");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("modified");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("name");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("char(36)")
                        .HasColumnName("owner_id");

                    b.HasKey("Id");

                    b.ToTable("journal_notes");
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.Organisation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("org_desc");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("org_name");

                    b.Property<int>("Tier")
                        .HasColumnType("int")
                        .HasColumnName("org_tier");

                    b.HasKey("Id");

                    b.ToTable("organisations");
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.OrganisationMembership", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<int>("OrganisationId")
                        .HasColumnType("int")
                        .HasColumnName("organisation_id");

                    b.Property<int>("Role")
                        .HasColumnType("int")
                        .HasColumnName("organisation_role");

                    b.Property<Guid>("UserId")
                        .HasColumnType("char(36)")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.ToTable("organisation_memberships");
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.PasswordResetRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("char(36)")
                        .HasColumnName("account_id");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("code");

                    b.Property<DateTime>("Expiry")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("expiry");

                    b.HasKey("Id");

                    b.ToTable("password_reset_requests");
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("created_date");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("description");

                    b.Property<DateTime?>("DueDate")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("due_date");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("name");

                    b.Property<int>("OrganisationId")
                        .HasColumnType("int")
                        .HasColumnName("org_id");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("char(36)")
                        .HasColumnName("owner_id");

                    b.HasKey("Id");

                    b.ToTable("projects");
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.ProjectAccess", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<DateTime>("AccessSince")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("access_since");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int")
                        .HasColumnName("project_id");

                    b.Property<int>("ProjectRole")
                        .HasColumnType("int")
                        .HasColumnName("project_role");

                    b.Property<Guid>("UserId")
                        .HasColumnType("char(36)")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.ToTable("projects_access");
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.ProjectMilestone", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("description");

                    b.Property<DateTime?>("DueDate")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("due_date");

                    b.Property<bool>("IsClosed")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("is_closed");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("last_updated");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int")
                        .HasColumnName("project_id");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("title");

                    b.HasKey("Id");

                    b.ToTable("project_milestones");
                });

            modelBuilder.Entity("PlanSuite.Models.Persistent.Sale", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("char(36)")
                        .HasColumnName("owner_id");

                    b.Property<int>("PaymentTier")
                        .HasColumnType("int")
                        .HasColumnName("payment_tier");

                    b.Property<DateTime>("SaleDate")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("sale_date");

                    b.Property<bool>("SaleIsFree")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("sale_is_free");

                    b.Property<int>("SaleState")
                        .HasColumnType("int")
                        .HasColumnName("sale_state");

                    b.HasKey("Id");

                    b.ToTable("sales");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("PlanSuite.Models.Persistent.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("PlanSuite.Models.Persistent.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PlanSuite.Models.Persistent.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("PlanSuite.Models.Persistent.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
