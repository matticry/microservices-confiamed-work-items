using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ItemsTrabajo.Domain.Entities;

[Table("tbl_user")]
public class User
{
    [Key]
    [Column("id_us")]
    public int IdUs { get; set; }

    [Column("username_us")]
    [Unicode(false)]
    public string? UsernameUs { get; set; }

    [Column("status_us")]
    [StringLength(1)]
    [Unicode(false)]
    public string? StatusUs { get; set; }

    [Column("create_at")]
    public DateTime? CreateAt { get; set; }

    public virtual ICollection<UserWork> UserWorks { get; set; } = new List<UserWork>();
}
