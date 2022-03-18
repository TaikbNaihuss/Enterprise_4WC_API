using Assignment4WC.Context.Repositories;
using Assignment4WC.Context.Repositories.Categories;
using Assignment4WC.Context.Repositories.ComplexQuestions;
using Assignment4WC.Context.Repositories.Locations;
using Assignment4WC.Context.Repositories.Member;
using Assignment4WC.Context.Repositories.Questions;
using Assignment4WC.Logic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Assignment4WC.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((_, services) =>
                {
                    services
                        .AddTransient<IFourWeekChallengeManager, FourWeekChallengeManager>()
                        .AddTransient<IQuestionRandomiser, QuestionRandomiser>()
                        .AddTransient<IGlobalRepository, GlobalRepository>()
                        .AddTransient<IComplexQuestionsRepository, ComplexQuestionsRepository>()
                        .AddTransient<IQuestionsRepository, QuestionsRepository>()
                        .AddTransient<IMembersRepository, MembersRepository>()
                        .AddTransient<ILocationsRepository, LocationsRepository>()
                        .AddTransient<ICategoriesRepository, CategoriesRepository>();
                });
    }
}
