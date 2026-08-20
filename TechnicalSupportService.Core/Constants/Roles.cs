namespace TechnicalSupportService.Core.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Engineer = "Engineer";
    public const string Manager = "Manager";
    public const string Applicant = "Applicant";

    public static readonly string[] All = { Admin, Engineer, Manager, Applicant };
}
