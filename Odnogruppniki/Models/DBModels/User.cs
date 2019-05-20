namespace Odnogruppniki.Models.DBModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            PersonalInfoes = new HashSet<PersonalInfo>();
            PersonalMessages = new HashSet<PersonalMessage>();
            PersonalMessages1 = new HashSet<PersonalMessage>();
        }

        public int id { get; set; }

        [Required]
        [StringLength(15)]
        public string login { get; set; }

        [Required]
        public string password { get; set; }

        public int? id_role { get; set; }

        public int? id_group { get; set; }

        public virtual Group Group { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonalInfo> PersonalInfoes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonalMessage> PersonalMessages { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonalMessage> PersonalMessages1 { get; set; }

        public virtual Role Role { get; set; }
    }
}
