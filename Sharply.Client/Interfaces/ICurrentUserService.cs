using System.Threading.Tasks;
using Sharply.Client.Models;

namespace Sharply.Client.Interfaces;

public interface ICurrentUserService
{
    CurrentUser? CurrentUser { get; }
    Task<CurrentUser> InitializeUser(string token);
    void ClearUser();
}
