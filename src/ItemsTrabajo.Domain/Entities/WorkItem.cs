
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ItemsTrabajo.Domain.Entities;

[Table("tbl_work_items")]
public class WorkItem
{
    [Key]
    [Column("id_wi")]
    public int IdWi { get; set; }

    [Column("code_wi")]
    [Unicode(false)]
    public string? CodeWi { get; set; }

    [Column("description_wi")]
    [Unicode(false)]
    public string? DescriptionWi { get; set; }

    [Column("status_wi")]
    [StringLength(1)]
    [Unicode(false)]
    public string? StatusWi { get; set; }

    [Column("relevance")]
    [StringLength(1)]
    [Unicode(false)]
    public string? Relevance { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("expiration_date")]
    public DateTime? ExpirationDate { get; set; }

    public virtual ICollection<UserWork> UserWorks { get; set; } = new List<UserWork>();
}
