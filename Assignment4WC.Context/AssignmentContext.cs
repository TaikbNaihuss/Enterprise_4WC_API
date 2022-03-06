using System;
using Assignment4WC.Context.Models;
using Assignment4WC.Models;
using Microsoft.EntityFrameworkCore;

namespace Assignment4WC.Context
{
    public class AssignmentContext : DbContext
    {
        public DbSet<Questions> Questions { get; set; }
        public DbSet<ComplexQuestions> ComplexQuestions { get; set; }
        public DbSet<Answers> Answers { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Locations> Locations { get; set; }
        public DbSet<Members> Members { get; set; }


        public AssignmentContext(DbContextOptions<AssignmentContext> contextOptions) : base(contextOptions) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Questions>(builder =>
            {
                builder.HasKey(answers => answers.QuestionId);

                builder.HasDiscriminator()
                    .HasValue<Questions>(QuestionComplexity.Simple.ToString())
                    .HasValue<ComplexQuestions>(QuestionComplexity.Complex.ToString());

                builder.HasMany(questions => questions.Answers)
                    .WithOne(answers => answers.Question);

                builder.HasOne(questions => questions.Category)
                    .WithMany(categories => categories.Questions);

                builder.Property(questions => questions.QuestionType)
                    .HasConversion(
                        s => s.ToString(),
                        s => (QuestionType)Enum.Parse(typeof(QuestionType), s));
            });


            modelBuilder.Entity<ComplexQuestions>(builder =>
            {
                builder.HasOne(questions => questions.Location)
                    .WithOne(locations => locations.Question);

                builder.HasBaseType<Questions>();
            });
               

            modelBuilder.Entity<Answers>(builder =>
            {
                builder.HasKey(answers => answers.AnswerId);

                builder.HasOne(answers => answers.Question)
                    .WithMany(questions => questions.Answers)
                    .HasForeignKey(answers => answers.QuestionId);
            });

            modelBuilder.Entity<Categories>(builder =>
            {
               builder.HasKey(answers => answers.CategoryId);

               builder.HasMany(categories => categories.Questions)
                   .WithOne(questions => questions.Category)
                   .HasForeignKey(questions => questions.CategoryId);

               builder.Property(categories => categories.CategoryName)
                   .HasConversion(
                       s => s.ToString(), 
                       s => (CategoryType)Enum.Parse(typeof(CategoryType), s));
            });

            modelBuilder.Entity<Locations>(builder =>
            {
                builder.HasKey(locations => locations.LocationId);

                builder.HasOne(locations => locations.Question)
                    .WithOne(questions => questions.Location)
                    .HasForeignKey<ComplexQuestions>(questions => questions.LocationId);
            });

            modelBuilder.Entity<Members>(builder =>
            {
                builder.HasKey(members => members.MemberId);
            });
        }
    }
}