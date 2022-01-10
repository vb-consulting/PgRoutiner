namespace PgRoutiner.DataAccess.Models;

public class ConstraintName
{
    public string Schema { get; set; }
    public string Table { get; set; }
    public string Name { get; set; }
    public PgConstraint Type { get; set; }
}
