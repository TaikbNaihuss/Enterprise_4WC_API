#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Assignment4WC.Context.Models;
using Assignment4WC.Models;

namespace Assignment4WC.Context.Repositories.Member
{
    public class MembersRepository : IMembersRepository
    {
        private readonly AssignmentContext _context;

        public MembersRepository(AssignmentContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Add(Members member) =>
            _context.Members.Add(member);

        public bool Any() =>
            _context.Members.Any();
        
        public Members? GetMemberOrNull(string username) =>
            _context.Members.FirstOrDefault(members => members.Username == username);

        public bool DoesUsernameExist(string username) =>
            _context.Members.Any(members => members.Username == username);

        public List<UserScore> GetUserScoreInDescendingOrder() =>
            _context.Members.OrderByDescending(members => members.UserScore)
                .Select(members => new UserScore(members.Username, members.UserScore))
                .ToList();

        public void SaveChanges() => 
            _context.SaveChanges();
    }
}