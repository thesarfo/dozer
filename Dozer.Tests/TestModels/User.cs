using System;
using Dozer.Core.Attributes;

namespace Dozer.Core.Tests.TestModels;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Column("UserName", "NVARCHAR(100)")]
    public string Username { get; set; }

    [Column("EmailAddress")]
    public string Email { get; set; }

    public DateTime CreatedAt { get; set; }

    [Column(dbType: "DECIMAL(10,2)")]
    public decimal Balance { get; set; }
} 