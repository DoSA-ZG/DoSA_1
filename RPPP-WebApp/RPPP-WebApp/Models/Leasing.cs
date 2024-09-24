using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Leasing
{
    public int Id { get; set; }

    public int PlotId { get; set; }

    public int OwnerId { get; set; }

    public int RentierId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual Person Owner { get; set; }

    public virtual Plot Plot { get; set; }

    public virtual Person Rentier { get; set; }
}
