﻿using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.Builder.CodeBuilders.Models;

public class Return
{
    public bool IsEnumerable { get; set; }
    public bool IsVoid { get; set; }
    public string Name { get; set; }
    public string PgName { get; set; }
    public List<PgReturns> Record { get; set; } = null;
}
