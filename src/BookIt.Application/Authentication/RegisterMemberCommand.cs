namespace BookIt.Application.Authentication;

public sealed record RegisterMemberCommand(string Name, string Email, string Password);
