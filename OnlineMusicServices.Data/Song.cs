//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineMusicServices.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Song
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Song()
        {
            this.Lyrics = new HashSet<Lyric>();
            this.SongComments = new HashSet<SongComment>();
            this.Albums = new HashSet<Album>();
            this.Playlists = new HashSet<Playlist>();
            this.Artists = new HashSet<Artist>();
        }
    
        public long Id { get; set; }
        public string Title { get; set; }
        public int GenreId { get; set; }
        public int AuthorId { get; set; }
        public string Composer { get; set; }
        public string ResourceId { get; set; }
        public System.DateTime UploadedDate { get; set; }
        public long Views { get; set; }
        public bool Verified { get; set; }
        public bool Privacy { get; set; }
    
        public virtual Genre Genre { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Lyric> Lyrics { get; set; }
        public virtual Resource Resource { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SongComment> SongComments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Album> Albums { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Playlist> Playlists { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Artist> Artists { get; set; }
    }
}