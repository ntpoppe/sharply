using Sharply.Client.Models;
using System.Threading.Tasks;

namespace Sharply.Client.Interfaces;

public interface ICurrentUserService
{
    CurrentUser? CurrentUser { get; }
    Task<CurrentUser> InitializeUser(string token);
    void ClearUser();
}
