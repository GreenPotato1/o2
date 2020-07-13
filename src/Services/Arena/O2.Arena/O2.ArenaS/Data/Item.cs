
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O2.ArenaS.Data
{
    public class Item
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Category { get; set; }
        public int Position { get; set; }
        public int Room { get; set; }
        public string RoomDescription { get; set; }
        public int RoomNumber { get; set; }
        public int SpecialNumber { get; set; }
        public int KeyCount { get; set; }
        public string Note { get; set; }
        public string RootType { get; set; }
    }
}