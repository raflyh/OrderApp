namespace UserService.GraphQL
{
    public record RegisterUser
    (
        string Email,
        string Username,
        string Fullname,
        string Password
    );
}
