using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Operation
{
    public int Id { get; set; }

    public int PlantId { get; set; }

    public int OperationTypeId { get; set; }

    public int RequestId { get; set; }

    public int Cost { get; set; }

    public bool Status { get; set; }

    public DateTime Date { get; set; }

    public int Amount { get; set; }

    public virtual OperationType OperationType { get; set; }

    public virtual Plant Plant { get; set; }

    public virtual Request Request { get; set; }
}
