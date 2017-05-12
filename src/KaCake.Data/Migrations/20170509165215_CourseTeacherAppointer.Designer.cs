using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using KaCake.Data;
using KaCake.Data.Models;

namespace KaCake.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20170509165215_CourseTeacherAppointer")]
    partial class CourseTeacherAppointer
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("KaCake.Data.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("KaCake.Data.Models.Assignment", b =>
                {
                    b.Property<int>("TaskVariantId");

                    b.Property<string>("UserId");

                    b.Property<DateTime>("DeadlineUtc");

                    b.Property<bool>("IsChecked");

                    b.Property<string>("ReviewerId");

                    b.Property<double>("Score");

                    b.Property<int>("Status");

                    b.HasKey("TaskVariantId", "UserId");

                    b.HasIndex("ReviewerId");

                    b.HasIndex("UserId");

                    b.ToTable("Assignments");
                });

            modelBuilder.Entity("KaCake.Data.Models.Course", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.ToTable("Courses");
                });

            modelBuilder.Entity("KaCake.Data.Models.CourseEnrollment", b =>
                {
                    b.Property<int>("CourseId");

                    b.Property<string>("UserId");

                    b.HasKey("CourseId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("CourseEnrollment");
                });

            modelBuilder.Entity("KaCake.Data.Models.CourseTeacher2", b =>
                {
                    b.Property<int>("CourseId");

                    b.Property<string>("TeacherId");

                    b.Property<string>("AppointerId");

                    b.HasKey("CourseId", "TeacherId");

                    b.HasIndex("AppointerId");

                    b.HasIndex("TeacherId");

                    b.ToTable("CourseTeacher2");
                });

            modelBuilder.Entity("KaCake.Data.Models.Submission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AssignmentTaskVariantId")
                        .IsRequired();

                    b.Property<string>("AssignmentUserId")
                        .IsRequired();

                    b.Property<string>("Path");

                    b.Property<DateTime>("Time");

                    b.HasKey("Id");

                    b.HasIndex("AssignmentTaskVariantId", "AssignmentUserId");

                    b.ToTable("Submissions");
                });

            modelBuilder.Entity("KaCake.Data.Models.TaskGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CourseId");

                    b.Property<string>("Description");

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.ToTable("TaskGroups");
                });

            modelBuilder.Entity("KaCake.Data.Models.TaskVariant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<int>("TaskGroupId");

                    b.HasKey("Id");

                    b.HasIndex("TaskGroupId");

                    b.ToTable("TaskVariants");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("KaCake.Data.Models.Assignment", b =>
                {
                    b.HasOne("KaCake.Data.Models.ApplicationUser", "Reviewer")
                        .WithMany()
                        .HasForeignKey("ReviewerId");

                    b.HasOne("KaCake.Data.Models.TaskVariant", "TaskVariant")
                        .WithMany("Assignments")
                        .HasForeignKey("TaskVariantId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("KaCake.Data.Models.ApplicationUser", "User")
                        .WithMany("Assignments")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KaCake.Data.Models.CourseEnrollment", b =>
                {
                    b.HasOne("KaCake.Data.Models.Course", "Course")
                        .WithMany("Students")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("KaCake.Data.Models.ApplicationUser", "User")
                        .WithMany("Courses")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KaCake.Data.Models.CourseTeacher2", b =>
                {
                    b.HasOne("KaCake.Data.Models.ApplicationUser", "Appointer")
                        .WithMany()
                        .HasForeignKey("AppointerId");

                    b.HasOne("KaCake.Data.Models.Course", "Course")
                        .WithMany("Teachers")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("KaCake.Data.Models.ApplicationUser", "Teacher")
                        .WithMany("TeachingCourses")
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KaCake.Data.Models.Submission", b =>
                {
                    b.HasOne("KaCake.Data.Models.Assignment", "Assignment")
                        .WithMany("Submissions")
                        .HasForeignKey("AssignmentTaskVariantId", "AssignmentUserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KaCake.Data.Models.TaskGroup", b =>
                {
                    b.HasOne("KaCake.Data.Models.Course", "Course")
                        .WithMany("TaskGroups")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KaCake.Data.Models.TaskVariant", b =>
                {
                    b.HasOne("KaCake.Data.Models.TaskGroup", "TaskGroup")
                        .WithMany("Variants")
                        .HasForeignKey("TaskGroupId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("KaCake.Data.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("KaCake.Data.Models.ApplicationUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("KaCake.Data.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
