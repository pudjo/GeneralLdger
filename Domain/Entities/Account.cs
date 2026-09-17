using System.ComponentModel.DataAnnotations;

namespace Accounting.Domain.Entities
{
    public class Account
    {
        [Key]
        public string Id { get; set; }
        public int Root { get; set; }
        public string Name { get; set; } = String.Empty;

        public string IdParent { get; set; }
        public Single Leaf { get; set; }
        public int Debet { get; set; }

    }
}
