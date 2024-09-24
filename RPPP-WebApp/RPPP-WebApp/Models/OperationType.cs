using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class OperationType
{
    public int Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Operation> Operations { get; set; } = new List<Operation>();
}
