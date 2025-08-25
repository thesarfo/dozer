using Dozer.Core.Attributes;

namespace Dozer.Sample.Models;

[Table("Products")]
public class Product
{
    [Key]
    public int Id { get; set; }

    [Column("ProductName", "TEXT")]
    public string Name { get; set; }

    [Column("Description", "TEXT")]
    public string Description { get; set; }

    [Column("Price", "DECIMAL(10,2)")]
    public decimal Price { get; set; }

    [Column("IsActive", "BOOLEAN")]
    public bool IsActive { get; set; }

    [Column("CreatedDate", "DATETIME")]
    public DateTime CreatedDate { get; set; }

    [Column("CategoryId", "INTEGER")]
    public int CategoryId { get; set; }
}
