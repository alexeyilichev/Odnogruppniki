namespace Odnogruppniki.Models.DBModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PersonalInfo")]
    public partial class PersonalInfo
    {
        public int id { get; set; }

        [Required]
        [StringLength(30)]
        public string name { get; set; }

        public string aboutinfo { get; set; }

        [StringLength(30)]
        public string city { get; set; }

        public int id_university { get; set; }

        public int id_faculty { get; set; }

        public int id_department { get; set; }

        public int id_role { get; set; }

        public int id_group { get; set; }

        public int id_user { get; set; }

        public string photo { get; set; }

        [StringLength(20)]
        public string phone { get; set; }

        public virtual Department Department { get; set; }

        public virtual Faculty Faculty { get; set; }

        public virtual Group Group { get; set; }

        public virtual Role Role { get; set; }

        public virtual University University { get; set; }

        public virtual User User { get; set; }
    }
}
