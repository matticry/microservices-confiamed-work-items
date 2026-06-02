using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ItemsTrabajo.Domain.Entities;

[Table("tbl_user_work")]
public class UserWork
{
    [Key]
    [Column("id_u_w")]
    public int IdUW { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("item_id")]
    public int? ItemId { get; set; }

    [Column("status")]
    [StringLength(1)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column("assignment_date")]
    public DateTime? AssignmentDate { get; set; }

    [Column("completion_date")]
    public DateTime? CompletionDate { get; set; }

    [Column("order_priority")]
    public int? OrderPriority { get; set; }

    [ForeignKey("ItemId")]
    public virtual WorkItem? Item { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
