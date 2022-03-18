#nullable enable
using System.Collections.Generic;
using Assignment4WC.Context.Models;
using Assignment4WC.Models;

namespace Assignment4WC.Context.Repositories.Member
{
    public interface IMembersRepository : ISaveChanges
    {
        void Add(Members member);
        bool Any();
        Members? GetMemberOrNull(string username);
        bool DoesUsernameExist(string username);
        List<UserScore> GetUserScoreInDescendingOrder();
    }
}