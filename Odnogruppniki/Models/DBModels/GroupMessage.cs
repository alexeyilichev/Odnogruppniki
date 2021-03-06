namespace Odnogruppniki.Models.DBModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GroupMessage")]
    public partial class GroupMessage
    {
        public int id { get; set; }

        public int id_out { get; set; }

        public int id_in { get; set; }

        public DateTime date { get; set; }

        public string message { get; set; }

        public virtual Group Group { get; set; }

        public virtual Group Group1 { get; set; }
    }
}
