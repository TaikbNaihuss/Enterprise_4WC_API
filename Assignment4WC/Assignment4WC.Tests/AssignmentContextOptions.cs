using Assignment4WC.Context;
using Microsoft.EntityFrameworkCore;

namespace Assignment4WC.Tests
{
    internal static class AssignmentContextOptions
    {
        public static DbContextOptions<AssignmentContext> GetDbOptions() =>
            new DbContextOptionsBuilder<AssignmentContext>()
                .UseInMemoryDatabase("AssignmentDatabase")
                .Options;
    }
}